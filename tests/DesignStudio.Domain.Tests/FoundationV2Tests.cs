using DesignStudio.Domain.Documentation;
using DesignStudio.Documentation;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Pricing;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Products;
using DesignStudio.Domain.Units;
using DesignStudio.Storage;
using System.Text;
using DesignStudio.Manufacturing;
using DesignStudio.Pricing;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class FoundationV2Tests
{
    [Fact]
    public void DesignObjectMetadata_ProvidesGenericHierarchyAndPlacement()
    {
        var building = new Building("Villa");
        var floor = new Floor("Ground Floor", building.Id);
        var room = new Room("Kitchen", floorId: floor.Id);
        room.SetPlacement(new Placement2D(new(1200, 2400), 450));

        Assert.Equal(ObjectCategory.Architecture, building.Category);
        Assert.Equal(building.Id, floor.ParentId);
        Assert.Equal(floor.Id, room.ParentId);
        Assert.Equal(90, room.Placement.RotationDegrees);
    }

    [Fact]
    public async Task DesignObjectMetadata_RoundTripsThroughStorage()
    {
        var project = new ProjectModel("Metadata Test");
        var floor = new Floor("Ground");
        var room = new Room("Living", floorId: floor.Id);
        room.SetPlacement(new Placement2D(new(500, 700), 270));
        room.SetVisibility(false);
        room.SetLocked(true);
        project.Add(floor);
        project.Add(room);

        await using var stream = new MemoryStream();
        var serializer = new JsonProjectSerializer();
        await serializer.SaveAsync(project, stream);
        stream.Position = 0;
        var restored = await serializer.LoadAsync(stream);
        var restoredRoom = restored.Get<Room>(room.Id);

        Assert.Equal(floor.Id, restoredRoom.ParentId);
        Assert.Equal(500, restoredRoom.Placement.Position.X);
        Assert.Equal(270, restoredRoom.Placement.RotationDegrees);
        Assert.False(restoredRoom.Metadata.IsVisible);
        Assert.True(restoredRoom.Metadata.IsLocked);
    }

    [Fact]
    public void ProductDefinition_IsIndependentFromCabinet()
    {
        var product = new ProductDefinition(
            EntityId.New(),
            "BED-001",
            "Bed",
            ProductCategory.Bedroom);

        Assert.Equal(ProductCategory.Bedroom, product.Category);
        Assert.Null(product.DesignObjectId);
    }

    [Fact]
    public void Project_CanOwnProductDefinitions_WithoutAddingThemToObjects()
    {
        var project = new ProjectModel("Test");
        var definitionId = EntityId.New();

        var definition = new ProductDefinition(
            definitionId,
            "BED-001",
            "Bed",
            ProductCategory.Bedroom);

        project.AddProductDefinition(definition);

        Assert.Single(project.ProductDefinitions);
        Assert.Same(definition, project.GetProductDefinition(definitionId));
        Assert.Empty(project.Objects);
        Assert.True(project.TryGetProductDefinition(definitionId, out var found));
        Assert.Same(definition, found);
    }

    [Fact]
    public void Project_ProductDefinitions_RejectNullAndDuplicateIds()
    {
        var project = new ProjectModel("Test");
        var definitionId = EntityId.New();

        var definition = new ProductDefinition(
            definitionId,
            "BED-001",
            "Bed",
            ProductCategory.Bedroom);

        Assert.Throws<ArgumentNullException>(
            () => project.AddProductDefinition(null!));

        project.AddProductDefinition(definition);

        var duplicate = new ProductDefinition(
            definitionId,
            "BED-002",
            "Another Bed",
            ProductCategory.Bedroom);

        Assert.Throws<InvalidOperationException>(
            () => project.AddProductDefinition(duplicate));
    }

    [Fact]
    public void Project_ProductDefinitions_CanBeRemoved()
    {
        var project = new ProjectModel("Test");
        var definitionId = EntityId.New();

        var definition = new ProductDefinition(
            definitionId,
            "BED-001",
            "Bed",
            ProductCategory.Bedroom);

        project.AddProductDefinition(definition);

        Assert.True(project.RemoveProductDefinition(definitionId));
        Assert.Empty(project.ProductDefinitions);
        Assert.False(project.TryGetProductDefinition(definitionId, out _));
        Assert.False(project.RemoveProductDefinition(definitionId));
    }

    [Fact]
    public void Pricing_IsIndependentFromMaterialAndCabinet()
    {
        var item = new PricingItem(
            EntityId.New(),
            "MAT-01",
            "Wall finish",
            PricingCategory.WallFinish,
            12,
            "m2",
            25,
            10);

        var subtotal = new PricingCalculator().CalculateSubtotal(new[] { item });

        Assert.Equal(330m, subtotal);
    }

    [Fact]
    public void ManufacturingDefinition_IsReferencedBySourceDesignObject()
    {
        var project = new ProjectModel("Test");
        var room = new Room("Room");

        project.Add(room);

        var definition = new ManufacturingDefinitionService()
            .Build(project, room, "MFG-001");

        Assert.Equal(room.Id, definition.SourceDesignObjectId);
        Assert.Equal("MFG-001", definition.Code);
    }

    [Fact]
    public void DesignDocument_CanReferenceAnyDesignObject()
    {
        var projectId = EntityId.New();
        var objectId = EntityId.New();

        var document = new DesignDocumentService().Create(
            projectId,
            "A-101",
            "Kitchen Elevation",
            DesignDocumentType.Elevation,
            objectId);

        Assert.Equal(projectId, document.ProjectId);
        Assert.Equal(objectId.Value.ToString("D"), document.SourceObjectId);
    }
}
