using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Validation;

namespace DesignStudio.Application.Products;

public sealed class CabinetManufacturingRulesService
{
    public ValidationResult Validate(Project project, Cabinet cabinet, CabinetHardwarePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(cabinet);

        var result = new ValidationResult();
        var hardwarePlan = new CabinetHardwarePlanningService().Build(project, cabinet, policy);

        if (cabinet.DoorCount > 0 && !hardwarePlan.Lines.Any(x => x.Kind == HardwareKind.Hinge))
            result.Error("HINGE_RULE_MISSING", "A cabinet with doors must have a hinge rule producing at least one hinge.");

        if (cabinet.ShelfCount > 0 && !hardwarePlan.Lines.Any(x => x.Kind == HardwareKind.ShelfPin))
            result.Error("SHELF_SUPPORT_RULE_MISSING", "Shelves require a shelf-support hardware rule.");

        foreach (var line in hardwarePlan.Lines.Where(x => x.CatalogItemId is null))
            result.Warning("HARDWARE_CATALOG_UNASSIGNED", $"Hardware code {line.Code} is not present in the project hardware catalog; a generic specification will be used.");

        foreach (var warning in hardwarePlan.Warnings)
            result.Warning("HARDWARE_RULE_WARNING", warning);

        return result;
    }
}
