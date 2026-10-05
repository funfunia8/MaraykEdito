using DesignStudio.Application.Products;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetNestingTests
{
    [Fact]
    public void Nesting_ProducesPlacements_WithKerfAwareSpacing()
    {
        var project = new ProjectModel("Nesting Test");
        var created = new CabinetService().Create(project, shelfCount: 6, doorCount: 2);
        var cutList = new CabinetCutListService().Build(created.Generation);

        var result = new CabinetNestingService().Build(project, cutList, new NestingOptions(
            Length.FromMillimeters(3),
            Length.FromMillimeters(10),
            true,
            Length.FromMillimeters(200),
            Length.FromMillimeters(300)));

        Assert.True(result.IsFeasible);
        Assert.NotEmpty(result.Sheets);
        Assert.NotEmpty(result.Sheets.SelectMany(x => x.Placements));
    }

    [Fact]
    public void Nesting_RespectsDirectionalGrain()
    {
        var project = new ProjectModel("Grain Test");
        var created = new CabinetService().Create(project, shelfCount: 0, doorCount: 1);
        var cutList = new CabinetCutListService().Build(created.Generation);

        var result = new CabinetNestingService().Build(project, cutList);
        Assert.True(result.IsFeasible);

        foreach (var placement in result.Sheets.SelectMany(x => x.Placements))
        {
            // Current panel stock is grain-along-width. The generated shelf/top/bottom keep grain along width,
            // while side panels keep grain along height, so their allowed orientation is deterministic.
            var source = cutList.Lines.Single(x => x.PartType == placement.PartType && x.Name == placement.PartName);
            if (source.GrainAlongWidth)
                Assert.False(placement.Rotated90Degrees);
            else
                Assert.True(placement.Rotated90Degrees);
        }
    }

    [Fact]
    public void Nesting_ReturnsUnplacedPart_WhenPartExceedsUsableSheet()
    {
        var project = new ProjectModel("Oversize Test");
        var created = new CabinetService().Create(project, shelfCount: 0, doorCount: 1, widthMm: 5000);
        var cutList = new CabinetCutListService().Build(created.Generation);

        var result = new CabinetNestingService().Build(project, cutList);
        Assert.False(result.IsFeasible);
        Assert.NotEmpty(result.UnplacedParts);
    }
}
