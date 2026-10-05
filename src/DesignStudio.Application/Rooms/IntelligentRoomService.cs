using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Domain.Validation;

namespace DesignStudio.Application.Rooms;

public sealed class IntelligentRoomService
{
    private readonly RoomValidationService _validator = new();

    public Room CreateRectangularRoom(Project project, string name, double widthMm, double depthMm,
        double wallThicknessMm = 120, double wallHeightMm = 2700, double originXmm = 0, double originYmm = 0)
    {
        if (widthMm <= 0 || depthMm <= 0)
            throw new ArgumentOutOfRangeException("Room dimensions must be positive.");

        var room = new Room(name);
        room.SetHeight(Length.FromMillimeters(wallHeightMm));

        var p0 = new Point2D(originXmm, originYmm);
        var p1 = new Point2D(originXmm + widthMm, originYmm);
        var p2 = new Point2D(originXmm + widthMm, originYmm + depthMm);
        var p3 = new Point2D(originXmm, originYmm + depthMm);

        var walls = new[]
        {
            new Wall(p0, p1, Length.FromMillimeters(wallThicknessMm), Length.FromMillimeters(wallHeightMm)),
            new Wall(p1, p2, Length.FromMillimeters(wallThicknessMm), Length.FromMillimeters(wallHeightMm)),
            new Wall(p2, p3, Length.FromMillimeters(wallThicknessMm), Length.FromMillimeters(wallHeightMm)),
            new Wall(p3, p0, Length.FromMillimeters(wallThicknessMm), Length.FromMillimeters(wallHeightMm))
        };

        foreach (var wall in walls)
        {
            project.Add(wall);
            room.AddWall(wall.Id);
        }

        project.Add(room);
        return room;
    }

    public Door AddDoor(Project project, Room room, int wallIndex, double offsetMm, double widthMm, double heightMm = 2100)
    {
        var wall = project.Get<Wall>(room.WallIds[wallIndex]);
        var door = new Door(wall.Id, Length.FromMillimeters(offsetMm), Length.FromMillimeters(widthMm),
            Length.FromMillimeters(heightMm));
        EnsureValid(project, room, door);
        return door;
    }

    public Window AddWindow(Project project, Room room, int wallIndex, double offsetMm, double widthMm,
        double heightMm = 1200, double sillMm = 900)
    {
        var wall = project.Get<Wall>(room.WallIds[wallIndex]);
        var window = new Window(wall.Id, Length.FromMillimeters(offsetMm), Length.FromMillimeters(widthMm),
            Length.FromMillimeters(heightMm), Length.FromMillimeters(sillMm));
        EnsureValid(project, room, window);
        return window;
    }

    public void ResizeWallEnd(Project project, Room room, int wallIndex, Point2D newEnd)
    {
        var wall = project.Get<Wall>(room.WallIds[wallIndex]);
        var oldEnd = wall.End;
        wall.SetGeometry(wall.Start, newEnd);

        var validation = _validator.Validate(project, room);
        if (!validation.IsValid)
        {
            wall.SetGeometry(wall.Start, oldEnd);
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, validation.Issues.Select(x => x.Message)));
        }
    }

    public ValidationResult Validate(Project project, Room room)
        => _validator.Validate(project, room);

    private void EnsureValid(Project project, Room room, ProjectObject candidate)
    {
        project.Add(candidate);
        var result = _validator.Validate(project, room);
        if (!result.IsValid)
        {
            project.Remove(candidate.Id);
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, result.Issues.Select(x => x.Message)));
        }
    }
}
