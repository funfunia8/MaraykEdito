using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Units;

namespace DesignStudio.Geometry.Derived;

public sealed record Wall2D(
    Point2D Start,
    Point2D End,
    Length Thickness)
{
    public double LengthMm => Start.DistanceTo(End);
}
