using DesignStudio.Domain.Units;
using DesignStudio.Domain.Geometry;

namespace DesignStudio.Domain.Geometry;

public sealed record WallSegment(Point2D Start, Point2D End, Length Thickness)
{
    public double LengthMm => Start.DistanceTo(End);
}
