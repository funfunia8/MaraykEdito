using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Manufacturing;

public sealed record ManufacturingDefinition(
    EntityId Id,
    EntityId SourceDesignObjectId,
    string Code,
    string Revision,
    IReadOnlyList<ManufacturingPart> Parts,
    IReadOnlyList<ManufacturingHardware> Hardware,
    IReadOnlyList<ManufacturingOperation> Operations,
    IReadOnlyList<ManufacturingAssembly> Assemblies,
    EntityId? ProductDefinitionId = null);

[Flags]
public enum ManufacturingEdgeFlags
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
    Front = 16,
    Back = 32
}

public sealed record ManufacturingPart(
    string Code,
    string Name,
    Length Width,
    Length Height,
    Length Thickness,
    int Quantity,
    EntityId? MaterialId = null,
    EntityId? EdgeBandMaterialId = null,
    bool GrainAlongWidth = false,
    string? SourceComponent = null,
    Length? Depth = null,
    ManufacturingEdgeFlags EdgeBanding = ManufacturingEdgeFlags.None);

public sealed record ManufacturingHardware(
    string Code,
    string Name,
    int Quantity,
    EntityId? CatalogItemId = null);

public sealed record ManufacturingOperation(
    string Code,
    string Name,
    string OperationType,
    IReadOnlyList<string> PartCodes,
    IReadOnlyDictionary<string, string>? Parameters = null);

public sealed record ManufacturingAssembly(
    string Code,
    string Name,
    IReadOnlyList<string> PartCodes,
    IReadOnlyList<string> HardwareCodes);