using DesignStudio.Domain.Project;
using DesignStudio.Domain.Geometry;

namespace DesignStudio.Geometry.Derived;

public sealed class RoomProjectionService
{
    public RoomPlan2D Build(Project project, Room room)
    {
        var walls = room.WallIds
            .Select(project.Get<Wall>)
            .Select(w => new Wall2D(w.Start, w.End, w.Thickness))
            .ToList();

        var openings = new List<Opening2D>();

        foreach (var door in project.Objects.OfType<Door>())
        {
            if (!room.WallIds.Contains(door.HostWallId)) continue;
            var wall = project.Get<Wall>(door.HostWallId);
            openings.Add(CreateOpening(wall, door.Id, door.OffsetFromWallStart.Millimeters,
                door.Width.Millimeters, 0, door.Height.Millimeters, OpeningKind.Door));
        }

        foreach (var window in project.Objects.OfType<Window>())
        {
            if (!room.WallIds.Contains(window.HostWallId)) continue;
            var wall = project.Get<Wall>(window.HostWallId);
            openings.Add(CreateOpening(wall, window.Id, window.OffsetFromWallStart.Millimeters,
                window.Width.Millimeters, window.SillHeight.Millimeters,
                window.SillHeight.Millimeters + window.Height.Millimeters, OpeningKind.Window));
        }

        var cabinets = project.Objects.OfType<Cabinet>()
            .Where(c => c.HostRoomId == room.Id)
            .Select(c => new Cabinet2D(
                c.Id,
                c.Name,
                c.Position,
                c.Width.Millimeters,
                c.Depth.Millimeters,
                c.RotationDegrees))
            .ToList();

        return new RoomPlan2D
        {
            Walls = walls,
            Openings = openings,
            Cabinets = cabinets
        };
    }

    private static Opening2D CreateOpening(
        Wall wall,
        DesignStudio.Domain.Identity.EntityId id,
        double offsetMm,
        double widthMm,
        double bottomMm,
        double topMm,
        OpeningKind kind)
    {
        var dx = wall.End.X - wall.Start.X;
        var dy = wall.End.Y - wall.Start.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);

        if (length <= 0.001) throw new InvalidOperationException("Cannot project an opening onto a zero-length wall.");

        var ux = dx / length;
        var uy = dy / length;

        var sx = wall.Start.X + ux * offsetMm;
        var sy = wall.Start.Y + uy * offsetMm;
        var ex = sx + ux * widthMm;
        var ey = sy + uy * widthMm;

        return new Opening2D(
            id,
            kind,
            new Point2D(sx, sy),
            new Point2D(ex, ey),
            bottomMm,
            topMm);
    }
}
