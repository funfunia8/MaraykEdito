using DesignStudio.Domain.Project;

namespace DesignStudio.Geometry.Derived;

public sealed class RoomSceneBuilder
{
    public RoomScene3D Build(Project project, Room room)
    {
        return new RoomScene3D
        {
            Walls = room.WallIds
                .Select(project.Get<Wall>)
                .Select(w => new Wall3D(
                    w.Start.X, w.Start.Y,
                    w.End.X, w.End.Y,
                    w.Thickness.Millimeters,
                    w.Height.Millimeters))
                .ToList()
        };
    }
}
