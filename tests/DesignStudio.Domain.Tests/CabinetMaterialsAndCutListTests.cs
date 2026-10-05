using DesignStudio.Application.Products;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetMaterialsAndCutListTests
{
    [Fact]
    public void CabinetCreation_AssignsDefaultMaterialsAndEdgeBand()
    {
        var project = new ProjectModel("Material Test");
        var created = new CabinetService().Create(project);

        Assert.NotNull(created.Cabinet.CarcassMaterialId);
        Assert.NotNull(created.Cabinet.BackMaterialId);
        Assert.NotNull(created.Cabinet.DoorMaterialId);
        Assert.NotNull(created.Cabinet.EdgeBandMaterialId);
        Assert.Equal(3, project.Objects.OfType<Material>().Count());
    }

    [Fact]
    public void CutList_GroupsIdenticalPartsAndCarriesMaterialRules()
    {
        var project = new ProjectModel("Cut List Test");
        var created = new CabinetService().Create(project, shelfCount: 4, doorCount: 2);
        var cutList = new CabinetCutListService().Build(created.Generation);

        Assert.Equal(7, cutList.Lines.Count);
        var shelf = cutList.Lines.Single(x => x.PartType == CabinetPartType.Shelf);
        Assert.Equal(4, shelf.Quantity);
        Assert.Equal(EdgeBandFlags.Front, shelf.EdgeBanding);
        Assert.NotNull(shelf.MaterialId);
        Assert.NotNull(shelf.EdgeBandMaterialId);
        Assert.True(shelf.AreaSquareMeters > 0);
    }

    [Fact]
    public void Preview_ProvidesRendererIndependentMainCarcassBoxes()
    {
        var project = new ProjectModel("Preview Test");
        var created = new CabinetService().Create(project);
        var preview = new CabinetPreviewService().Build(created.Cabinet, created.Generation);

        Assert.Equal(created.Cabinet.Width.Millimeters, preview.Width);
        Assert.Equal(created.Cabinet.Height.Millimeters, preview.Height);
        Assert.Contains(preview.Parts, x => x.PartType == CabinetPartType.LeftSide);
        Assert.Contains(preview.Parts, x => x.PartType == CabinetPartType.RightSide);
        Assert.Contains(preview.Parts, x => x.PartType == CabinetPartType.Top);
        Assert.Contains(preview.Parts, x => x.PartType == CabinetPartType.Bottom);
    }

    [Fact]
    public async Task MaterialsAndCabinetAssignments_RoundTripWithStableIds()
    {
        var project = new ProjectModel("Material Storage");
        var created = new CabinetService().Create(project);
        var serializer = new JsonProjectSerializer();
        await using var stream = new MemoryStream();

        await serializer.SaveAsync(project, stream);
        stream.Position = 0;
        var restored = await serializer.LoadAsync(stream);

        var restoredCabinet = restored.Objects.OfType<Cabinet>().Single();
        var restoredMaterials = restored.Objects.OfType<Material>().ToList();

        Assert.Equal(created.Cabinet.Id, restoredCabinet.Id);
        Assert.Equal(created.Cabinet.CarcassMaterialId, restoredCabinet.CarcassMaterialId);
        Assert.Equal(created.Cabinet.BackMaterialId, restoredCabinet.BackMaterialId);
        Assert.Equal(created.Cabinet.EdgeBandMaterialId, restoredCabinet.EdgeBandMaterialId);
        Assert.Equal(3, restoredMaterials.Count);
        Assert.Contains(restoredMaterials, x => x.Code == "PANEL-18");
    }

    [Fact]
    public void LegacyCabinetWithoutMaterials_RemainsGeneratableWithWarningsOnly()
    {
        var cabinet = new Cabinet(
            "Legacy", Length.FromMillimeters(600), Length.FromMillimeters(720), Length.FromMillimeters(560),
            Length.FromMillimeters(18), Length.FromMillimeters(6));
        var generation = new CabinetParametricService().Generate(cabinet);

        Assert.True(generation.Validation.IsValid);
        Assert.Contains(generation.Validation.Issues, x => x.Code == "CARCASS_MATERIAL_UNASSIGNED");
        Assert.Contains(generation.Validation.Issues, x => x.Code == "BACK_MATERIAL_UNASSIGNED");
    }
}
