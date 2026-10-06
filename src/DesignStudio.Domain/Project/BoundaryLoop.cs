using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public sealed class BoundaryLoop
{
    private readonly List<EntityId> _wallIds = new();

    public IReadOnlyList<EntityId> WallIds => _wallIds;

    internal void AddWall(EntityId wallId)
    {
        if (!_wallIds.Contains(wallId))
            _wallIds.Add(wallId);
    }
}
