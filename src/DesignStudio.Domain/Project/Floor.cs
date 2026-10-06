using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public sealed class Floor : ProjectObject
{
    public Floor(string name, EntityId? buildingId = null, EntityId? id = null) : base(id)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Floor" : name.Trim();
        SetCategory(ObjectCategory.Architecture);
        SetParent(buildingId);
    }

    public override string Type => "Floor";
    public string Name { get; private set; }
    public EntityId? BuildingId => ParentId;
}
