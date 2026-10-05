using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

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