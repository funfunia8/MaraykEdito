using DesignStudio.Domain.Project;
using DesignStudio.Domain.Validation;

namespace DesignStudio.Application.Rooms;

public sealed class RoomValidationService
{
    public ValidationResult Validate(Project project, Room room)
    {
        var result = new ValidationResult();

        if (room.WallIds.Count < 3)
            result.Error("ROOM-001", "A room must contain at least three walls.");

        foreach (var wallId in room.WallIds)
        {
            if (!project.TryGet(wallId, out var obj) || obj is not Wall)
            {
                result.Error("ROOM-002", $"Room references missing wall {wallId}.");
                continue;
            }

            var wall = (Wall)obj;
            if (wall.LengthMm < RoomConstraints.MinimumWallLengthMm)
                result.Error("WALL-001", $"Wall {wall.Id} is shorter than the minimum allowed length of {RoomConstraints.MinimumWallLengthMm:0} mm.");
        }

        foreach (var door in project.Objects.OfType<Door>())
        {
            if (!room.WallIds.Contains(door.HostWallId)) continue;
            var wall = project.Get<Wall>(door.HostWallId);
            ValidateOpeningBounds(result, wall, door.OffsetFromWallStart.Millimeters, door.Width.Millimeters, door.Height.Millimeters, 0, $"Door {door.Id}", "OPENING-001", "OPENING-002");
        }

        foreach (var window in project.Objects.OfType<Window>())
        {
            if (!room.WallIds.Contains(window.HostWallId)) continue;
            var wall = project.Get<Wall>(window.HostWallId);
            ValidateOpeningBounds(result, wall, window.OffsetFromWallStart.Millimeters, window.Width.Millimeters, window.Height.Millimeters, window.SillHeight.Millimeters, $"Window {window.Id}", "OPENING-003", "OPENING-004");
        }

        ValidateOpeningOverlaps(result, project, room);
        return result;
    }

    private static void ValidateOpeningBounds(
        ValidationResult result, Wall wall, double offsetMm, double widthMm, double heightMm, double sillMm,
        string label, string horizontalCode, string verticalCode)
    {
        if (widthMm < RoomConstraints.MinimumOpeningWidthMm)
            result.Error(horizontalCode, $"{label} is narrower than the minimum opening width of {RoomConstraints.MinimumOpeningWidthMm:0} mm.");

        if (heightMm < RoomConstraints.MinimumOpeningHeightMm)
            result.Error(verticalCode, $"{label} is shorter than the minimum opening height of {RoomConstraints.MinimumOpeningHeightMm:0} mm.");

        if (offsetMm < -RoomConstraints.GeometryToleranceMm || offsetMm + widthMm > wall.LengthMm + RoomConstraints.GeometryToleranceMm)
            result.Error(horizontalCode, $"{label} exceeds its host wall.");

        if (sillMm < -RoomConstraints.GeometryToleranceMm || sillMm + heightMm > wall.Height.Millimeters + RoomConstraints.GeometryToleranceMm)
            result.Error(verticalCode, $"{label} exceeds wall height.");
    }

    private static void ValidateOpeningOverlaps(ValidationResult result, Project project, Room room)
    {
        var openings = project.Objects
            .Where(x => x is Door or Window)
            .Select(x => (Object: x, Host: x switch
            {
                Door door => door.HostWallId,
                Window window => window.HostWallId,
                _ => default
            }, Offset: x switch
            {
                Door door => door.OffsetFromWallStart.Millimeters,
                Window window => window.OffsetFromWallStart.Millimeters,
                _ => 0
            }, Width: x switch
            {
                Door door => door.Width.Millimeters,
                Window window => window.Width.Millimeters,
                _ => 0
            }))
            .Where(x => room.WallIds.Contains(x.Host))
            .GroupBy(x => x.Host);

        foreach (var group in openings)
        {
            var ordered = group.OrderBy(x => x.Offset).ToList();
            for (var i = 1; i < ordered.Count; i++)
            {
                var previous = ordered[i - 1];
                var current = ordered[i];
                if (current.Offset < previous.Offset + previous.Width - RoomConstraints.GeometryToleranceMm)
                {
                    result.Error("OPENING-005", $"Openings {previous.Object.Id} and {current.Object.Id} overlap on wall {group.Key}.");
                }
            }
        }
    }
}
