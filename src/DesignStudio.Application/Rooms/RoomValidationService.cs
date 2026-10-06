using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Validation;

namespace DesignStudio.Application.Rooms;

public sealed class RoomValidationService
{
    public ValidationResult Validate(Project project, Room room)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(room);

        var result = new ValidationResult();

        if (room.WallIds.Count < 3)
            result.Error("ROOM-001", "A room must contain at least three walls.");

        var walls = new List<Wall>();
        var wallIds = new HashSet<DesignStudio.Domain.Identity.EntityId>();

        foreach (var wallId in room.WallIds)
        {
            if (!wallIds.Add(wallId))
            {
                result.Error(
                    "ROOM-003",
                    $"Room references wall {wallId} more than once.");
                continue;
            }

            if (!project.TryGet(wallId, out var obj) || obj is not Wall)
            {
                result.Error(
                    "ROOM-002",
                    $"Room references missing wall {wallId}.");
                continue;
            }

            var wall = (Wall)obj;
            walls.Add(wall);

            if (wall.LengthMm < RoomConstraints.MinimumWallLengthMm)
            {
                result.Error(
                    "WALL-001",
                    $"Wall {wall.Id} is shorter than the minimum allowed length of {RoomConstraints.MinimumWallLengthMm:0} mm.");
            }
        }

        ValidateRoomTopology(result, room, walls);
        ValidateRoomBoundaryGeometry(result, room, walls);

        foreach (var door in project.Objects.OfType<Door>())
        {
            if (!room.WallIds.Contains(door.HostWallId))
                continue;

            if (!project.TryGet(door.HostWallId, out var obj) ||
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
            if (!room.WallIds.Contains(window.HostWallId))
                continue;

            if (!project.TryGet(window.HostWallId, out var obj) ||
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

        ValidateOpeningOverlaps(result, project, room);

        return result;
    }

    private static void ValidateRoomTopology(
        ValidationResult result,
        Room room,
        IReadOnlyList<Wall> walls)
    {
        if (walls.Count != room.WallIds.Count)
            return;

        if (walls.Count < 3)
            return;

        var tolerance = RoomConstraints.GeometryToleranceMm;

        for (var index = 0; index < walls.Count - 1; index++)
        {
            var current = walls[index];
            var next = walls[index + 1];

            if (current.End.DistanceTo(next.Start) > tolerance)
            {
                result.Error(
                    "ROOM-004",
                    $"Room boundary is disconnected between walls {current.Id} and {next.Id}.");
            }
        }

        var first = walls[0];
        var last = walls[^1];

        if (last.End.DistanceTo(first.Start) > tolerance)
        {
            result.Error(
                "ROOM-005",
                $"Room boundary is not closed between walls {last.Id} and {first.Id}.");
        }
    }

    private static void ValidateRoomBoundaryGeometry(
        ValidationResult result,
        Room room,
        IReadOnlyList<Wall> walls)
    {
        if (walls.Count != room.WallIds.Count)
            return;

        if (walls.Count < 3)
            return;

        var tolerance = RoomConstraints.GeometryToleranceMm;

        for (var firstIndex = 0; firstIndex < walls.Count; firstIndex++)
        {
            for (var secondIndex = firstIndex + 1;
                 secondIndex < walls.Count;
                 secondIndex++)
            {
                var first = walls[firstIndex];
                var second = walls[secondIndex];

                var relation = SegmentGeometry.Classify(
                    first.Start,
                    first.End,
                    second.Start,
                    second.End,
                    tolerance);

                var adjacent =
                    secondIndex == firstIndex + 1 ||
                    (firstIndex == 0 && secondIndex == walls.Count - 1);

                if (adjacent)
                {
                    if (relation != SegmentRelation.Touch)
                    {
                        result.Error(
                            "ROOM-006",
                            $"Adjacent room boundary walls {first.Id} and {second.Id} have an invalid geometric relationship: {relation}.");
                    }

                    continue;
                }

                if (relation != SegmentRelation.None)
                {
                    result.Error(
                        "ROOM-006",
                        $"Room boundary walls {first.Id} and {second.Id} intersect or overlap: {relation}.");
                }
            }
        }
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
            offsetMm + widthMm > wall.LengthMm + RoomConstraints.GeometryToleranceMm)
        {
            result.Error(
                horizontalCode,
                $"{label} exceeds its host wall.");
        }

        if (sillMm < -RoomConstraints.GeometryToleranceMm ||
            sillMm + heightMm > wall.Height.Millimeters + RoomConstraints.GeometryToleranceMm)
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
        var openings = project.Objects
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
                    Door door => door.OffsetFromWallStart.Millimeters,
                    Window window => window.OffsetFromWallStart.Millimeters,
                    _ => 0
                },
                Width: x switch
                {
                    Door door => door.Width.Millimeters,
                    Window window => window.Width.Millimeters,
                    _ => 0
                }))
            .Where(x => room.WallIds.Contains(x.Host))
            .GroupBy(x => x.Host);

        foreach (var group in openings)
        {
            var ordered = group
                .OrderBy(x => x.Offset)
                .ToList();

            for (var index = 1; index < ordered.Count; index++)
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
}