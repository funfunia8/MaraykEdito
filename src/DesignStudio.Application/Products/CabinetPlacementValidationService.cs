using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Validation;
using DesignStudio.Geometry.Derived;

namespace DesignStudio.Application.Products;

public sealed class CabinetPlacementValidationService
{
    public ValidationResult Validate(Project project, Room room)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(room);

        var result = new ValidationResult();
        var roomBounds = GetRoomBounds(project, room);
        if (roomBounds is null)
        {
            result.Error("CAB-PLACE-001", "Cabinet placement cannot be validated because the room has no valid wall bounds.");
            return result;
        }

        var cabinets = project.Objects
            .OfType<Cabinet>()
            .Where(c => c.HostRoomId == room.Id)
            .ToList();

        foreach (var cabinet in cabinets)
        {
            var footprint = GetFootprint(cabinet);
            ValidateInsideRoom(result, cabinet, footprint, roomBounds.Value);
            ValidateOpeningClearance(result, project, room, cabinet, footprint);
        }

        ValidateCabinetCollisions(result, cabinets);
        return result;
    }

    private static void ValidateInsideRoom(
        ValidationResult result,
        Cabinet cabinet,
        Bounds footprint,
        Bounds roomBounds)
    {
        if (footprint.MinX < roomBounds.MinX - CabinetPlacementConstraints.GeometryToleranceMm ||
            footprint.MaxX > roomBounds.MaxX + CabinetPlacementConstraints.GeometryToleranceMm ||
            footprint.MinY < roomBounds.MinY - CabinetPlacementConstraints.GeometryToleranceMm ||
            footprint.MaxY > roomBounds.MaxY + CabinetPlacementConstraints.GeometryToleranceMm)
        {
            result.Error(
                "CAB-PLACE-002",
                $"Cabinet {cabinet.Id} is outside the active room bounds.");
        }

    }

    private static void ValidateOpeningClearance(
        ValidationResult result,
        Project project,
        Room room,
        Cabinet cabinet,
        Bounds footprint)
    {
        var projection = new RoomProjectionService().Build(project, room);
        foreach (var opening in projection.Openings)
        {
            var minX = Math.Min(opening.Start.X, opening.End.X) - CabinetPlacementConstraints.OpeningClearanceMm;
            var maxX = Math.Max(opening.Start.X, opening.End.X) + CabinetPlacementConstraints.OpeningClearanceMm;
            var minY = Math.Min(opening.Start.Y, opening.End.Y) - CabinetPlacementConstraints.OpeningClearanceMm;
            var maxY = Math.Max(opening.Start.Y, opening.End.Y) + CabinetPlacementConstraints.OpeningClearanceMm;

            if (Intersects(footprint, new Bounds(minX, minY, maxX, maxY)))
            {
                result.Error(
                    "CAB-PLACE-004",
                    $"Cabinet {cabinet.Id} intrudes into the clearance zone of opening {opening.Id}.");
            }
        }
    }

    private static void ValidateCabinetCollisions(ValidationResult result, IReadOnlyList<Cabinet> cabinets)
    {
        for (var i = 0; i < cabinets.Count; i++)
        {
            var first = cabinets[i];
            var firstBounds = GetFootprint(first);

            for (var j = i + 1; j < cabinets.Count; j++)
            {
                var second = cabinets[j];
                var secondBounds = GetFootprint(second);

                if (Intersects(firstBounds, secondBounds))
                {
                    result.Error(
                        "CAB-PLACE-005",
                        $"Cabinets {first.Id} and {second.Id} overlap.");
                    continue;
                }

                var gap = DistanceBetween(firstBounds, secondBounds);
                if (gap > CabinetPlacementConstraints.GeometryToleranceMm &&
                    gap < CabinetPlacementConstraints.CabinetWarningClearanceMm)
                {
                    result.Warning(
                        "CAB-PLACE-006",
                        $"Cabinets {first.Id} and {second.Id} have only {gap:0.#} mm clearance.");
                }
            }
        }
    }

    private static Bounds? GetRoomBounds(Project project, Room room)
    {
        var points = new List<Point2D>();
        foreach (var wallId in room.WallIds)
        {
            if (!project.TryGet(wallId, out var obj) || obj is not Wall wall)
                continue;

            points.Add(wall.Start);
            points.Add(wall.End);
        }

        if (points.Count == 0) return null;

        return new Bounds(
            points.Min(p => p.X),
            points.Min(p => p.Y),
            points.Max(p => p.X),
            points.Max(p => p.Y));
    }

    private static Bounds GetFootprint(Cabinet cabinet)
    {
        var halfWidth = cabinet.Width.Millimeters / 2.0;
        var halfDepth = cabinet.Depth.Millimeters / 2.0;
        var radians = cabinet.RotationDegrees * Math.PI / 180.0;
        var cos = Math.Abs(Math.Cos(radians));
        var sin = Math.Abs(Math.Sin(radians));
        var extentX = cos * halfWidth + sin * halfDepth;
        var extentY = sin * halfWidth + cos * halfDepth;

        return new Bounds(
            cabinet.Position.X - extentX,
            cabinet.Position.Y - extentY,
            cabinet.Position.X + extentX,
            cabinet.Position.Y + extentY);
    }

    private static bool Intersects(Bounds a, Bounds b)
        => a.MinX < b.MaxX - CabinetPlacementConstraints.GeometryToleranceMm &&
           a.MaxX > b.MinX + CabinetPlacementConstraints.GeometryToleranceMm &&
           a.MinY < b.MaxY - CabinetPlacementConstraints.GeometryToleranceMm &&
           a.MaxY > b.MinY + CabinetPlacementConstraints.GeometryToleranceMm;

    private static double DistanceBetween(Bounds a, Bounds b)
    {
        var dx = Math.Max(0, Math.Max(a.MinX - b.MaxX, b.MinX - a.MaxX));
        var dy = Math.Max(0, Math.Max(a.MinY - b.MaxY, b.MinY - a.MaxY));
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private readonly record struct Bounds(double MinX, double MinY, double MaxX, double MaxY);
}
