using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;

namespace DesignStudio.Geometry.Derived;

public sealed record Cabinet2D(
    EntityId Id,
    string Name,
    Point2D Center,
    double WidthMm,
    double DepthMm,
    double RotationDegrees);
