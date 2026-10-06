using DesignStudio.Domain.Documentation;

namespace DesignStudio.Application.Documentation;

public sealed class ProductionDimensionEngine
{
    private const double DefaultArrowSize = 3.5;
    private const double DefaultStrokeWidth = 0.35;
    private const double DefaultFontSize = 2.8;

    public void AddHorizontalDimension(
        List<DrawingPrimitive> primitives,
        double x1,
        double x2,
        double geometryY,
        double dimensionY,
        string label,
        double arrowSize = DefaultArrowSize)
    {
        ArgumentNullException.ThrowIfNull(primitives);

        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Dimension label is required.", nameof(label));

        if (Math.Abs(x2 - x1) < double.Epsilon)
            return;

        primitives.Add(Line(
            x1,
            geometryY,
            x1,
            dimensionY));

        primitives.Add(Line(
            x2,
            geometryY,
            x2,
            dimensionY));

        primitives.Add(Line(
            x1,
            dimensionY,
            x2,
            dimensionY));

        AddArrowHead(primitives, x1, dimensionY, arrowSize, pointsRight: true);
        AddArrowHead(primitives, x2, dimensionY, arrowSize, pointsRight: false);

        primitives.Add(Text(
            (x1 + x2) / 2,
            dimensionY - 2,
            label));
    }

    public void AddVerticalDimension(
        List<DrawingPrimitive> primitives,
        double x1,
        double y1,
        double y2,
        double dimensionX,
        string label,
        double arrowSize = DefaultArrowSize)
    {
        ArgumentNullException.ThrowIfNull(primitives);

        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Dimension label is required.", nameof(label));

        if (Math.Abs(y2 - y1) < double.Epsilon)
            return;

        primitives.Add(Line(
            x1,
            y1,
            dimensionX,
            y1));

        primitives.Add(Line(
            x1,
            y2,
            dimensionX,
            y2));

        primitives.Add(Line(
            dimensionX,
            y1,
            dimensionX,
            y2));

        AddVerticalArrowHead(primitives, dimensionX, y1, arrowSize, pointsDown: true);
        AddVerticalArrowHead(primitives, dimensionX, y2, arrowSize, pointsDown: false);

        primitives.Add(Text(
            dimensionX - 2,
            (y1 + y2) / 2,
            label));
    }

    private static void AddArrowHead(
        List<DrawingPrimitive> primitives,
        double x,
        double y,
        double size,
        bool pointsRight)
    {
        var direction = pointsRight ? 1 : -1;

        primitives.Add(Line(
            x,
            y,
            x + direction * size,
            y - size / 2));

        primitives.Add(Line(
            x,
            y,
            x + direction * size,
            y + size / 2));
    }

    private static void AddVerticalArrowHead(
        List<DrawingPrimitive> primitives,
        double x,
        double y,
        double size,
        bool pointsDown)
    {
        var direction = pointsDown ? 1 : -1;

        primitives.Add(Line(
            x,
            y,
            x - size / 2,
            y + direction * size));

        primitives.Add(Line(
            x,
            y,
            x + size / 2,
            y + direction * size));
    }

    private static DrawingPrimitive Line(
        double x1,
        double y1,
        double x2,
        double y2) =>
        new(
            DrawingPrimitiveType.Line,
            new(x1, y1),
            new(x2, y2),
            string.Empty,
            DefaultStrokeWidth,
            0);

    private static DrawingPrimitive Text(
        double x,
        double y,
        string text) =>
        new(
            DrawingPrimitiveType.Text,
            new(x, y),
            new(x, y),
            text,
            0,
            DefaultFontSize);
}
