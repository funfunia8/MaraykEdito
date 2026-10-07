namespace DesignStudio.Domain.Geometry;

public sealed class Region2D
{
    private readonly IReadOnlyList<Point2D> _vertices;

    public Region2D(
        IReadOnlyList<Point2D> vertices,
        double validityToleranceMm = 0.001)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        if (vertices.Count < 3)
        {
            throw new ArgumentException(
                "A region requires at least three boundary vertices.",
                nameof(vertices));
        }

        if (!double.IsFinite(validityToleranceMm) ||
            validityToleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(validityToleranceMm));
        }

        foreach (var point in vertices)
        {
            if (!double.IsFinite(point.X) ||
                !double.IsFinite(point.Y))
            {
                throw new ArgumentException(
                    "Region vertices must contain finite coordinates.",
                    nameof(vertices));
            }
        }

        var copied =
            vertices.ToArray();

        PolygonGeometry.ValidateSimple(
            copied,
            validityToleranceMm);

        var orientation =
            PolygonGeometry.GetOrientation(
                copied,
                validityToleranceMm);

        if (orientation == PolygonOrientation.Degenerate)
        {
            throw new ArgumentException(
                "A region cannot be degenerate or near-zero-area.",
                nameof(vertices));
        }

        _vertices =
            Array.AsReadOnly(copied);

        ValidityToleranceMm =
            validityToleranceMm;
    }

    public IReadOnlyList<Point2D> Vertices =>
        _vertices;

    public double SignedArea =>
        PolygonGeometry.SignedArea(_vertices);

    public double AreaMm2 =>
        Math.Abs(SignedArea);

    public PolygonOrientation Orientation =>
        PolygonGeometry.GetOrientation(
            _vertices,
            ValidityToleranceMm);

    public double ValidityToleranceMm { get; }

    public PointContainment Classify(
        Point2D point,
        double toleranceMm = 0.001)
    {
        ValidatePoint(point);

        if (!double.IsFinite(toleranceMm) ||
            toleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toleranceMm));
        }

        for (var index = 0;
             index < _vertices.Count;
             index++)
        {
            var start =
                _vertices[index];

            var end =
                _vertices[
                    (index + 1) %
                    _vertices.Count];

            if (DistanceToSegment(
                    point,
                    start,
                    end) <= toleranceMm)
            {
                return PointContainment.Boundary;
            }
        }

        var inside = false;

        for (var index = 0;
             index < _vertices.Count;
             index++)
        {
            var current =
                _vertices[index];

            var next =
                _vertices[
                    (index + 1) %
                    _vertices.Count];

            var crossesRay =
                (current.Y > point.Y) !=
                (next.Y > point.Y);

            if (!crossesRay)
                continue;

            var denominator =
                next.Y - current.Y;

            if (Math.Abs(denominator) <= double.Epsilon)
                continue;

            var intersectionX =
                current.X +
                ((point.Y - current.Y) *
                 (next.X - current.X) /
                 denominator);

            if (point.X < intersectionX)
                inside = !inside;
        }

        return inside
            ? PointContainment.Inside
            : PointContainment.Outside;
    }

    public bool Contains(
        Point2D point,
        double toleranceMm = 0.001)
        => Classify(point, toleranceMm)
            != PointContainment.Outside;

    public bool ContainsInterior(
        Point2D point,
        double toleranceMm = 0.001)
        => Classify(point, toleranceMm)
            == PointContainment.Inside;

    private static double DistanceToSegment(
        Point2D point,
        Point2D start,
        Point2D end)
    {
        var dx =
            end.X - start.X;

        var dy =
            end.Y - start.Y;

        var lengthSquared =
            (dx * dx) +
            (dy * dy);

        if (lengthSquared <= 0)
            return point.DistanceTo(start);

        var projection =
            ((point.X - start.X) * dx +
             (point.Y - start.Y) * dy) /
            lengthSquared;

        var clamped =
            Math.Clamp(
                projection,
                0,
                1);

        var closest =
            new Point2D(
                start.X + clamped * dx,
                start.Y + clamped * dy);

        return point.DistanceTo(closest);
    }

    private static void ValidatePoint(
        Point2D point)
    {
        if (!double.IsFinite(point.X) ||
            !double.IsFinite(point.Y))
        {
            throw new ArgumentOutOfRangeException(
                nameof(point),
                "Point coordinates must be finite numbers.");
        }
    }
}
