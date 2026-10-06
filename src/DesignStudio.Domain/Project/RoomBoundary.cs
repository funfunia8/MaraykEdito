namespace DesignStudio.Domain.Project;

public sealed class RoomBoundary
{
    public BoundaryLoop OuterLoop { get; } = new();

    internal void AddOuterEdge(BoundaryEdge edge)
    {
        OuterLoop.AddEdge(edge);
    }
}
