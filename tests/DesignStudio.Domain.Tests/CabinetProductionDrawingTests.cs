using DesignStudio.Application.Documentation;
using DesignStudio.Application.Products;
using DesignStudio.Domain.Documentation;
using DesignStudio.Domain.Project;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetProductionDrawingTests
{
    [Fact]
    public void FrontElevationDrawing_UsesElementGeometry()
    {
        var project =
            new ProjectModel("Front Elevation Drawing Test");

        var created =
            new CabinetService().Create(
                project,
                shelfCount: 2,
                doorCount: 2);

        var cutList =
            new CabinetCutListService().Build(created.Generation);

        var documents =
            new CabinetProductionDocumentationService().Build(
                project,
                created.Cabinet,
                created.Generation,
                cutList);

        var layout =
            documents.ShopDrawings.FrontElevationLayout;

        Assert.NotNull(layout);

        var drawing =
            new CabinetProductionDrawingBuilder().Build(documents);

        const double viewX = 25;
        const double viewY = 65;
        const double viewWidth = 150;
        const double viewHeight = 100;

        var scaleW =
            viewWidth / layout!.OverallWidth.Millimeters;

        var scaleH =
            viewHeight / layout.OverallHeight.Millimeters;

        var scale =
            Math.Min(scaleW, scaleH);

        var contentWidth =
            layout.OverallWidth.Millimeters * scale;

        var contentHeight =
            layout.OverallHeight.Millimeters * scale;

        var originX =
            viewX +
            (viewWidth - contentWidth) / 2;

        var originY =
            viewY +
            (viewHeight - contentHeight) / 2;

        foreach (var element in layout.Elements)
        {
            if (element.Width.Millimeters <= 0 ||
                element.Height.Millimeters <= 0)
            {
                continue;
            }

            var expectedX =
                originX +
                element.X.Millimeters * scale;

            var expectedY =
                originY +
                (layout.OverallHeight.Millimeters
                 - element.Y.Millimeters
                 - element.Height.Millimeters) * scale;

            var expectedWidth =
                element.Width.Millimeters * scale;

            var expectedHeight =
                element.Height.Millimeters * scale;

            var match =
                drawing.Primitives.SingleOrDefault(
                    primitive =>
                        primitive.Type ==
                        DrawingPrimitiveType.Rectangle &&
                        NearlyEqual(
                            primitive.Start.X,
                            expectedX) &&
                        NearlyEqual(
                            primitive.Start.Y,
                            expectedY) &&
                        NearlyEqual(
                            primitive.End.X,
                            expectedX + expectedWidth) &&
                        NearlyEqual(
                            primitive.End.Y,
                            expectedY + expectedHeight));

            Assert.True(
                match is not null,
                $"Missing rectangle for {element.ElementType} {element.Index}.");
        }
    }

    [Fact]
    public void FrontElevationDrawing_ContainsShelfAndDoorLabels()
    {
        var project =
            new ProjectModel("Front Elevation Labels Test");

        var created =
            new CabinetService().Create(
                project,
                shelfCount: 2,
                doorCount: 2);

        var cutList =
            new CabinetCutListService().Build(created.Generation);

        var documents =
            new CabinetProductionDocumentationService().Build(
                project,
                created.Cabinet,
                created.Generation,
                cutList);

        var drawing =
            new CabinetProductionDrawingBuilder().Build(documents);

            var shelfLabels =
            drawing.Primitives
                .Where(
                    x =>
                        x.Type == DrawingPrimitiveType.Text &&
                        x.Text is "Shelf 1" or "Shelf 2")
                .Select(x => x.Text)
                .ToList();

        var doorLabels =
            drawing.Primitives
                .Where(
                    x =>
                        x.Type == DrawingPrimitiveType.Text &&
                        x.Text.StartsWith("Door ",
                            StringComparison.Ordinal) &&
                        x.Text.Length > 5 &&
                        int.TryParse(x.Text.AsSpan(5), out _))
                .Select(x => x.Text)
                .ToList();

        Assert.Equal(
            new[] { "Shelf 1", "Shelf 2" },
            shelfLabels);

        Assert.Equal(
            new[] { "Door 1", "Door 2" },
            doorLabels);
    }

    private static bool NearlyEqual(
        double left,
        double right,
        double tolerance = 0.000001)
    {
        return Math.Abs(left - right) <= tolerance;
    }
}
