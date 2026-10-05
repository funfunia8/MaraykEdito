using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Project;

public sealed class Room : ProjectObject
{
    private readonly List<EntityId> _wallIds = new();

    public Room(string name, EntityId? id = null, EntityId? floorId = null) : base(id)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Room" : name;
        SetCategory(ObjectCategory.Architecture);
        FloorId = floorId;
        SetParent(floorId);
    }

    public override string Type => "Room";
    public string Name { get; private set; }
    public Length Height { get; private set; } = Length.FromMillimeters(2700);
    public EntityId? FloorId { get; private set; }

    public IReadOnlyList<EntityId> WallIds => _wallIds;

    public void SetHeight(Length height)
    {
        if (height.Millimeters <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Height = height;
    }

    public void AddWall(EntityId wallId)
    {
        if (!_wallIds.Contains(wallId))
            _wallIds.Add(wallId);
    }
}
