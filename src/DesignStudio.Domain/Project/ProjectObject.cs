using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public abstract class ProjectObject
{
    protected ProjectObject(EntityId? id = null)
    {
        Id = id ?? EntityId.New();
    }

    public EntityId Id { get; }
    public DesignObjectMetadata Metadata { get; } = new();
    public ObjectCategory Category => Metadata.Category;
    public EntityId? ParentId => Metadata.ParentId;
    public Placement2D Placement => Metadata.Placement;
    public abstract string Type { get; }

    public void SetCategory(ObjectCategory category) => Metadata.SetCategory(category);
    public void SetParent(EntityId? parentId) => Metadata.SetParent(parentId);
    public void SetLayer(EntityId? layerId) => Metadata.SetLayer(layerId);
    public void SetPlacement(Placement2D placement) => Metadata.SetPlacement(placement);
    public void SetVisibility(bool isVisible) => Metadata.SetVisibility(isVisible);
    public void SetLocked(bool isLocked) => Metadata.SetLocked(isLocked);
}
