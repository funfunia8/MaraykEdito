using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Products;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Manufacturing;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class ManufacturingDefinitionContractTests
{
    [Fact]
    public void Build_CreatesGenericManufacturingDefinition()
    {
        var project = new ProjectModel("Manufacturing Contract");

        var cabinet = new Cabinet(
            "Cabinet",
            Length.FromMillimeters(600),
            Length.FromMillimeters(720),
            Length.FromMillimeters(560),
            Length.FromMillimeters(18),
            Length.FromMillimeters(6));

        project.Add(cabinet);

        var product = new ProductDefinition(
            EntityId.New(),
            "CAB-BASE",
            "Base Cabinet",
            ProductCategory.Kitchen);

        project.AddProductDefinition(product);
        cabinet.SetProductDefinition(product.Id);

        var service = new ManufacturingDefinitionService();

        var definition = service.Build(
            project,
            cabinet,
            "CAB-MFG-001",
            "b",
            product.Id);

        Assert.NotEqual(default, definition.Id);
        Assert.Equal(cabinet.Id, definition.SourceDesignObjectId);
        Assert.Equal(product.Id, definition.ProductDefinitionId);
        Assert.Equal("CAB-MFG-001", definition.Code);
        Assert.Equal("B", definition.Revision);

        Assert.Empty(definition.Parts);
        Assert.Empty(definition.Hardware);
        Assert.Empty(definition.Operations);
        Assert.Empty(definition.Assemblies);
    }

    [Fact]
    public void Build_NormalizesMissingRevisionToA()
    {
        var project = new ProjectModel("Manufacturing Revision");

        var cabinet = new Cabinet(
            "Cabinet",
            Length.FromMillimeters(600),
            Length.FromMillimeters(720),
            Length.FromMillimeters(560),
            Length.FromMillimeters(18),
            Length.FromMillimeters(6));

        project.Add(cabinet);

        var definition = new ManufacturingDefinitionService().Build(
            project,
            cabinet,
            "CAB-MFG-001",
            string.Empty);

        Assert.Equal("A", definition.Revision);
    }

    [Fact]
    public void Build_RejectsDesignObjectOutsideProject()
    {
        var project = new ProjectModel("Foreign Object");

        var foreignCabinet = new Cabinet(
            "Foreign",
            Length.FromMillimeters(600),
            Length.FromMillimeters(720),
            Length.FromMillimeters(560),
            Length.FromMillimeters(18),
            Length.FromMillimeters(6));

        var service = new ManufacturingDefinitionService();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.Build(project, foreignCabinet, "CAB-MFG-001"));

        Assert.Contains("does not belong", exception.Message);
    }

    [Fact]
    public void Build_RejectsUnknownProductDefinition()
    {
        var project = new ProjectModel("Unknown Product Definition");

        var cabinet = new Cabinet(
            "Cabinet",
            Length.FromMillimeters(600),
            Length.FromMillimeters(720),
            Length.FromMillimeters(560),
            Length.FromMillimeters(18),
            Length.FromMillimeters(6));

        project.Add(cabinet);

        var unknownProductDefinitionId = EntityId.New();

        var service = new ManufacturingDefinitionService();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.Build(
                project,
                cabinet,
                "CAB-MFG-001",
                "A",
                unknownProductDefinitionId));

        Assert.Contains("Product definition", exception.Message);
    }

    [Fact]
    public void ManufacturingPart_SupportsGenericDepthAndEdgeBanding()
    {
        var part = new ManufacturingPart(
            "SIDE-L",
            "Left Side",
            Length.FromMillimeters(600),
            Length.FromMillimeters(720),
            Length.FromMillimeters(18),
            1,
            MaterialId: null,
            EdgeBandMaterialId: null,
            GrainAlongWidth: false,
            SourceComponent: "LeftSide",
            Depth: Length.FromMillimeters(560),
            EdgeBanding: ManufacturingEdgeFlags.Front);

        Assert.Equal(560, part.Depth?.Millimeters);
        Assert.Equal(ManufacturingEdgeFlags.Front, part.EdgeBanding);
        Assert.Equal("LeftSide", part.SourceComponent);
    }

    [Fact]
    public void ManufacturingPart_CanRepresentMultipleEdgeBands()
    {
        var part = new ManufacturingPart(
            "DOOR",
            "Door",
            Length.FromMillimeters(300),
            Length.FromMillimeters(700),
            Length.FromMillimeters(18),
            2,
            EdgeBandMaterialId: EntityId.New(),
            EdgeBanding:
                ManufacturingEdgeFlags.Left |
                ManufacturingEdgeFlags.Right |
                ManufacturingEdgeFlags.Top |
                ManufacturingEdgeFlags.Bottom);

        Assert.Equal(
            ManufacturingEdgeFlags.Left |
            ManufacturingEdgeFlags.Right |
            ManufacturingEdgeFlags.Top |
            ManufacturingEdgeFlags.Bottom,
            part.EdgeBanding);
    }
}