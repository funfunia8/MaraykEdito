using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public sealed class Building : ProjectObject
{
    public Building(string name, EntityId? id = null) : base(id)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Building" : name.Trim();
        SetCategory(ObjectCategory.Architecture);
    }

    public override string Type => "Building";
    public string Name { get; private set; }
}
