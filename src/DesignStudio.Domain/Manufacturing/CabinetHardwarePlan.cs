using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Manufacturing;

public sealed record HardwarePlanLine(
    string Code,
    string Name,
    HardwareKind Kind,
    HardwareUnit Unit,
    int Quantity,
    EntityId? CatalogItemId,
    string RuleCode);

public sealed record CabinetHardwarePlan(
    IReadOnlyList<HardwarePlanLine> Lines,
    IReadOnlyList<string> Warnings);

public sealed record CabinetHardwarePolicy(
    int HingesForDoorHeightUpTo900Mm,
    int HingesForDoorHeightUpTo1800Mm,
    int HingesForDoorHeightAbove1800Mm,
    int ShelfPinsPerShelf,
    int HandlesPerDoor,
    int CarcassScrewsPerCabinet)
{
    public static CabinetHardwarePolicy Default { get; } =
        new(2, 3, 4, 4, 1, 8);
}
