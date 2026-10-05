namespace DesignStudio.Geometry.Derived;

public sealed record Wall3D(
    double StartX,
    double StartY,
    double EndX,
    double EndY,
    double ThicknessMm,
    double HeightMm);
