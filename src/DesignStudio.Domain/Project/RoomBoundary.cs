namespace DesignStudio.Domain.Project;

public sealed class RoomBoundary
{
    public BoundaryLoop OuterLoop { get; } = new();
}
