using System.Globalization;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;
using DesignStudio.Manufacturing;

namespace DesignStudio.Application.Products;

public sealed class CabinetManufacturingDefinitionService
{
    private readonly IManufacturingDefinitionService _definitionService =
        new ManufacturingDefinitionService();

    public ManufacturingDefinition Build(
        Project project,
        Cabinet cabinet,
        CabinetGenerationResult generation,
        string code,
        string revision = "A",
        CabinetHardwarePlan? hardwarePlan = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(cabinet);
        ArgumentNullException.ThrowIfNull(generation);

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Manufacturing code is required.",
                nameof(code));

        var definition = _definitionService.Build(
            project,
            cabinet,
            code,
            revision,
            cabinet.ProductDefinitionId);

        // Never publish manufacturing output from an invalid parametric result.
        if (!generation.Validation.IsValid)
            return definition;

        var parts = generation.Parts
            .Select(MapPart)
            .ToList();

        var hardwareSource =
            hardwarePlan ??
            new CabinetHardwarePlanningService().Build(
                project,
                cabinet);

        var hardware = hardwareSource.Lines
            .Where(x => x.Quantity > 0)
            .Select(x => new ManufacturingHardware(
                x.Code,
                x.Name,
                x.Quantity,
                x.CatalogItemId))
            .ToList();

        var operations = BuildOperations(cabinet, generation);

        var assembly = new ManufacturingAssembly(
            "CABINET-ASSEMBLY",
            "Cabinet Assembly",
            parts
                .Select(x => x.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            hardware
                .Select(x => x.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList());

        return definition with
        {
            Parts = parts,
            Hardware = hardware,
            Operations = operations,
            Assemblies = new[] { assembly }
        };
    }

    private static ManufacturingPart MapPart(CabinetPart part)
    {
        ArgumentNullException.ThrowIfNull(part);

        return new ManufacturingPart(
            Code: GetPartCode(part.Type),
            Name: part.Name,
            Width: part.Width,
            Height: part.Height,
            Thickness: part.Thickness,
            Quantity: part.Quantity,
            MaterialId: part.MaterialId,
            EdgeBandMaterialId: part.EdgeBandMaterialId,
            GrainAlongWidth: part.GrainAlongWidth,
            SourceComponent: part.Type.ToString(),
            Depth: part.Depth,
            EdgeBanding: MapEdgeBanding(part.EdgeBanding));
    }

    private static IReadOnlyList<ManufacturingOperation> BuildOperations(
        Cabinet cabinet,
        CabinetGenerationResult generation)
    {
        if (cabinet.ConstructionMethod !=
            CabinetConstructionMethod.CapturedBackGroove)
        {
            return Array.Empty<ManufacturingOperation>();
        }

        var spec = generation.ManufacturingSpec;

        var parameters = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["widthMm"] = Format(spec.GrooveWidth.Millimeters),
            ["depthMm"] = Format(spec.GrooveDepth.Millimeters),
            ["offsetFromRearMm"] = Format(spec.GrooveOffsetFromRear.Millimeters)
        };

        return new[]
        {
            new ManufacturingOperation(
                "BACK-GROOVE",
                "Captured back groove",
                "Groove",
                parameters)
        };
    }

    private static ManufacturingEdgeFlags MapEdgeBanding(
        EdgeBandFlags flags)
    {
        var result = ManufacturingEdgeFlags.None;

        if (flags.HasFlag(EdgeBandFlags.Left))
            result |= ManufacturingEdgeFlags.Left;

        if (flags.HasFlag(EdgeBandFlags.Right))
            result |= ManufacturingEdgeFlags.Right;

        if (flags.HasFlag(EdgeBandFlags.Top))
            result |= ManufacturingEdgeFlags.Top;

        if (flags.HasFlag(EdgeBandFlags.Bottom))
            result |= ManufacturingEdgeFlags.Bottom;

        if (flags.HasFlag(EdgeBandFlags.Front))
            result |= ManufacturingEdgeFlags.Front;

        if (flags.HasFlag(EdgeBandFlags.Back))
            result |= ManufacturingEdgeFlags.Back;

        return result;
    }

    private static string GetPartCode(CabinetPartType type)
        => type switch
        {
            CabinetPartType.LeftSide => "SIDE-L",
            CabinetPartType.RightSide => "SIDE-R",
            CabinetPartType.Top => "TOP",
            CabinetPartType.Bottom => "BOTTOM",
            CabinetPartType.Back => "BACK",
            CabinetPartType.Shelf => "SHELF",
            CabinetPartType.Door => "DOOR",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unsupported cabinet part type.")
        };

    private static string Format(double value)
        => value.ToString(
            "0.###",
            CultureInfo.InvariantCulture);
}