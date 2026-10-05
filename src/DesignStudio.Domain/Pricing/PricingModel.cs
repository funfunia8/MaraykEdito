using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Pricing;

public enum PricingCategory
{
    Material,
    Furniture,
    Woodwork,
    Hardware,
    Glass,
    Metal,
    Stone,
    Gypsum,
    Flooring,
    WallFinish,
    Paint,
    Lighting,
    Labor,
    Manufacturing,
    Installation,
    Transport,
    Subcontract,
    Waste,
    Other
}

public sealed record PricingItem(
    EntityId Id,
    string Code,
    string Description,
    PricingCategory Category,
    decimal Quantity,
    string Unit,
    decimal UnitCost,
    decimal WastePercent = 0,
    decimal LaborCost = 0,
    decimal InstallationCost = 0,
    EntityId? SourceObjectId = null,
    EntityId? SourceMaterialId = null)
{
    public decimal ExtendedMaterialCost => Quantity * UnitCost * (1 + WastePercent / 100m);
    public decimal TotalCost => ExtendedMaterialCost + LaborCost + InstallationCost;
}
