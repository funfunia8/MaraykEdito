using DesignStudio.Application.Documentation;
using DesignStudio.Application.Products;
using DesignStudio.Domain.Documentation;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class ProductionDrawingBuilderTests
{
    [Fact]
    public void Builder_Produces_Production_Page_With_Core_Sections()
    {
        var project = new ProjectModel("Drawing Builder Test");

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

        Assert.Equal("Cabinet Production Sheet", page.Title);
        Assert.Equal(documents.DocumentCode, page.DocumentCode);
        Assert.Equal("A", page.Revision);
        Assert.Equal(420, page.WidthMm);
        Assert.Equal(297, page.HeightMm);

        Assert.Contains(
            page.Primitives,
            x => x.Type == DrawingPrimitiveType.Text &&
                 x.Text == "CUT DOCUMENT");

        Assert.Contains(
            page.Primitives,
            x => x.Type == DrawingPrimitiveType.Text &&
                 x.Text == "ASSEMBLY");

        Assert.Contains(
            page.Primitives,
            x => x.Type == DrawingPrimitiveType.Text &&
                 x.Text == "HARDWARE");

        Assert.Contains(
            page.Primitives,
            x => x.Type == DrawingPrimitiveType.Rectangle);

        Assert.NotEmpty(page.Primitives);
    }
}
