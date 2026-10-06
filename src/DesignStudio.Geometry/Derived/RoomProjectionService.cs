using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Geometry.Derived;

public sealed class RoomProjectionService
{
    public RoomPlan2D Build(
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

                    return new Wall2D(
                        segment.Start,
                        segment.End,
                        segment.Thickness);
                })
                .ToList();

        var openings = new List<Opening2D>();

        foreach (var door in project.Objects.OfType<Door>())
        {
            if (!IsOpeningOnRoomBoundary(
                    room,
                    door.HostWallId,
                    door.OffsetFromWallStart.Millimeters,
                    door.Width.Millimeters))
            {
                continue;
            }

            var wall = project.Get<Wall>(door.HostWallId);

            openings.Add(
                CreateOpening(
                    wall,
                    door.Id,
                    door.OffsetFromWallStart.Millimeters,
                    door.Width.Millimeters,
                    0,
                    door.Height.Millimeters,
                    OpeningKind.Door));
        }

        foreach (var window in project.Objects.OfType<Window>())
        {
            if (!IsOpeningOnRoomBoundary(
                    room,
                    window.HostWallId,
                    window.OffsetFromWallStart.Millimeters,
                    window.Width.Millimeters))
            {
                continue;
            }

            var wall = project.Get<Wall>(window.HostWallId);

            openings.Add(
                CreateOpening(
                    wall,
                    window.Id,
                    window.OffsetFromWallStart.Millimeters,
                    window.Width.Millimeters,
                    window.SillHeight.Millimeters,
                    window.SillHeight.Millimeters +
                    window.Height.Millimeters,
                    OpeningKind.Window));
        }

        var cabinets =
            project.Objects
                .OfType<Cabinet>()
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

    private static bool IsOpeningOnRoomBoundary(
        Room room,
        EntityId wallId,
        double offsetMm,
        double widthMm)
    {
        if (!double.IsFinite(offsetMm) ||
            !double.IsFinite(widthMm) ||
            widthMm <= 0)
        {
            return false;
        }

        var endMm = offsetMm + widthMm;

        if (!double.IsFinite(endMm))
            return false;

        foreach (var edge in room.Boundary.OuterLoop.Edges)
        {
            if (edge.WallId != wallId)
                continue;

            if (edge.IsWholeWall)
                return true;

            var span = edge.Span!.Value;

            if (span.ContainsOffset(
                    Length.FromMillimeters(offsetMm)) &&
                span.ContainsOffset(
                    Length.FromMillimeters(endMm)))
            {
                return true;
            }
        }

        return false;
    }

    private static Opening2D CreateOpening(
        Wall wall,
        EntityId id,
        double offsetMm,
        double widthMm,
        double bottomMm,
        double topMm,
        OpeningKind kind)
    {
        var length = wall.LengthMm;

        if (!double.IsFinite(length) ||
            length <= 0.001)
        {
            throw new InvalidOperationException(
                "Cannot project an opening onto a zero-length wall.");
        }

        var dx = wall.End.X - wall.Start.X;
        var dy = wall.End.Y - wall.Start.Y;

        var ux = dx / length;
        var uy = dy / length;

        var start = new Point2D(
            wall.Start.X + ux * offsetMm,
            wall.Start.Y + uy * offsetMm);

        var end = new Point2D(
            start.X + ux * widthMm,
            start.Y + uy * widthMm);

        return new Opening2D(
            id,
            kind,
            start,
            end,
            bottomMm,
            topMm);
    }
}
