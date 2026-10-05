using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;

namespace DesignStudio.Geometry.Derived;

public enum OpeningKind
{
    Door,
    Window
}

public sealed record Opening2D(
    EntityId Id,
    OpeningKind Kind,
    Point2D Start,
    Point2D End,
    double BottomMm,
    double TopMm);
