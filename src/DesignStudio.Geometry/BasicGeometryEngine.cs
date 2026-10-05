using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Geometry.Abstractions;

namespace DesignStudio.Geometry;

public sealed class BasicGeometryEngine : IGeometryEngine
{
    public BoxGeometry CreateBox(double width, double depth, double height)
    {
        if (width <= 0 || depth <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(
                "Box dimensions must be positive.");

        return new BoxGeometry(width, depth, height);
    }

    public BoxGeometry CreateWallSolid(Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        return CreateBox(
            wall.LengthMm,
            wall.Thickness.Millimeters,
            wall.Height.Millimeters);
    }

    public bool AreEqual(double a, double b, double tolerance)
    {
        if (tolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance));

        return Math.Abs(a - b) <= tolerance;
    }

    public bool IsValid(BoxGeometry box)
    {
        ArgumentNullException.ThrowIfNull(box);

        return box.Width > 0
            && box.Depth > 0
            && box.Height > 0;
    }
}