using System.Text;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Products;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public sealed class ProductDefinitionLinkTests
{
    [Fact]
    public void ProductDefinition_CanBeReusedByMultipleCabinets()
    {
        var project = new ProjectModel("Product Definition Reuse");

        var definitionId = EntityId.New();

        var definition = new ProductDefinition(
            definitionId,
            "CAB-BASE-600",
            "Base Cabinet 600",
            ProductCategory.Kitchen);

        project.AddProductDefinition(definition);

        var cabinet1 = CreateCabinet("Cabinet 1");
        var cabinet2 = CreateCabinet("Cabinet 2");

        cabinet1.SetProductDefinition(definitionId);
        cabinet2.SetProductDefinition(definitionId);

        project.Add(cabinet1);
        project.Add(cabinet2);

        Assert.Single(project.ProductDefinitions);
        Assert.Equal(2, project.Objects.Count);

        Assert.Equal(
            definitionId,
            cabinet1.ProductDefinitionId);

        Assert.Equal(
            definitionId,
            cabinet2.ProductDefinitionId);

        Assert.Same(
            definition,
            project.GetProductDefinition(definitionId));
    }

    [Fact]
    public void Cabinet_CanExistWithoutProductDefinition()
    {
        var cabinet = CreateCabinet("Standalone Cabinet");

        Assert.Null(cabinet.ProductDefinitionId);
    }

    [Fact]
    public void ProductDefinition_DoesNotBecomeProjectObject()
    {
        var project = new ProjectModel("Definition Boundary");

        var definition = new ProductDefinition(
            EntityId.New(),
            "CAB-001",
            "Kitchen Cabinet",
            ProductCategory.Kitchen);

        project.AddProductDefinition(definition);

        var cabinet = CreateCabinet("Cabinet");
        project.Add(cabinet);

        Assert.Single(project.ProductDefinitions);
        Assert.Single(project.Objects);

        Assert.DoesNotContain(
            project.Objects,
            obj => obj.Id == definition.Id);
    }

    [Fact]
    public async Task SaveAndLoad_PreservesProductDefinitionAndCabinetLink()
    {
        var project = new ProjectModel("Persistence Test");

        var definitionId = EntityId.New();

        project.AddProductDefinition(
            new ProductDefinition(
                definitionId,
                "CAB-BASE-600",
                "Base Cabinet 600",
                ProductCategory.Kitchen,
                Manufacturer: "Marayk",
                Model: "BASE-600"));

        var cabinet = CreateCabinet("Kitchen Base");
        cabinet.SetProductDefinition(definitionId);
        project.Add(cabinet);

        var serializer = new JsonProjectSerializer();

        await using var stream = new MemoryStream();

        await serializer.SaveAsync(project, stream);

        stream.Position = 0;

        var restored = await serializer.LoadAsync(stream);

        var restoredDefinition =
            restored.GetProductDefinition(definitionId);

        var restoredCabinet =
            restored.Objects
                .OfType<Cabinet>()
                .Single();

        Assert.Equal(
            definitionId,
            restoredCabinet.ProductDefinitionId);

        Assert.Equal(
            "CAB-BASE-600",
            restoredDefinition.Code);

        Assert.Equal(
            "Base Cabinet 600",
            restoredDefinition.Name);

        Assert.Equal(
            ProductCategory.Kitchen,
            restoredDefinition.Category);

        Assert.Equal(
            "Marayk",
            restoredDefinition.Manufacturer);

        Assert.Equal(
            "BASE-600",
            restoredDefinition.Model);
    }

    [Fact]
    public async Task Load_OldV2CabinetWithoutProductDefinitionId_SetsNull()
    {
        var projectId = Guid.NewGuid();
        var cabinetId = Guid.NewGuid();

        var json = $$"""
        {
          "schemaVersion": 2,
          "projectId": "{{projectId:D}}",
          "name": "Legacy Project",
          "objects": [
            {
              "id": "{{cabinetId:D}}",
              "type": "Cabinet",
              "data": {
                "name": "Legacy Cabinet",
                "widthMm": 600,
                "heightMm": 720,
                "depthMm": 560,
                "panelThicknessMm": 18,
                "backThicknessMm": 6,
                "constructionMethod": "CapturedBackGroove",
                "shelfCount": 2,
                "doorCount": 1
              },
              "metadata": {
                "category": "Furniture",
                "positionX": 0,
                "positionY": 0,
                "rotationDegrees": 0,
                "isVisible": true,
                "isLocked": false
              }
            }
          ]
        }
        """;

        var serializer = new JsonProjectSerializer();

        await using var stream =
            new MemoryStream(
                Encoding.UTF8.GetBytes(json));

        var project =
            await serializer.LoadAsync(stream);

        var cabinet =
            project.Objects
                .OfType<Cabinet>()
                .Single();

        Assert.Null(cabinet.ProductDefinitionId);
        Assert.Empty(project.ProductDefinitions);
    }

    [Fact]
    public async Task Load_OldV2WithoutProductDefinitions_CreatesEmptyDefinitionCollection()
    {
        var projectId = Guid.NewGuid();
        var cabinetId = Guid.NewGuid();

        var json = $$"""
        {
          "schemaVersion": 2,
          "projectId": "{{projectId:D}}",
          "name": "Legacy Project",
          "objects": [
            {
              "id": "{{cabinetId:D}}",
              "type": "Cabinet",
              "data": {
                "name": "Legacy Cabinet",
                "widthMm": 600,
                "heightMm": 720,
                "depthMm": 560,
                "panelThicknessMm": 18,
                "backThicknessMm": 6,
                "constructionMethod": "CapturedBackGroove",
                "shelfCount": 2,
                "doorCount": 1
              }
            }
          ]
        }
        """;

        var serializer = new JsonProjectSerializer();

        await using var stream =
            new MemoryStream(
                Encoding.UTF8.GetBytes(json));

        var project =
            await serializer.LoadAsync(stream);

        Assert.Empty(project.ProductDefinitions);
        Assert.Single(project.Objects);
    }

    private static Cabinet CreateCabinet(string name)
        => new(
            name,
            Length.FromMillimeters(600),
            Length.FromMillimeters(720),
            Length.FromMillimeters(560),
            Length.FromMillimeters(18),
            Length.FromMillimeters(6));
}