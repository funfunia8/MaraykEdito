namespace DesignStudio.Geometry.Abstractions;

public interface IGeometryEngine
{
    BoxGeometry CreateBox(double width, double depth, double height);
    bool AreEqual(double a, double b, double tolerance);
    bool IsValid(BoxGeometry box);
}
