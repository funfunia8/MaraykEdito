using DesignStudio.Application.Documentation;
using DesignStudio.Application.Products;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetProductionDocumentationTests
{
    [Fact]
    public void ProductionDocuments_ContainViewsPartsCutListAndAssembly()
    {
        var project = new ProjectModel("Documentation Test");
        var service = new CabinetService();
        var created = service.Create(project, shelfCount: 2, doorCount: 2);

        var cutList = new CabinetCutListService().Build(created.Generation);
        var nesting = new CabinetNestingService().Build(project, cutList);
        var docs = new CabinetProductionDocumentationService().Build(
            project,
            created.Cabinet,
            created.Generation,
            cutList,
            nesting);

        Assert.Equal(created.Cabinet.Id, docs.CabinetId);
        Assert.Equal(3, docs.ShopDrawings.Views.Count);
        Assert.Equal(created.Generation.Parts.Count, docs.ShopDrawings.PartViews.Count);
        Assert.Equal(cutList.Lines.Count, docs.CutDocument.Lines.Count);
        Assert.NotEmpty(docs.AssemblyDocument.Steps);
        Assert.Equal("A", docs.Revision);
    }

    [Fact]
    public void ProductionDocuments_UseMaterialCodesAndNestingPlacements()
    {
        var project = new ProjectModel("Documentation Mapping Test");
        var created = new CabinetService().Create(project, shelfCount: 1, doorCount: 1);
        var cutList = new CabinetCutListService().Build(created.Generation);
        var nesting = new CabinetNestingService().Build(project, cutList);

        var docs = new CabinetProductionDocumentationService().Build(
            project,
            created.Cabinet,
            created.Generation,
            cutList,
            nesting,
            "b");

        Assert.Equal("B", docs.Revision);
        Assert.All(docs.CutDocument.Lines, x => Assert.False(string.IsNullOrWhiteSpace(x.MaterialCode)));
        Assert.Equal(
            nesting.Sheets.SelectMany(x => x.Placements).Count(),
            docs.CutDocument.Sheets.SelectMany(x => x.Placements).Count());
    }

    [Fact]
    public void ProductionDocuments_AssemblyMentionsBackSpecification()
    {
        var project = new ProjectModel("Assembly Test");
        var created = new CabinetService().Create(project, shelfCount: 0, doorCount: 0, backThicknessMm: 8);
        var cutList = new CabinetCutListService().Build(created.Generation);

        var docs = new CabinetProductionDocumentationService().Build(
            project,
            created.Cabinet,
            created.Generation,
            cutList);

        var backStep = docs.AssemblyDocument.Steps.Single(x => x.Sequence == 30);
        Assert.Contains("Groove width", backStep.Instruction);
        Assert.Contains("Groove depth", backStep.Instruction);
    }
}
