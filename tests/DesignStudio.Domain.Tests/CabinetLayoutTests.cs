using DesignStudio.Application.Products;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetLayoutTests
{
    [Fact]
    public void Build_CalculatesClearOpeningFromOverallDimensions()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 0,
            doorCount: 0);

        var layout = new CabinetLayoutService().Build(cabinet);

        Assert.Equal(564, layout.ClearWidth.Millimeters);
        Assert.Equal(684, layout.ClearHeight.Millimeters);
    }

    [Fact]
    public void Build_WithNoShelves_ReturnsNoShelfLayouts()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 0,
            doorCount: 0);

        var layout = new CabinetLayoutService().Build(cabinet);

        Assert.Empty(layout.Shelves);
    }

    [Fact]
    public void Build_WithOneShelf_PlacesShelfInsideClearOpening()
    {
       var cabinet = CreateCabinet(
    width: 600,
    height: 720,
    depth: 560,
    panel: 18,
    shelfCount: 1,
    doorCount: 0);

var layout = new CabinetLayoutService().Build(cabinet);
var shelf = Assert.Single(layout.Shelves);

Assert.Equal(1, shelf.Index);

Assert.Equal(18, shelf.X.Millimeters);
Assert.Equal(564, shelf.Width.Millimeters);

Assert.Equal(
    cabinet.PanelThickness.Millimeters,
    shelf.X.Millimeters);

Assert.Equal(
    cabinet.Width.Millimeters - cabinet.PanelThickness.Millimeters,
    shelf.X.Millimeters + shelf.Width.Millimeters);

Assert.Equal(560, shelf.Depth.Millimeters);
Assert.Equal(18, shelf.Thickness.Millimeters);

Assert.Equal(351, shelf.Y.Millimeters);

Assert.True(shelf.Y.Millimeters > 0);

Assert.True(
    shelf.Y.Millimeters + shelf.Thickness.Millimeters
    <= cabinet.Height.Millimeters - cabinet.PanelThickness.Millimeters);
    }

    [Fact]
    public void Build_WithTwoShelves_DistributesShelvesVertically()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 2,
            doorCount: 0);

        var layout = new CabinetLayoutService().Build(cabinet);

        Assert.Equal(2, layout.Shelves.Count);

        var first = layout.Shelves[0];
        var second = layout.Shelves[1];

        Assert.Equal(1, first.Index);
        Assert.Equal(2, second.Index);

        Assert.Equal(18, first.X.Millimeters);
Assert.Equal(18, second.X.Millimeters);
Assert.Equal(234, first.Y.Millimeters);
Assert.Equal(468, second.Y.Millimeters);

        Assert.Equal(564, first.Width.Millimeters);
        Assert.Equal(564, second.Width.Millimeters);

        Assert.True(first.Y.Millimeters > 0);
        Assert.True(second.Y.Millimeters > first.Y.Millimeters);

        Assert.True(
            first.Y.Millimeters + first.Thickness.Millimeters
            <= second.Y.Millimeters);

        Assert.True(
            second.Y.Millimeters + second.Thickness.Millimeters
            <= layout.ClearHeight.Millimeters);
    }

    [Fact]
    public void Build_WithNoDoors_ReturnsNoDoorLayouts()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 0,
            doorCount: 0);

        var layout = new CabinetLayoutService().Build(cabinet);

        Assert.Empty(layout.Doors);
    }

    [Fact]
    public void Build_WithOneDoor_FillsClearOpening()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 0,
            doorCount: 1);

        var layout = new CabinetLayoutService().Build(cabinet);
        var door = Assert.Single(layout.Doors);

        Assert.Equal(1, door.Index);
        Assert.Equal(18, door.X.Millimeters);
        Assert.Equal(18, door.Y.Millimeters);
        Assert.Equal(564, door.Width.Millimeters);
        Assert.Equal(684, door.Height.Millimeters);
        Assert.Equal(18, door.Thickness.Millimeters);

        Assert.Equal(
            600,
            door.X.Millimeters + door.Width.Millimeters
            + 18);
    }

    [Fact]
    public void Build_WithTwoDoors_DistributesDoorsAcrossClearWidth()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 0,
            doorCount: 2);

        var layout = new CabinetLayoutService().Build(cabinet);

        Assert.Equal(2, layout.Doors.Count);

        var first = layout.Doors[0];
        var second = layout.Doors[1];

        Assert.Equal(1, first.Index);
        Assert.Equal(2, second.Index);

        Assert.Equal(18, first.X.Millimeters);
        Assert.Equal(300, second.X.Millimeters);

        Assert.Equal(18, first.Y.Millimeters);
        Assert.Equal(18, second.Y.Millimeters);

        Assert.Equal(282, first.Width.Millimeters);
        Assert.Equal(282, second.Width.Millimeters);

        Assert.Equal(684, first.Height.Millimeters);
        Assert.Equal(684, second.Height.Millimeters);

        Assert.Equal(
            564,
            first.Width.Millimeters + second.Width.Millimeters);

        Assert.Equal(
            first.X.Millimeters + first.Width.Millimeters,
            second.X.Millimeters);
    }

    [Fact]
    public void Build_WithThreeDoors_CoversEntireClearWidthWithoutOverlap()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 0,
            doorCount: 3);

        var layout = new CabinetLayoutService().Build(cabinet);

        Assert.Equal(3, layout.Doors.Count);

        var totalWidth = layout.Doors.Sum(x => x.Width.Millimeters);

        Assert.Equal(
            layout.ClearWidth.Millimeters,
            totalWidth,
            precision: 8);

        for (var i = 0; i < layout.Doors.Count; i++)
        {
            var door = layout.Doors[i];

            Assert.Equal(i + 1, door.Index);
            Assert.Equal(18, door.Y.Millimeters);
            Assert.Equal(684, door.Height.Millimeters);

            Assert.True(door.X.Millimeters >= 18);
            Assert.True(
                door.X.Millimeters + door.Width.Millimeters
                <= 582);

            if (i > 0)
            {
                var previous = layout.Doors[i - 1];

                Assert.Equal(
                    previous.X.Millimeters + previous.Width.Millimeters,
                    door.X.Millimeters,
                    precision: 8);
            }
        }
    }

    [Fact]
    public void Build_ShelvesRemainInsideClearOpening()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 4,
            doorCount: 0);

        var layout = new CabinetLayoutService().Build(cabinet);

        Assert.Equal(4, layout.Shelves.Count);

        foreach (var shelf in layout.Shelves)
        {
           Assert.True(
    shelf.X.Millimeters >= cabinet.PanelThickness.Millimeters);

Assert.True(shelf.Y.Millimeters >= 0);

Assert.True(
    shelf.X.Millimeters + shelf.Width.Millimeters
    <= cabinet.Width.Millimeters);

            Assert.True(
    shelf.Y.Millimeters + shelf.Thickness.Millimeters
    <= cabinet.Height.Millimeters - cabinet.PanelThickness.Millimeters);
        }
    }

    [Fact]
    public void Build_DoorsRemainInsideOverallCabinetBounds()
    {
        var cabinet = CreateCabinet(
            width: 600,
            height: 720,
            depth: 560,
            panel: 18,
            shelfCount: 0,
            doorCount: 4);

        var layout = new CabinetLayoutService().Build(cabinet);

        foreach (var door in layout.Doors)
        {
            Assert.True(door.X.Millimeters >= 0);
            Assert.True(door.Y.Millimeters >= 0);

            Assert.True(
                door.X.Millimeters + door.Width.Millimeters
                <= cabinet.Width.Millimeters);

            Assert.True(
                door.Y.Millimeters + door.Height.Millimeters
                <= cabinet.Height.Millimeters);
        }
    }

    private static Cabinet CreateCabinet(
        double width,
        double height,
        double depth,
        double panel,
        int shelfCount,
        int doorCount)
    {
        return new Cabinet(
            "Layout Test Cabinet",
            Length.FromMillimeters(width),
            Length.FromMillimeters(height),
            Length.FromMillimeters(depth),
            Length.FromMillimeters(panel),
            Length.FromMillimeters(6),
            CabinetConstructionMethod.CapturedBackGroove,
            shelfCount,
            doorCount);
    }
}
