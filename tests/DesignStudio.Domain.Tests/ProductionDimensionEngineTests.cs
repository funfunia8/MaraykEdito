using DesignStudio.Application.Products;
using DesignStudio.Application.Documentation;
using DesignStudio.Domain.Documentation;

namespace DesignStudio.Domain.Tests;

public sealed class ProductionDimensionEngineTests
{
    [Fact]
    public void HorizontalDimension_ProducesExtensionsDimensionLineArrowsAndLabel()
    {
        var primitives = new List<DrawingPrimitive>();
        var engine = new ProductionDimensionEngine();

        engine.AddHorizontalDimension(
            primitives,
            x1: 20,
            x2: 120,
            geometryY: 80,
            dimensionY: 95,
            label: "600 mm");

        Assert.Equal(8, primitives.Count);
        Assert.Equal(7, primitives.Count(x => x.Type == DrawingPrimitiveType.Line));

        var label = primitives.Single(x =>
            x.Type == DrawingPrimitiveType.Text);

        Assert.Equal("600 mm", label.Text);
        Assert.Equal(70, label.Start.X);
    }

    [Fact]
    public void VerticalDimension_ProducesExtensionsDimensionLineArrowsAndLabel()
    {
        var primitives = new List<DrawingPrimitive>();
        var engine = new ProductionDimensionEngine();

        engine.AddVerticalDimension(
            primitives,
            x1: 100,
            y1: 30,
            y2: 130,
            dimensionX: 85,
            label: "2000 mm");

        Assert.Equal(8, primitives.Count);
        Assert.Equal(7, primitives.Count(x => x.Type == DrawingPrimitiveType.Line));

        var label = primitives.Single(x =>
            x.Type == DrawingPrimitiveType.Text);

        Assert.Equal("2000 mm", label.Text);
        Assert.Equal(85 - 2, label.Start.X);
        Assert.Equal(80, label.Start.Y);
    }

    [Fact]
    public void Builder_ContainsOverallDimensionLabels()
    {
        var project = new DesignStudio.Domain.Project.Project("Dimension Builder Test");

        var created = new CabinetService().Create(
            project,
            shelfCount: 2,
            doorCount: 2);

        var cutList = new CabinetCutListService()
            .Build(created.Generation);

        var nesting = new CabinetNestingService()
            .Build(project, cutList);

        var documents = new CabinetProductionDocumentationService()
            .Build(
                project,
                created.Cabinet,
                created.Generation,
                cutList,
                nesting);

        var page = new CabinetProductionDrawingBuilder()
            .Build(documents);

        Assert.Contains(
            page.Primitives,
            x => x.Type == DrawingPrimitiveType.Text &&
                 x.Text == $"{created.Cabinet.Width.Millimeters:0.##} mm");

        Assert.Contains(
            page.Primitives,
            x => x.Type == DrawingPrimitiveType.Text &&
                 x.Text == $"{created.Cabinet.Height.Millimeters:0.##} mm");

        Assert.True(
            page.Primitives.Count(x => x.Type == DrawingPrimitiveType.Line) >= 20);
    }
}
