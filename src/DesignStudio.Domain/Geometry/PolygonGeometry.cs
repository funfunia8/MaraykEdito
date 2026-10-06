namespace DesignStudio.Domain.Geometry;

public enum PolygonOrientation
{
    Degenerate,
    CounterClockwise,
    Clockwise
}

public static class PolygonGeometry
{
    public static double SignedArea(
        IReadOnlyList<Point2D> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count < 3)
        {
            throw new ArgumentException(
                "A polygon requires at least three points.",
                nameof(points));
        }

        double twiceArea = 0;

        for (var index = 0; index < points.Count; index++)
        {
            var current = points[index];
            var next = points[(index + 1) % points.Count];

            twiceArea +=
                (current.X * next.Y) -
                (next.X * current.Y);
        }

        return twiceArea / 2.0;
    }

    public static PolygonOrientation GetOrientation(
        IReadOnlyList<Point2D> points,
        double toleranceMm)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (toleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toleranceMm));
        }

        var signedArea = SignedArea(points);
        var perimeter = CalculatePerimeter(points);
        var areaTolerance = toleranceMm * perimeter;

        if (Math.Abs(signedArea) <= areaTolerance)
            return PolygonOrientation.Degenerate;

        return signedArea > 0
            ? PolygonOrientation.CounterClockwise
            : PolygonOrientation.Clockwise;
    }

    private static double CalculatePerimeter(
        IReadOnlyList<Point2D> points)
    {
        double perimeter = 0;

        for (var index = 0; index < points.Count; index++)
        {
            var current = points[index];
            var next = points[(index + 1) % points.Count];

            perimeter += current.DistanceTo(next);
        }

        return perimeter;
    }
}
