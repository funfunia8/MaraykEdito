using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Application.Products;
using DesignStudio.Domain.Manufacturing;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetHardwarePlanningTests
{
    [Fact]
    public void DoorCountAndHeightDriveHingeQuantity()
    {
        var project = new ProjectModel("Hardware Test");
        project.Add(new HardwareItem("HINGE-110", "110 Degree Hinge", HardwareKind.Hinge));
        var cabinet = new Cabinet("Tall", Length.FromMillimeters(900), Length.FromMillimeters(1900), Length.FromMillimeters(450), Length.FromMillimeters(18), Length.FromMillimeters(8), doorCount: 2);

        var plan = new CabinetHardwarePlanningService().Build(project, cabinet);

        var hinges = Assert.Single(plan.Lines, x => x.Kind == HardwareKind.Hinge);
        Assert.Equal(8, hinges.Quantity);
        Assert.Equal("HINGE-110", hinges.Code);
        Assert.NotNull(hinges.CatalogItemId);
    }

    [Fact]
    public void ShelfCountProducesFourPinsPerShelfByDefault()
    {
        var project = new ProjectModel("Hardware Test");
        var cabinet = new Cabinet("Shelf", Length.FromMillimeters(800), Length.FromMillimeters(900), Length.FromMillimeters(400), Length.FromMillimeters(18), Length.FromMillimeters(6), shelfCount: 3, doorCount: 0);

        var plan = new CabinetHardwarePlanningService().Build(project, cabinet);

        var pins = Assert.Single(plan.Lines, x => x.Kind == HardwareKind.ShelfPin);
        Assert.Equal(12, pins.Quantity);
    }
}
