using System.Globalization;
using System.Net;
using System.Text;
using DesignStudio.Domain.Documentation;

namespace DesignStudio.Application.Documentation;

public sealed class SvgProductionDrawingExporter
{
    public string Export(ProductionDrawingPage page)
    {
        ArgumentNullException.ThrowIfNull(page);

        var sb = new StringBuilder();
        sb.AppendLine(
            $"""<svg xmlns="http://www.w3.org/2000/svg" width="{F(page.WidthMm)}mm" height="{F(page.HeightMm)}mm" viewBox="0 0 {F(page.WidthMm)} {F(page.HeightMm)}">""");
        sb.AppendLine("""<rect x="0" y="0" width="100%" height="100%" fill="white"/>""");

        foreach (var primitive in page.Primitives)
        {
            switch (primitive.Type)
            {
                case DrawingPrimitiveType.Line:
                    sb.AppendLine(
                        $"""<line x1="{F(primitive.Start.X)}" y1="{F(primitive.Start.Y)}" x2="{F(primitive.End.X)}" y2="{F(primitive.End.Y)}" stroke="black" stroke-width="{F(primitive.StrokeWidth)}"/>""");
                    break;

                case DrawingPrimitiveType.Rectangle:
                    sb.AppendLine(
                        $"""<rect x="{F(primitive.Start.X)}" y="{F(primitive.Start.Y)}" width="{F(primitive.End.X - primitive.Start.X)}" height="{F(primitive.End.Y - primitive.Start.Y)}" fill="none" stroke="black" stroke-width="{F(primitive.StrokeWidth)}"/>""");
                    break;

                case DrawingPrimitiveType.Text:
                    sb.AppendLine(
                        $"""<text x="{F(primitive.Start.X)}" y="{F(primitive.Start.Y)}" font-family="Arial, sans-serif" font-size="{F(primitive.FontSize)}" fill="black">{WebUtility.HtmlEncode(primitive.Text)}</text>""");
                    break;
            }
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static string F(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}
