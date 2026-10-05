using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Domain.Validation;

namespace DesignStudio.Application.Products;

public sealed record CabinetGenerationResult(
    CabinetManufacturingSpec ManufacturingSpec,
    IReadOnlyList<CabinetPart> Parts,
    ValidationResult Validation);

public sealed class CabinetParametricService
{
    private const double GrooveClearanceMm = 0.30;
    private const double RearGrooveOffsetMm = 20.0;
    private const double MinimumCabinetClearanceMm = 100.0;

    public CabinetGenerationResult Generate(Cabinet cabinet)
    {
        ArgumentNullException.ThrowIfNull(cabinet);
        var validation = Validate(cabinet);
        if (!validation.IsValid)
            return new CabinetGenerationResult(CreateSpec(cabinet), Array.Empty<CabinetPart>(), validation);

        var spec = CreateSpec(cabinet);
        var parts = new List<CabinetPart>();
        var carcassDepth = cabinet.Depth;
        var panel = cabinet.PanelThickness;

        parts.Add(new CabinetPart(
            CabinetPartType.LeftSide, "Left Side",
            panel, cabinet.Height, carcassDepth, panel, 1,
            EdgeBandFlags.Front, GrainAlongWidth: false,
            cabinet.CarcassMaterialId, cabinet.EdgeBandMaterialId));

        parts.Add(new CabinetPart(
            CabinetPartType.RightSide, "Right Side",
            panel, cabinet.Height, carcassDepth, panel, 1,
            EdgeBandFlags.Front, GrainAlongWidth: false,
            cabinet.CarcassMaterialId, cabinet.EdgeBandMaterialId));

        var clearWidth = cabinet.Width.Millimeters - (2 * panel.Millimeters);
        var horizontalDepth = cabinet.Depth;

        parts.Add(new CabinetPart(
            CabinetPartType.Top, "Top",
            Length.FromMillimeters(clearWidth), horizontalDepth, panel, panel, 1,
            EdgeBandFlags.Front, GrainAlongWidth: true,
            cabinet.CarcassMaterialId, cabinet.EdgeBandMaterialId));

        parts.Add(new CabinetPart(
            CabinetPartType.Bottom, "Bottom",
            Length.FromMillimeters(clearWidth), horizontalDepth, panel, panel, 1,
            EdgeBandFlags.Front, GrainAlongWidth: true,
            cabinet.CarcassMaterialId, cabinet.EdgeBandMaterialId));

        parts.Add(new CabinetPart(
            CabinetPartType.Back, "Back",
            spec.BackPartWidth, spec.BackPartHeight, cabinet.BackThickness, cabinet.BackThickness, 1,
            EdgeBandFlags.None, GrainAlongWidth: true,
            cabinet.BackMaterialId, null));

        if (cabinet.ShelfCount > 0)
        {
            var shelfHeight = Length.FromMillimeters(Math.Max(
                0,
                (cabinet.Height.Millimeters - (2 * panel.Millimeters)) / (cabinet.ShelfCount + 1)));

            parts.Add(new CabinetPart(
                CabinetPartType.Shelf, "Shelf",
                Length.FromMillimeters(clearWidth),
                shelfHeight,
                horizontalDepth,
                panel,
                cabinet.ShelfCount,
                EdgeBandFlags.Front,
                GrainAlongWidth: true,
                cabinet.CarcassMaterialId, cabinet.EdgeBandMaterialId));
        }

        if (cabinet.DoorCount > 0)
        {
            var doorWidth = Length.FromMillimeters(clearWidth / cabinet.DoorCount);
            parts.Add(new CabinetPart(
                CabinetPartType.Door, "Door",
                doorWidth,
                Length.FromMillimeters(Math.Max(0, cabinet.Height.Millimeters - (2 * panel.Millimeters))),
                panel,
                panel,
                cabinet.DoorCount,
                EdgeBandFlags.Left | EdgeBandFlags.Right | EdgeBandFlags.Top | EdgeBandFlags.Bottom,
                GrainAlongWidth: false,
                cabinet.DoorMaterialId ?? cabinet.CarcassMaterialId, cabinet.EdgeBandMaterialId));
        }

        return new CabinetGenerationResult(spec, parts, validation);
    }

    public ValidationResult Validate(Cabinet cabinet)
    {
        var result = new ValidationResult();
        var minPanelSpace = (2 * cabinet.PanelThickness.Millimeters) + MinimumCabinetClearanceMm;

        if (cabinet.Width.Millimeters < minPanelSpace)
            result.Error("CABINET_WIDTH_TOO_SMALL", "Cabinet width is too small for the selected panel thickness.");
        if (cabinet.Height.Millimeters < minPanelSpace)
            result.Error("CABINET_HEIGHT_TOO_SMALL", "Cabinet height is too small for the selected panel thickness.");
        if (cabinet.Depth.Millimeters <= cabinet.BackThickness.Millimeters)
            result.Error("CABINET_DEPTH_TOO_SMALL", "Cabinet depth must be greater than back thickness.");

        if (cabinet.BackThickness.Millimeters > cabinet.PanelThickness.Millimeters)
            result.Warning("BACK_THICKER_THAN_PANEL", "Back thickness is greater than carcass panel thickness; verify construction method.");

        if (cabinet.CarcassMaterialId is null)
            result.Warning("CARCASS_MATERIAL_UNASSIGNED", "Carcass material is not assigned yet.");
        if (cabinet.BackMaterialId is null)
            result.Warning("BACK_MATERIAL_UNASSIGNED", "Back material is not assigned yet.");

        if (cabinet.ConstructionMethod == CabinetConstructionMethod.CapturedBackGroove && cabinet.Depth.Millimeters < RearGrooveOffsetMm)
            result.Error("GROOVE_DEPTH_INVALID", "Captured back groove requires at least 20 mm cabinet depth.");

        return result;
    }

    private static CabinetManufacturingSpec CreateSpec(Cabinet cabinet)
    {
        var grooveWidth = cabinet.BackThickness.Millimeters + GrooveClearanceMm;
        var grooveDepth = Math.Max(4.0, Math.Min(10.0, cabinet.BackThickness.Millimeters + 2.0));
        var grooveOffset = cabinet.ConstructionMethod == CabinetConstructionMethod.CapturedBackGroove
            ? Math.Min(RearGrooveOffsetMm, Math.Max(0, cabinet.Depth.Millimeters - grooveDepth))
            : 0.0;

        var backWidth = cabinet.Width.Millimeters - (2 * cabinet.PanelThickness.Millimeters) + (2 * grooveDepth);
        var backHeight = cabinet.Height.Millimeters - (2 * cabinet.PanelThickness.Millimeters) + (2 * grooveDepth);

        return new CabinetManufacturingSpec(
            Length.FromMillimeters(grooveWidth),
            Length.FromMillimeters(grooveDepth),
            Length.FromMillimeters(grooveOffset),
            Length.FromMillimeters(backWidth),
            Length.FromMillimeters(backHeight));
    }
}
