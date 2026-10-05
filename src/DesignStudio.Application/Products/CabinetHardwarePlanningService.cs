using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Products;

public sealed class CabinetHardwarePlanningService
{
    public CabinetHardwarePlan Build(
        Project project,
        Cabinet cabinet,
        CabinetHardwarePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(cabinet);

        var rules = policy ?? CabinetHardwarePolicy.Default;
        var lines = new List<HardwarePlanLine>();
        var warnings = new List<string>();

        if (cabinet.DoorCount > 0)
        {
            var hingesPerDoor = cabinet.Height.Millimeters <= 900
                ? rules.HingesForDoorHeightUpTo900Mm
                : cabinet.Height.Millimeters <= 1800
                    ? rules.HingesForDoorHeightUpTo1800Mm
                    : rules.HingesForDoorHeightAbove1800Mm;

            AddLine(lines, project, "HINGE-110", "Concealed hinge 110°", HardwareKind.Hinge, HardwareUnit.Piece, cabinet.DoorCount * hingesPerDoor, "HINGE_COUNT_BY_DOOR_HEIGHT");
            AddLine(lines, project, "HANDLE-STD", "Standard cabinet handle", HardwareKind.Handle, HardwareUnit.Piece, cabinet.DoorCount * rules.HandlesPerDoor, "HANDLE_PER_DOOR");
        }

        if (cabinet.ShelfCount > 0)
            AddLine(lines, project, "SHELF-PIN-5", "Shelf support pin 5 mm", HardwareKind.ShelfPin, HardwareUnit.Piece, cabinet.ShelfCount * rules.ShelfPinsPerShelf, "SHELF_PINS_PER_SHELF");

        AddLine(lines, project, "SCREW-CARCASS", "Carcass assembly screw", HardwareKind.Screw, HardwareUnit.Piece, rules.CarcassScrewsPerCabinet, "CARCASS_SCREW_POLICY");

        if (lines.Count == 0)
            warnings.Add("NO_HARDWARE_REQUIRED_BY_CURRENT_RULES");

        return new CabinetHardwarePlan(lines, warnings);
    }

    private static void AddLine(
        List<HardwarePlanLine> lines,
        Project project,
        string code,
        string name,
        HardwareKind kind,
        HardwareUnit unit,
        int quantity,
        string ruleCode)
    {
        if (quantity <= 0) return;

        EntityId? id = null;
        var catalog = project.Objects
            .OfType<HardwareItem>()
            .FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));

        if (catalog is not null) id = catalog.Id;

        lines.Add(new HardwarePlanLine(code, catalog?.Name ?? name, kind, catalog?.Unit ?? unit, quantity, id, ruleCode));
    }
}
