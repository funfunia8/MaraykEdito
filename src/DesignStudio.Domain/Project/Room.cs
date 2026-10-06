using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Project;

public sealed class Room : ProjectObject
{
    private readonly RoomBoundary _boundary = new();

    public Room(
        string name,
        EntityId? id = null,
        EntityId? floorId = null) : base(id)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Room" : name;
        SetCategory(ObjectCategory.Architecture);
        SetParent(floorId);
    }

    public override string Type => "Room";

    public string Name { get; private set; }

    public Length Height { get; private set; } =
        Length.FromMillimeters(2700);

    public EntityId? FloorId => ParentId;

    public RoomBoundary Boundary => _boundary;

    // Compatibility projection for the existing project/storage API.
    public IReadOnlyList<EntityId> WallIds =>
        _boundary.OuterLoop.WallIds;

    public void SetHeight(Length height)
    {
        if (height.Millimeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        Height = height;
    }

    public void AddWall(EntityId wallId)
    {
        _boundary.OuterLoop.AddWall(wallId);
    }

    public void AddBoundaryEdge(BoundaryEdge edge)
    {
        _boundary.AddOuterEdge(edge);
    }
}
