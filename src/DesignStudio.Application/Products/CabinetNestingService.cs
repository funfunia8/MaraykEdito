using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Application.Products;

/// <summary>
/// Deterministic sheet-packing foundation. This is intentionally not a production CNC optimizer.
/// It provides repeatable placement, grain/rotation constraints, kerf spacing, and conservative
/// candidate-remnant reporting so a dedicated optimizer can replace it later behind the same contract.
/// </summary>
public sealed class CabinetNestingService
{
    private sealed record PackItem(CabinetCutListLine Line, int ItemIndex);

    private sealed class RowState
    {
        public RowState(double y, double height) { Y = y; Height = height; }
        public double Y { get; }
        public double Height { get; }
        public double NextX { get; set; }
    }

    public NestingResult Build(Project project, CabinetCutList cutList, NestingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(cutList);
        options ??= NestingOptions.Default;

        var groups = cutList.Lines
            .Where(x => x.Quantity > 0)
            .GroupBy(x => x.MaterialId)
            .OrderBy(x => x.Key?.Value);

        var sheets = new List<NestingSheet>();
        var unplaced = new List<CabinetCutListLine>();

        foreach (var group in groups)
        {
            if (group.Key is null)
            {
                unplaced.AddRange(group);
                continue;
            }

            if (!project.TryGet(group.Key.Value, out var materialObject) || materialObject is not Material material)
            {
                unplaced.AddRange(group);
                continue;
            }

            if (material.Kind != MaterialKind.Sheet || material.SheetWidth.Millimeters <= 0 || material.SheetHeight.Millimeters <= 0)
            {
                unplaced.AddRange(group);
                continue;
            }

            var packItems = group
                .SelectMany(line => Enumerable.Range(0, line.Quantity).Select(i => new PackItem(line, i + 1)))
                .OrderByDescending(x => x.Line.Width.Millimeters * x.Line.Height.Millimeters)
                .ThenBy(x => x.Line.PartType)
                .ThenBy(x => x.Line.Name)
                .ToList();

            var materialSheets = new List<NestingSheetBuilder>();
            foreach (var item in packItems)
            {
                var placed = false;
                foreach (var sheet in materialSheets)
                {
                    if (TryPlace(sheet, material, item, options))
                    {
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    var sheet = new NestingSheetBuilder(materialSheets.Count, material, options);
                    if (TryPlace(sheet, material, item, options))
                    {
                        materialSheets.Add(sheet);
                    }
                    else
                    {
                        unplaced.Add(item.Line);
                    }
                }
            }

            sheets.AddRange(materialSheets.Select(x => x.Build()));
        }

        return new NestingResult(sheets, unplaced);
    }

    private static bool TryPlace(NestingSheetBuilder sheet, Material material, PackItem item, NestingOptions options)
    {
        var choices = GetOrientationChoices(item.Line, material, options);
        foreach (var choice in choices)
        {
            if (sheet.TryPlace(item, choice.Width, choice.Height, choice.Rotated90Degrees, options.Kerf.Millimeters))
                return true;
        }
        return false;
    }

    private static IReadOnlyList<(double Width, double Height, bool Rotated90Degrees)> GetOrientationChoices(
        CabinetCutListLine line,
        Material material,
        NestingOptions options)
    {
        var original = (line.Width.Millimeters, line.Height.Millimeters, false);
        var rotated = (line.Height.Millimeters, line.Width.Millimeters, true);

        if (!options.AllowRotation || material.GrainDirection == MaterialGrainDirection.None)
            return options.AllowRotation ? new[] { original, rotated } : new[] { original };

        // Directional sheet grain: preserve the source part's grain axis relative to the sheet axis.
        var partGrainIsWidth = line.GrainAlongWidth;
        var sheetGrainIsWidth = material.GrainDirection == MaterialGrainDirection.AlongWidth;
        return partGrainIsWidth == sheetGrainIsWidth ? new[] { original } : new[] { rotated };
    }

    private sealed class NestingSheetBuilder
    {
        private readonly List<NestingPlacement> _placements = new();
        private readonly List<RowState> _rows = new();
        private readonly Material _material;
        private readonly NestingOptions _options;
        private readonly double _usableLeft;
        private readonly double _usableTop;
        private readonly double _usableRight;
        private readonly double _usableBottom;
        private readonly double _usableWidth;
        private readonly double _usableHeight;

        public NestingSheetBuilder(int sheetIndex, Material material, NestingOptions options)
        {
            SheetIndex = sheetIndex;
            _material = material;
            _options = options;
            _usableLeft = options.SheetMargin.Millimeters;
            _usableTop = options.SheetMargin.Millimeters;
            _usableRight = material.SheetWidth.Millimeters - options.SheetMargin.Millimeters;
            _usableBottom = material.SheetHeight.Millimeters - options.SheetMargin.Millimeters;
            _usableWidth = _usableRight - _usableLeft;
            _usableHeight = _usableBottom - _usableTop;
        }

        public int SheetIndex { get; }

        public bool TryPlace(PackItem item, double width, double height, bool rotated90, double kerf)
        {
            if (width <= 0 || height <= 0 || width > _usableWidth || height > _usableHeight)
                return false;

            foreach (var row in _rows)
            {
                var x = row.NextX;
                if (x + width <= _usableRight + 0.0001 && row.Y + height <= _usableBottom + 0.0001)
                {
                    _placements.Add(new NestingPlacement(
                        item.Line.PartType,
                        item.Line.Name,
                        item.Line.MaterialId,
                        Length.FromMillimeters(x),
                        Length.FromMillimeters(row.Y),
                        Length.FromMillimeters(width),
                        Length.FromMillimeters(height),
                        rotated90,
                        item.ItemIndex,
                        item.Line.EdgeBanding));
                    row.NextX = x + width + kerf;
                    return true;
                }
            }

            var nextY = _rows.Count == 0
                ? _usableTop
                : _rows.Max(x => x.Y + x.Height) + kerf;

            if (nextY + height > _usableBottom + 0.0001)
                return false;

            var newRow = new RowState(nextY, height) { NextX = _usableLeft + width + kerf };
            _rows.Add(newRow);
            _placements.Add(new NestingPlacement(
                item.Line.PartType,
                item.Line.Name,
                item.Line.MaterialId,
                Length.FromMillimeters(_usableLeft),
                Length.FromMillimeters(nextY),
                Length.FromMillimeters(width),
                Length.FromMillimeters(height),
                rotated90,
                item.ItemIndex,
                item.Line.EdgeBanding));
            return true;
        }

        public NestingSheet Build()
        {
            var remnants = new List<NestingRemnant>();
            foreach (var row in _rows)
            {
                var rightWidth = _usableRight - row.NextX + _options.Kerf.Millimeters;
                if (rightWidth >= _options.MinimumRemnantWidth.Millimeters && row.Height >= _options.MinimumRemnantHeight.Millimeters)
                {
                    remnants.Add(new NestingRemnant(
                        Length.FromMillimeters(row.NextX - _options.Kerf.Millimeters),
                        Length.FromMillimeters(row.Y),
                        Length.FromMillimeters(rightWidth),
                        Length.FromMillimeters(row.Height),
                        rightWidth * row.Height / 1_000_000.0));
                }
            }

            var usedHeight = _rows.Count == 0 ? 0 : _rows.Max(x => x.Y + x.Height);
            var bottomHeight = _usableBottom - usedHeight;
            if (bottomHeight >= _options.MinimumRemnantHeight.Millimeters && _usableWidth >= _options.MinimumRemnantWidth.Millimeters)
            {
                remnants.Add(new NestingRemnant(
                    Length.FromMillimeters(_usableLeft),
                    Length.FromMillimeters(usedHeight + _options.Kerf.Millimeters),
                    Length.FromMillimeters(_usableWidth),
                    Length.FromMillimeters(Math.Max(0, bottomHeight - _options.Kerf.Millimeters)),
                    _usableWidth * Math.Max(0, bottomHeight - _options.Kerf.Millimeters) / 1_000_000.0));
            }

            return new NestingSheet(
                SheetIndex,
                _material.Id,
                _material.Code,
                _material.SheetWidth,
                _material.SheetHeight,
                _placements.ToList(),
                remnants);
        }
    }
}
