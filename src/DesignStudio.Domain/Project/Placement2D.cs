using DesignStudio.Domain.Geometry;

namespace DesignStudio.Domain.Project;

public sealed record Placement2D(Point2D Position, double RotationDegrees = 0)
{
    public Placement2D Normalize()
    {
        if (double.IsNaN(RotationDegrees) || double.IsInfinity(RotationDegrees))
            throw new ArgumentOutOfRangeException(nameof(RotationDegrees));

        var normalized = RotationDegrees % 360.0;
        if (normalized < 0) normalized += 360.0;
        return this with { RotationDegrees = normalized };
    }

    public static Placement2D Origin { get; } = new(new Point2D(0, 0), 0);
}
