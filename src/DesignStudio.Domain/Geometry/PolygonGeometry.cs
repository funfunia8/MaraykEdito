namespace DesignStudio.Domain.Geometry;

public enum PolygonOrientation
{
    Degenerate,
    CounterClockwise,
    Clockwise
}

public static class PolygonGeometry
{
    public static double SignedArea(IReadOnlyList<Point2D> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count < 3)
        {
            throw new ArgumentException(
                "A polygon requires at least three points.",
                nameof(points));
        }

        var area = 0d;

        for (var index = 0; index < points.Count; index++)
        {
            var current = points[index];
            var next = points[(index + 1) % points.Count];

            area +=
                (current.X * next.Y) -
                (next.X * current.Y);
        }

        return area / 2d;
    }

    public static PolygonOrientation GetOrientation(
        IReadOnlyList<Point2D> points,
        double toleranceMm)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (!double.IsFinite(toleranceMm) ||
            toleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toleranceMm));
        }

        var signedArea = SignedArea(points);

        if (!double.IsFinite(signedArea))
            return PolygonOrientation.Degenerate;

        var areaTolerance =
            toleranceMm * toleranceMm;

        if (Math.Abs(signedArea) <= areaTolerance)
            return PolygonOrientation.Degenerate;

        return signedArea > 0
            ? PolygonOrientation.CounterClockwise
            : PolygonOrientation.Clockwise;
    }

    public static void ValidateSimple(
        IReadOnlyList<Point2D> points,
        double toleranceMm = 0.001)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count < 3)
        {
            throw new ArgumentException(
                "A polygon requires at least three vertices.",
                nameof(points));
        }

        if (!double.IsFinite(toleranceMm) ||
            toleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toleranceMm));
        }

        foreach (var point in points)
        {
            if (!double.IsFinite(point.X) ||
                !double.IsFinite(point.Y))
            {
                throw new ArgumentException(
                    "Invalid polygon vertices.",
                    nameof(points));
            }
        }

        var edgeCount = points.Count;

        for (var index = 0;
             index < edgeCount;
             index++)
        {
            var start = points[index];
            var end = points[(index + 1) % edgeCount];

            if (start.DistanceTo(end) <= toleranceMm)
            {
                throw new ArgumentException(
                    "Polygon boundary edges must have non-zero length.",
                    nameof(points));
            }
        }

        for (var firstIndex = 0;
             firstIndex < edgeCount;
             firstIndex++)
        {
            var firstStart = points[firstIndex];
            var firstEnd =
                points[(firstIndex + 1) % edgeCount];

            for (var secondIndex = firstIndex + 1;
                 secondIndex < edgeCount;
                 secondIndex++)
            {
                var secondStart = points[secondIndex];
                var secondEnd =
                    points[(secondIndex + 1) % edgeCount];

                var relation =
                    SegmentGeometry.Classify(
                        secondStart,
                        secondEnd,
                        firstStart,
                        firstEnd,
                        toleranceMm);

                var areAdjacent =
                    secondIndex == firstIndex + 1 ||
                    (firstIndex == 0 &&
                     secondIndex == edgeCount - 1);

                if (areAdjacent)
                {
                    if (relation != SegmentRelation.Touch)
                    {
                        throw new ArgumentException(
                            "Adjacent polygon edges must meet only at their shared endpoint.",
                            nameof(points));
                    }
                }
                else if (relation != SegmentRelation.None)
                {
                    throw new ArgumentException(
                        "Non-adjacent polygon edges must not intersect, touch, or overlap.",
                        nameof(points));
                }
            }
        }
    }
}
