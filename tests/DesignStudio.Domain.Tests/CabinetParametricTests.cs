using DesignStudio.Application.Commands;
using DesignStudio.Application.Products;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetParametricTests
{
    [Fact]
    public void DefaultCabinet_GeneratesExpectedCoreParts()
    {
        var project = new ProjectModel("Cabinet Test");
        var service = new CabinetService();
        var created = service.Create(project);

        Assert.True(created.Generation.Validation.IsValid);
        Assert.Equal(7, created.Generation.Parts.Count);
        Assert.Contains(created.Generation.Parts, x => x.Type == CabinetPartType.LeftSide);
        Assert.Contains(created.Generation.Parts, x => x.Type == CabinetPartType.Back);
        Assert.Contains(created.Generation.Parts, x => x.Type == CabinetPartType.Door && x.Quantity == 1);
    }

    [Fact]
    public void BackThickness_PropagatesIntoConstructionAndBackPartDimensions()
    {
        var cabinet = new Cabinet(
            "Cabinet",
            Length.FromMillimeters(600), Length.FromMillimeters(720), Length.FromMillimeters(560),
            Length.FromMillimeters(18), Length.FromMillimeters(6));
        var service = new CabinetParametricService();

        var thin = service.Generate(cabinet);
        var thinBack = thin.Parts.Single(x => x.Type == CabinetPartType.Back);

        cabinet.SetBackThickness(Length.FromMillimeters(8));
        var thick = service.Generate(cabinet);
        var thickBack = thick.Parts.Single(x => x.Type == CabinetPartType.Back);

        Assert.True(thick.ManufacturingSpec.GrooveWidth.Millimeters > thin.ManufacturingSpec.GrooveWidth.Millimeters);
        Assert.True(thick.ManufacturingSpec.GrooveDepth.Millimeters >= thin.ManufacturingSpec.GrooveDepth.Millimeters);
        Assert.True(thickBack.Width.Millimeters > thinBack.Width.Millimeters);
        Assert.True(thickBack.Height.Millimeters > thinBack.Height.Millimeters);
    }

    [Fact]
    public void ResizeCabinet_UndoRestoresPreviousParameters()
    {
        var cabinet = new Cabinet(
            "Cabinet",
            Length.FromMillimeters(600), Length.FromMillimeters(720), Length.FromMillimeters(560),
            Length.FromMillimeters(18), Length.FromMillimeters(6));
        var history = new CommandHistory();
        var command = new ResizeCabinetCommand(cabinet, 800, 900, 600, 18, 8, 3, 2, CabinetConstructionMethod.CapturedBackGroove);

        history.Execute(command);
        Assert.Equal(800, cabinet.Width.Millimeters);
        Assert.Equal(8, cabinet.BackThickness.Millimeters);
        Assert.Equal(3, cabinet.ShelfCount);

        history.Undo();
        Assert.Equal(600, cabinet.Width.Millimeters);
        Assert.Equal(6, cabinet.BackThickness.Millimeters);
        Assert.Equal(2, cabinet.ShelfCount);
    }

    [Fact]
    public async Task Cabinet_RoundTripsThroughProjectSerializerWithStableId()
    {
        var project = new ProjectModel("Cabinet Storage");
        var service = new CabinetService();
        var created = service.Create(project);
        var serializer = new JsonProjectSerializer();
        await using var stream = new MemoryStream();

        await serializer.SaveAsync(project, stream);
        stream.Position = 0;
        var restored = await serializer.LoadAsync(stream);
        var cabinet = restored.Objects.OfType<Cabinet>().Single();

        Assert.Equal(created.Cabinet.Id, cabinet.Id);
        Assert.Equal(created.Cabinet.BackThickness, cabinet.BackThickness);
        Assert.Equal(created.Cabinet.ConstructionMethod, cabinet.ConstructionMethod);
    }
}
