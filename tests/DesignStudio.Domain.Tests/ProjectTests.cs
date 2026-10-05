using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public class ProjectTests
{
    [Fact]
    public void Project_Can_Add_Wall()
    {
        var project = new ProjectModel("Test");
        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(3000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2800));

        project.Add(wall);

        Assert.Single(project.Objects);
        Assert.Equal("Wall", project.Objects.Single().Type);
    }

    [Fact]
    public void Wall_Length_Can_Be_Changed()
    {
        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(3000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2800));

        wall.SetGeometry(new Point2D(0, 0), new Point2D(4500, 0));

        Assert.Equal(4500, wall.LengthMm);
    }
}
