using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Domain.Validation;

namespace DesignStudio.Application.Rooms;

public sealed class RoomValidationService
{
    public ValidationResult Validate(
        Project project,
        Room room)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(room);

        var result = new ValidationResult();
        var edges = room.Boundary.OuterLoop.Edges;

        if (edges.Count < 3)
        {
            result.Error(
                "ROOM-001",
                "A room must contain at least three boundary edges.");
        }

        var resolvedEdges =
            ResolveBoundaryEdges(
                project,
                room,
                result);

        var boundaryIsResolved =
            resolvedEdges.Count == edges.Count &&
            resolvedEdges.Count >= 3;

        var topologyValid =
            boundaryIsResolved &&
            ValidateRoomTopology(
                result,
                resolvedEdges);

        var geometryValid =
            boundaryIsResolved &&
            ValidateRoomBoundaryGeometry(
                result,
                resolvedEdges);

        if (topologyValid && geometryValid)
        {
            ValidateRoomBoundaryOrientation(
                result,
                resolvedEdges);
        }

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

            if (!project.TryGet(
                    door.HostWallId,
                    out var obj) ||
                obj is not Wall wall)
            {
                continue;
            }

            ValidateOpeningBounds(
                result,
                wall,
                door.OffsetFromWallStart.Millimeters,
                door.Width.Millimeters,
                door.Height.Millimeters,
                0,
                $"Door {door.Id}",
                "OPENING-001",
                "OPENING-002");
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

            if (!project.TryGet(
                    window.HostWallId,
                    out var obj) ||
                obj is not Wall wall)
            {
                continue;
            }

            ValidateOpeningBounds(
                result,
                wall,
                window.OffsetFromWallStart.Millimeters,
                window.Width.Millimeters,
                window.Height.Millimeters,
                window.SillHeight.Millimeters,
                $"Window {window.Id}",
                "OPENING-003",
                "OPENING-004");
        }

        ValidateOpeningOverlaps(
            result,
            project,
            room);

        return result;
    }

    private static List<ResolvedBoundaryEdge> ResolveBoundaryEdges(
        Project project,
        Room room,
        ValidationResult result)
    {
        var resolved = new List<ResolvedBoundaryEdge>();
        var edges = room.Boundary.OuterLoop.Edges;

        foreach (var edge in edges)
        {
            if (!project.TryGet(
                    edge.WallId,
                    out var obj) ||
                obj is not Wall wall)
            {
                result.Error(
                    "ROOM-002",
                    $"Room references missing wall {edge.WallId}.");

                continue;
            }

            if (!double.IsFinite(wall.LengthMm) ||
                wall.LengthMm <= RoomConstraints.GeometryToleranceMm)
            {
                result.Error(
                    "WALL-001",
                    $"Wall {wall.Id} has an invalid or zero length.");

                continue;
            }

            if (wall.LengthMm < RoomConstraints.MinimumWallLengthMm)
            {
                result.Error(
                    "WALL-001",
                    $"Wall {wall.Id} is shorter than the minimum allowed length of {RoomConstraints.MinimumWallLengthMm:0} mm.");
            }

            WallSegment segment;

            try
            {
                segment = edge.ResolveSegment(wall);
            }
            catch (ArgumentOutOfRangeException)
            {
                result.Error(
                    "ROOM-009",
                    $"Boundary edge for wall {wall.Id} has an invalid span.");

                continue;
            }
            catch (InvalidOperationException)
            {
                result.Error(
                    "ROOM-009",
                    $"Boundary edge for wall {wall.Id} has a span outside the physical wall.");

                continue;
            }

            resolved.Add(
                new ResolvedBoundaryEdge(
                    edge,
                    wall,
                    segment));
        }

        return resolved;
    }

    private static bool ValidateRoomTopology(
        ValidationResult result,
        IReadOnlyList<ResolvedBoundaryEdge> edges)
    {
        var tolerance =
            RoomConstraints.GeometryToleranceMm;

        var valid = true;

        for (var index = 0;
             index < edges.Count - 1;
             index++)
        {
            var current = edges[index];
            var next = edges[index + 1];

            if (current.Segment.End.DistanceTo(
                    next.Segment.Start) > tolerance)
            {
                result.Error(
                    "ROOM-004",
                    $"Room boundary is disconnected between walls " +
                    $"{current.Wall.Id} and {next.Wall.Id}.");

                valid = false;
            }
        }

        var first = edges[0];
        var last = edges[^1];

        if (last.Segment.End.DistanceTo(
                first.Segment.Start) > tolerance)
        {
            result.Error(
                "ROOM-005",
                $"Room boundary is not closed between walls " +
                $"{last.Wall.Id} and {first.Wall.Id}.");

            valid = false;
        }

        return valid;
    }

    private static bool ValidateRoomBoundaryGeometry(
        ValidationResult result,
        IReadOnlyList<ResolvedBoundaryEdge> edges)
    {
        var tolerance =
            RoomConstraints.GeometryToleranceMm;

        var valid = true;

        for (var firstIndex = 0;
             firstIndex < edges.Count;
             firstIndex++)
        {
            for (var secondIndex = firstIndex + 1;
                 secondIndex < edges.Count;
                 secondIndex++)
            {
                var first = edges[firstIndex];
                var second = edges[secondIndex];

                var relation =
                    SegmentGeometry.Classify(
                        first.Segment.Start,
                        first.Segment.End,
                        second.Segment.Start,
                        second.Segment.End,
                        tolerance);

                var adjacent =
                    secondIndex == firstIndex + 1 ||
                    (firstIndex == 0 &&
                     secondIndex == edges.Count - 1);

                if (adjacent)
                {
                    if (relation != SegmentRelation.Touch)
                    {
                        result.Error(
                            "ROOM-006",
                            $"Adjacent room boundary edges on walls " +
                            $"{first.Wall.Id} and {second.Wall.Id} " +
                            $"have an invalid geometric relationship: {relation}.");

                        valid = false;
                    }

                    continue;
                }

                if (relation != SegmentRelation.None)
                {
                    result.Error(
                        "ROOM-006",
                        $"Room boundary edges on walls " +
                        $"{first.Wall.Id} and {second.Wall.Id} " +
                        $"intersect or overlap: {relation}.");

                    valid = false;
                }
            }
        }

        return valid;
    }

    private static void ValidateRoomBoundaryOrientation(
        ValidationResult result,
        IReadOnlyList<ResolvedBoundaryEdge> edges)
    {
        var points =
            edges
                .Select(edge => edge.Segment.Start)
                .ToArray();

        var orientation =
            PolygonGeometry.GetOrientation(
                points,
                RoomConstraints.GeometryToleranceMm);

        switch (orientation)
        {
            case PolygonOrientation.CounterClockwise:
                return;

            case PolygonOrientation.Clockwise:
                result.Error(
                    "ROOM-007",
                    "Room outer boundary must be counter-clockwise in domain coordinates.");
                return;

            case PolygonOrientation.Degenerate:
                result.Error(
                    "ROOM-008",
                    "Room outer boundary has zero or near-zero signed area and is degenerate.");
                return;

            default:
                throw new InvalidOperationException(
                    $"Unsupported polygon orientation: {orientation}.");
        }
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
                    Length.FromMillimeters(offsetMm),
                    RoomConstraints.GeometryToleranceMm) &&
                span.ContainsOffset(
                    Length.FromMillimeters(endMm),
                    RoomConstraints.GeometryToleranceMm))
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateOpeningBounds(
        ValidationResult result,
        Wall wall,
        double offsetMm,
        double widthMm,
        double heightMm,
        double sillMm,
        string label,
        string horizontalCode,
        string verticalCode)
    {
        if (!double.IsFinite(offsetMm) ||
            !double.IsFinite(widthMm) ||
            !double.IsFinite(heightMm) ||
            !double.IsFinite(sillMm))
        {
            result.Error(
                horizontalCode,
                $"{label} contains non-finite dimensions.");

            return;
        }

        if (widthMm < RoomConstraints.MinimumOpeningWidthMm)
        {
            result.Error(
                horizontalCode,
                $"{label} is narrower than the minimum opening width of {RoomConstraints.MinimumOpeningWidthMm:0}mm.");
        }

        if (heightMm < RoomConstraints.MinimumOpeningHeightMm)
        {
            result.Error(
                verticalCode,
                $"{label} is shorter than the minimum opening height of {RoomConstraints.MinimumOpeningHeightMm:0} mm.");
        }

        if (offsetMm < -RoomConstraints.GeometryToleranceMm ||
            offsetMm + widthMm >
            wall.LengthMm + RoomConstraints.GeometryToleranceMm)
        {
            result.Error(
                horizontalCode,
                $"{label} exceeds its host wall.");
        }

        if (sillMm < -RoomConstraints.GeometryToleranceMm ||
            sillMm + heightMm >
            wall.Height.Millimeters + RoomConstraints.GeometryToleranceMm)
        {
            result.Error(
                verticalCode,
                $"{label} exceeds wall height.");
        }
    }

    private static void ValidateOpeningOverlaps(
        ValidationResult result,
        Project project,
        Room room)
    {
        var openings =
            project.Objects
                .Where(x => x is Door or Window)
                .Select(x => (
                    Object: x,
                    Host: x switch
                    {
                        Door door => door.HostWallId,
                        Window window => window.HostWallId,
                        _ => default
                    },
                    Offset: x switch
                    {
                        Door door =>
                            door.OffsetFromWallStart.Millimeters,

                        Window window =>
                            window.OffsetFromWallStart.Millimeters,

                        _ => 0
                    },
                    Width: x switch
                    {
                        Door door =>
                            door.Width.Millimeters,

                        Window window =>
                            window.Width.Millimeters,

                        _ => 0
                    }))
                .Where(x =>
                    IsOpeningOnRoomBoundary(
                        room,
                        x.Host,
                        x.Offset,
                        x.Width))
                .GroupBy(x => x.Host);

        foreach (var group in openings)
        {
            var ordered =
                group
                    .OrderBy(x => x.Offset)
                    .ToList();

            for (var index = 1;
                 index < ordered.Count;
                 index++)
            {
                var previous = ordered[index - 1];
                var current = ordered[index];

                if (current.Offset <
                    previous.Offset +
                    previous.Width -
                    RoomConstraints.GeometryToleranceMm)
                {
                    result.Error(
                        "OPENING-005",
                        $"Openings {previous.Object.Id} and {current.Object.Id} overlap on wall {group.Key}.");
                }
            }
        }
    }

    private readonly record struct ResolvedBoundaryEdge(
        BoundaryEdge Edge,
        Wall Wall,
        WallSegment Segment);
}
