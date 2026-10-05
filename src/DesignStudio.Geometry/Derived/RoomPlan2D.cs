using DesignStudio.Domain.Project;

namespace DesignStudio.Geometry.Derived;

public sealed class RoomPlan2D
{
    public required IReadOnlyList<Wall2D> Walls { get; init; }
    public required IReadOnlyList<Opening2D> Openings { get; init; }
    public IReadOnlyList<Cabinet2D> Cabinets { get; init; } = Array.Empty<Cabinet2D>();
}
