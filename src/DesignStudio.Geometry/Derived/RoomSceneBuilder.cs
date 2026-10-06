using DesignStudio.Domain.Project;

namespace DesignStudio.Geometry.Derived;

public sealed class RoomSceneBuilder
{
    public RoomScene3D Build(
        Project project,
        Room room)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(room);

        var walls =
            room.Boundary.OuterLoop.Edges
                .Select(edge =>
                {
                    var wall = project.Get<Wall>(edge.WallId);
                    var segment = edge.ResolveSegment(wall);

                    return new Wall3D(
                        segment.Start.X,
                        segment.Start.Y,
                        segment.End.X,
                        segment.End.Y,
                        segment.Thickness.Millimeters,
                        wall.Height.Millimeters);
                })
                .ToList();

        return new RoomScene3D
        {
            Walls = walls
        };
    }
}
