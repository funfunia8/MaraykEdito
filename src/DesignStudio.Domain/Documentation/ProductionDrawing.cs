using DesignStudio.Domain.Manufacturing;

namespace DesignStudio.Domain.Documentation;

public enum DrawingPrimitiveType
{
    Line,
    Rectangle,
    Text
}

public sealed record DrawingPoint(double X, double Y);

public sealed record DrawingPrimitive(
    DrawingPrimitiveType Type,
    DrawingPoint Start,
    DrawingPoint End,
    string Text,
    double StrokeWidth,
    double FontSize);

public sealed record ProductionDrawingPage(
    string Title,
    string DocumentCode,
    string Revision,
    double WidthMm,
    double HeightMm,
    IReadOnlyList<DrawingPrimitive> Primitives);

public interface IProductionDrawingBuilder
{
    ProductionDrawingPage Build(ProductionDocumentSet documentSet);
}
