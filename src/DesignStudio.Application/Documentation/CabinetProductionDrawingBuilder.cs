using DesignStudio.Domain.Documentation;

namespace DesignStudio.Application.Documentation;

public sealed class CabinetProductionDrawingBuilder : IProductionDrawingBuilder
{
    private const double PageWidthMm = 420;
    private const double PageHeightMm = 297;

    public ProductionDrawingPage Build(ProductionDocumentSet documentSet)
    {
        ArgumentNullException.ThrowIfNull(documentSet);

        var primitives = new List<DrawingPrimitive>();
        AddTitleBlock(primitives, documentSet);

        var front = documentSet.ShopDrawings.Views
            .FirstOrDefault(x => x.ViewType == ShopDrawingViewType.FrontElevation);
        var side = documentSet.ShopDrawings.Views
            .FirstOrDefault(x => x.ViewType == ShopDrawingViewType.SideElevation);
        var top = documentSet.ShopDrawings.Views
            .FirstOrDefault(x => x.ViewType == ShopDrawingViewType.TopPlan);

        if (front is not null)
            AddView(primitives, front, 25, 65, 150, 100);
        if (side is not null)
            AddView(primitives, side, 205, 65, 90, 100);
        if (top is not null)
            AddView(primitives, top, 315, 65, 80, 70);

        AddCutSummary(primitives, documentSet, 25, 190);
        AddAssemblySummary(primitives, documentSet, 250, 190);
        AddHardwareSummary(primitives, documentSet, 250, 242);

        return new ProductionDrawingPage(
            "Cabinet Production Sheet",
            documentSet.DocumentCode,
            documentSet.Revision,
            PageWidthMm,
            PageHeightMm,
            primitives);
    }

    private static void AddTitleBlock(
        List<DrawingPrimitive> primitives,
        ProductionDocumentSet documentSet)
    {
        primitives.Add(Text(15, 15, documentSet.DocumentCode, 5));
        primitives.Add(Text(15, 23, "CABINET PRODUCTION SHEET", 4));
        primitives.Add(Text(350, 15, $"REV {documentSet.Revision}", 4));
        primitives.Add(Line(15, 28, 405, 28));
    }

    private static void AddView(
        List<DrawingPrimitive> primitives,
        ShopDrawingView view,
        double x,
        double y,
        double width,
        double height)
    {
        primitives.Add(Text(x, y - 7, view.Title, 3.5));
        primitives.Add(Rect(x, y, width, height));

        var scaleW = width / Math.Max(1, view.Width.Millimeters);
        var scaleH = height / Math.Max(1, view.Height.Millimeters);
        var scale = Math.Min(scaleW, scaleH);

        var contentW = view.Width.Millimeters * scale;
        var contentH = view.Height.Millimeters * scale;
        var ox = x + (width - contentW) / 2;
        var oy = y + (height - contentH) / 2;

        primitives.Add(Rect(ox, oy, contentW, contentH));
        primitives.Add(Text(
            x,
            y + height + 6,
            $"W {view.Width.Millimeters:0.##} x H {view.Height.Millimeters:0.##} mm",
            3));

        var noteIndex = 0;
        foreach (var note in view.Notes.Take(2))
        {
            primitives.Add(Text(x, y + height + 12 + noteIndex * 5, note, 2.7));
            noteIndex++;
        }
    }

    private static void AddCutSummary(
        List<DrawingPrimitive> primitives,
        ProductionDocumentSet documentSet,
        double x,
        double y)
    {
        primitives.Add(Text(x, y, "CUT DOCUMENT", 4));
        var lineY = y + 8;
        primitives.Add(Line(x, lineY - 3, x + 205, lineY - 3));

        foreach (var line in documentSet.CutDocument.Lines.Take(8))
        {
            var material = string.IsNullOrWhiteSpace(line.MaterialCode) ? "UNASSIGNED" : line.MaterialCode;
            primitives.Add(Text(
                x,
                lineY,
                $"{line.Quantity}x {line.PartName}  {line.Width.Millimeters:0.#}x{line.Height.Millimeters:0.#}  {material}",
                2.7));
            lineY += 5;
        }
    }

    private static void AddAssemblySummary(
        List<DrawingPrimitive> primitives,
        ProductionDocumentSet documentSet,
        double x,
        double y)
    {
        primitives.Add(Text(x, y, "ASSEMBLY", 4));
        var lineY = y + 8;

        foreach (var step in documentSet.AssemblyDocument.Steps.Take(8))
        {
            primitives.Add(Text(x, lineY, $"{step.Sequence}: {step.Title}", 2.7));
            lineY += 5;
        }
    }


    private static void AddHardwareSummary(
        List<DrawingPrimitive> primitives,
        ProductionDocumentSet documentSet,
        double x,
        double y)
    {
        primitives.Add(Text(x, y, "HARDWARE", 4));
        var lineY = y + 8;
        foreach (var line in documentSet.AssemblyDocument.HardwareLines.Take(6))
        {
            primitives.Add(Text(x, lineY, $"{line.Quantity}x {line.Code}  {line.Name}", 2.7));
            lineY += 5;
        }
    }

    private static DrawingPrimitive Line(double x1, double y1, double x2, double y2) =>
        new(DrawingPrimitiveType.Line, new(x1, y1), new(x2, y2), string.Empty, 0.4, 0);

    private static DrawingPrimitive Rect(double x, double y, double width, double height) =>
        new(
            DrawingPrimitiveType.Rectangle,
            new(x, y),
            new(x + width, y + height),
            string.Empty,
            0.4,
            0);

    private static DrawingPrimitive Text(double x, double y, string text, double fontSize) =>
        new(
            DrawingPrimitiveType.Text,
            new(x, y),
            new(x, y),
            text,
            0,
            fontSize);
}
