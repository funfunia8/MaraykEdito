namespace DesignStudio.Domain.Geometry;

public readonly record struct Point2D(double X, double Y)
{
    public double DistanceTo(Point2D other)
        => Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
}
