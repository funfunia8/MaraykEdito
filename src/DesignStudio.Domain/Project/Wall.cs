using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Project;

public sealed class Wall : ProjectObject
{
    public Wall(
        Point2D start,
        Point2D end,
        Length thickness,
        Length height,
        EntityId? id = null) : base(id)
    {
        SetCategory(ObjectCategory.Architecture);
        SetGeometry(start, end);
        SetThickness(thickness);
        SetHeight(height);
    }

    public override string Type => "Wall";

    public Point2D Start { get; private set; }
    public Point2D End { get; private set; }
    public Length Thickness { get; private set; }
    public Length Height { get; private set; }

    public void SetGeometry(Point2D start, Point2D end)
    {
        if (start.DistanceTo(end) <= 0.001)
            throw new ArgumentException("Wall endpoints must be distinct.");
        Start = start;
        End = end;
    }

    public void SetThickness(Length thickness)
    {
        if (thickness.Millimeters <= 0) throw new ArgumentOutOfRangeException(nameof(thickness));
        Thickness = thickness;
    }

    public void SetHeight(Length height)
    {
        if (height.Millimeters <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Height = height;
    }

    public double LengthMm => Start.DistanceTo(End);
}
