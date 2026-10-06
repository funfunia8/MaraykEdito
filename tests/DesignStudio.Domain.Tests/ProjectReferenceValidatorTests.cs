using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Products;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Domain.Validation;

namespace DesignStudio.Domain.Tests;
using ProjectModel = DesignStudio.Domain.Project.Project;
public sealed class ProjectReferenceValidatorTests
{
    [Fact]
    public void ValidProject_HasNoReferenceErrors()
    {
        var project = new ProjectModel("Valid References");

        var building = new Building("Villa");
        var floor = new Floor("Ground", building.Id);
        var room = new Room("Kitchen", floorId: floor.Id);

        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(3000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2800));

        room.AddWall(wall.Id);

        var product = new ProductDefinition(
            EntityId.New(),
            "CAB-001",
            "Base Cabinet",
            ProductCategory.Kitchen);

        var material = new Material(
            "PANEL-18",
            "Panel",
            MaterialKind.Sheet,
            Length.FromMillimeters(18),
            Length.FromMillimeters(2800),
            Length.FromMillimeters(2070),
            Length.FromMillimeters(1));

        var cabinet = new Cabinet(
            "Cabinet",
            Length.FromMillimeters(600),
            Length.FromMillimeters(720),
            Length.FromMillimeters(560),
            Length.FromMillimeters(18),
            Length.FromMillimeters(6));

        cabinet.SetPlacement(
            room.Id,
            new Point2D(100, 200));

        cabinet.SetProductDefinition(product.Id);

        cabinet.SetMaterialAssignments(
            material.Id,
            material.Id,
            material.Id,
            material.Id);

        project.Add(building);
        project.Add(floor);
        project.Add(room);
        project.Add(wall);
        project.Add(cabinet);
        project.AddProductDefinition(product);
        project.AddMaterial(material);

        var result =
            new ProjectReferenceValidator()
                .Validate(project);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void MissingParent_IsReported()
    {
        var project = new ProjectModel("Missing Parent");
        var room = new Room(
            "Kitchen",
            floorId: EntityId.New());

        project.Add(room);

        var result =
            new ProjectReferenceValidator()
                .Validate(project);

        Assert.Contains(
            result.Issues,
            x => x.Code == "PROJECT-REF-001");
    }

    [Fact]
    public void WrongParentType_IsReported()
    {
        var project = new ProjectModel("Wrong Parent");

        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(3000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2800));

        var room = new Room("Kitchen");
        room.SetParent(wall.Id);

        project.Add(wall);
        project.Add(room);

        var result =
            new ProjectReferenceValidator()
                .Validate(project);

        Assert.Contains(
            result.Issues,
            x => x.Code == "PROJECT-REF-002");
    }

    [Fact]
    public void MissingRoomWall_IsReported()
    {
        var project = new ProjectModel("Missing Wall");
        var room = new Room("Kitchen");

        room.AddWall(EntityId.New());
        project.Add(room);

        var result =
            new ProjectReferenceValidator()
                .Validate(project);

        Assert.Contains(
            result.Issues,
            x => x.Code == "PROJECT-REF-004");
    }

    [Fact]
    public void ParentCycle_IsReported()
    {
        var project = new ProjectModel("Parent Cycle");

        var first = new Building("First");
        var second = new Building("Second");

        first.SetParent(second.Id);
        second.SetParent(first.Id);

        project.Add(first);
        project.Add(second);

        var result =
            new ProjectReferenceValidator()
                .Validate(project);

        Assert.Contains(
            result.Issues,
            x => x.Code == "PROJECT-REF-003");
    }

    [Fact]
    public void MissingCabinetReferences_AreReported()
    {
        var project =
            new ProjectModel("Missing Cabinet References");

        var cabinet = new Cabinet(
            "Cabinet",
            Length.FromMillimeters(600),
            Length.FromMillimeters(720),
            Length.FromMillimeters(560),
            Length.FromMillimeters(18),
            Length.FromMillimeters(6));

        cabinet.SetProductDefinition(EntityId.New());

        cabinet.SetMaterialAssignments(
            EntityId.New(),
            EntityId.New(),
            null,
            EntityId.New());

        project.Add(cabinet);

        var result =
            new ProjectReferenceValidator()
                .Validate(project);

        Assert.Contains(
            result.Issues,
            x => x.Code == "PROJECT-REF-006");

        Assert.Equal(
            3,
            result.Issues.Count(
                x => x.Code == "PROJECT-REF-007"));
    }

    [Fact]
    public void ProductDefinitionMissingDesignObject_IsReported()
    {
        var project =
           new ProjectModel( "Missing Design Object");

        var definition =
            new ProductDefinition(
                EntityId.New(),
                "BED-001",
                "Bed",
                ProductCategory.Bedroom,
                DesignObjectId: EntityId.New());

        project.AddProductDefinition(definition);

        var result =
            new ProjectReferenceValidator()
                .Validate(project);

        Assert.Contains(
            result.Issues,
            x => x.Code == "PROJECT-REF-008");
    }

    [Fact]
    public void OrphanDoor_IsReportedWithoutThrowing()
    {
        var project = new ProjectModel("Orphan Door");

        var door = new Door(
            EntityId.New(),
            Length.FromMillimeters(100),
            Length.FromMillimeters(900),
            Length.FromMillimeters(2100));

        project.Add(door);

        var result =
            new ProjectReferenceValidator()
                .Validate(project);

        Assert.Contains(
            result.Issues,
            x => x.Code == "PROJECT-REF-001");
    }
}