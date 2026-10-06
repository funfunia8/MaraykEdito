using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public sealed class BoundaryLoop
{
    private readonly List<BoundaryEdge> _edges = new();

    public IReadOnlyList<BoundaryEdge> Edges => _edges;

    public IReadOnlyList<EntityId> WallIds =>
        _edges
            .Select(edge => edge.WallId)
            .ToArray();

    internal void AddWall(EntityId wallId)
    {
        AddEdge(new BoundaryEdge(wallId));
    }

    internal void AddEdge(BoundaryEdge edge)
    {
        if (_edges.Any(
                existing => existing.WallId == edge.WallId))
        {
            return;
        }

        _edges.Add(edge);
    }
}
