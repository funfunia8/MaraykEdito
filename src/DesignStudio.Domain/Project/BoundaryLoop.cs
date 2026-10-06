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
        foreach (var existing in _edges)
        {
            if (existing.WallId != edge.WallId)
                continue;

            if (existing.Span == edge.Span)
            {
                // Same physical wall span is already represented.
                // Traversal direction does not create a distinct span.
                return;
            }

            if (existing.IsWholeWall || edge.IsWholeWall)
            {
                throw new InvalidOperationException(
                    $"Boundary loop cannot contain overlapping spans of wall {edge.WallId}.");
            }

            if (existing.Span!.Value.Overlaps(edge.Span!.Value))
            {
                throw new InvalidOperationException(
                    $"Boundary loop cannot contain overlapping spans of wall {edge.WallId}.");
            }
        }

        _edges.Add(edge);
    }
}
