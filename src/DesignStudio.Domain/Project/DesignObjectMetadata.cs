using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public sealed class DesignObjectMetadata
{
    public ObjectCategory Category { get; private set; } = ObjectCategory.Reference;
    public EntityId? ParentId { get; private set; }
    public EntityId? LayerId { get; private set; }
    public Placement2D Placement { get; private set; } = Placement2D.Origin;
    public bool IsVisible { get; private set; } = true;
    public bool IsLocked { get; private set; }

    public void SetCategory(ObjectCategory category) => Category = category;

    public void SetParent(EntityId? parentId) => ParentId = parentId;

    public void SetLayer(EntityId? layerId) => LayerId = layerId;

    public void SetPlacement(Placement2D placement)
    {
        ArgumentNullException.ThrowIfNull(placement);
        Placement = placement.Normalize();
    }

    public void SetVisibility(bool isVisible) => IsVisible = isVisible;

    public void SetLocked(bool isLocked) => IsLocked = isLocked;
}
