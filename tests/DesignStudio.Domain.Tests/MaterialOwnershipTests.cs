using System.Text;
using System.Text.Json;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Products;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public sealed class MaterialOwnershipTests
{
    [Fact]
    public void Material_IsOwnedByProjectMaterials()
    {
        var project = new ProjectModel("Material Ownership");

        var material = CreateMaterial();

        project.AddMaterial(material);

        Assert.Single(project.Materials);
        Assert.Same(material, project.GetMaterial(material.Id));

        Assert.DoesNotContain(
            project.Objects,
            obj => obj.Id == material.Id);
    }

    [Fact]
    public void Material_CannotBeAddedAsProjectObject()
    {
        var project = new ProjectModel("Material Boundary");
        var material = CreateMaterial();

        var exception = Assert.Throws<InvalidOperationException>(
            () => project.Add(material));

        Assert.Contains(
            "AddMaterial",
            exception.Message,
            StringComparison.Ordinal);

        Assert.Empty(project.Objects);
        Assert.Empty(project.Materials);
    }

    [Fact]
    public void Material_CanBeRetrievedByStableId()
    {
        var project = new ProjectModel("Material Lookup");

        var material = CreateMaterial();

        project.AddMaterial(material);

        Assert.True(
            project.TryGetMaterial(
                material.Id,
                out var restored));

        Assert.Same(material, restored);
    }

    [Fact]
    public void Material_DuplicateId_IsRejected()
    {
        var project = new ProjectModel("Material Duplicate");

        var id = EntityId.New();

        var first = CreateMaterial(id);
        var second = CreateMaterial(id);

        project.AddMaterial(first);

        var exception = Assert.Throws<InvalidOperationException>(
            () => project.AddMaterial(second));

        Assert.Contains(
            id.ToString(),
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Material_Removal_ReturnsExpectedResult()
    {
        var project = new ProjectModel("Material Removal");

        var material = CreateMaterial();

        project.AddMaterial(material);

        Assert.True(project.RemoveMaterial(material.Id));
        Assert.Empty(project.Materials);

        Assert.False(project.RemoveMaterial(material.Id));
    }

    [Fact]
    public void Material_Null_IsRejected()
    {
        var project = new ProjectModel("Material Null");

        Assert.Throws<ArgumentNullException>(
            () => project.AddMaterial(null!));
    }

    [Fact]
    public void Material_UsesItsExistingStableId()
    {
        var project = new ProjectModel("Material Identity");

        var id = EntityId.New();
        var material = CreateMaterial(id);

        project.AddMaterial(material);

        Assert.Equal(id, material.Id);
        Assert.Equal(id, project.GetMaterial(id).Id);
    }

    [Fact]
    public async Task LegacyV2MaterialObject_IsMigratedToProjectMaterials()
    {
        var projectId = Guid.NewGuid();
        var materialId = Guid.NewGuid();

        var json = $$"""
        {
          "schemaVersion": 2,
          "projectId": "{{projectId:D}}",
          "name": "Legacy Material Project",
          "objects": [
            {
              "id": "{{materialId:D}}",
              "type": "Material",
              "data": {
                "code": "PANEL-18",
                "name": "Panel 18mm",
                "kind": "Sheet",
                "thicknessMm": 18,
                "sheetWidthMm": 2800,
                "sheetHeightMm": 2070,
                "bandWidthMm": 0,
                "grainDirection": "AlongWidth",
                "category": "Panel",
                "manufacturer": "Legacy Manufacturer",
                "supplier": "Legacy Supplier",
                "color": "White",
                "finish": "Matte",
                "unit": "sheet"
              },
              "metadata": {
                "category": "Finish",
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

        var material =
            project.GetMaterial(
                new EntityId(materialId));

        Assert.Equal("PANEL-18", material.Code);
        Assert.Equal("Panel 18mm", material.Name);
        Assert.Equal(MaterialKind.Sheet, material.Kind);
        Assert.Equal(18, material.Thickness.Millimeters);
        Assert.Equal(MaterialCategory.Panel, material.MaterialCategory);
        Assert.Equal("Legacy Manufacturer", material.Manufacturer);
        Assert.Equal("Legacy Supplier", material.Supplier);
        Assert.Equal("White", material.Color);
        Assert.Equal("Matte", material.Finish);
        Assert.Equal("sheet", material.Unit);

        Assert.Single(project.Materials);
        Assert.Empty(project.Objects);
        Assert.Equal(ObjectCategory.Finish, material.Category);
    }

    [Fact]
    public async Task SaveAndLoad_PreservesMaterialOwnershipWithoutChangingV2StorageShape()
    {
        var project = new ProjectModel("Material Persistence");

        var material = CreateMaterial();
        project.AddMaterial(material);

        var serializer = new JsonProjectSerializer();

        await using var stream = new MemoryStream();

        await serializer.SaveAsync(project, stream);

        stream.Position = 0;

        using var document =
            await JsonDocument.ParseAsync(stream);

        var materialRecord =
            document.RootElement
                .GetProperty("objects")
                .EnumerateArray()
                .Single(x => x.GetProperty("type").GetString() == "Material");

        Assert.Equal(
            material.Id.Value.ToString("D"),
            materialRecord.GetProperty("id").GetString());

        stream.Position = 0;

        var restored =
            await serializer.LoadAsync(stream);

        Assert.Single(restored.Materials);
        Assert.Empty(restored.Objects);
        Assert.Equal(
            material.Id,
            restored.Materials.Single().Id);

        Assert.Equal(
            ObjectCategory.Finish,
            restored.Materials.Single().Category);
    }

    private static Material CreateMaterial(EntityId? id = null)
        => new(
            "PANEL-18",
            "Panel 18mm",
            MaterialKind.Sheet,
            Length.FromMillimeters(18),
            Length.FromMillimeters(2800),
            Length.FromMillimeters(2070),
            Length.FromMillimeters(0),
            MaterialGrainDirection.AlongWidth,
            id);
}