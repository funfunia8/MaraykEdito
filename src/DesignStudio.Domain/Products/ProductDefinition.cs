using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Products;

public sealed record ProductDefinition(
    EntityId Id,
    string Code,
    string Name,
    ProductCategory Category,
    EntityId? DesignObjectId = null,
    string? Manufacturer = null,
    string? Model = null);
