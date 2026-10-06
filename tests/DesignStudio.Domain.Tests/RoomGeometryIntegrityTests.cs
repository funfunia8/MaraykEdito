using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class RoomGeometryIntegrityTests
{
    [Fact]
    public void SimpleClosedRoomHasNoGeometryIntegrityErrors()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        AddWall(project, room, new Point2D(0, 0), new Point2D(5000, 0));
        AddWall(project, room, new Point2D(5000, 0), new Point2D(5000, 4000));
        AddWall(project, room, new Point2D(5000, 4000), new Point2D(0, 4000));
        AddWall(project, room, new Point2D(0, 4000), new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService().Validate(project, room);

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code == "ROOM-006");
    }

    [Fact]
    public void SelfIntersectingBowTieBoundaryIsRejected()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        AddWall(project, room, new Point2D(0, 0), new Point2D(5000, 4000));
        AddWall(project, room, new Point2D(5000, 4000), new Point2D(0, 4000));
        AddWall(project, room, new Point2D(0, 4000), new Point2D(5000, 0));
        AddWall(project, room, new Point2D(5000, 0), new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService().Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue => issue.Code == "ROOM-006");
    }

    [Fact]
    public void NonAdjacentBoundaryTouchIsRejected()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        AddWall(project, room, new Point2D(0, 0), new Point2D(5000, 0));
        AddWall(project, room, new Point2D(5000, 0), new Point2D(5000, 4000));
        AddWall(project, room, new Point2D(5000, 4000), new Point2D(2500, 0));
        AddWall(project, room, new Point2D(2500, 0), new Point2D(0, 4000));
        AddWall(project, room, new Point2D(0, 4000), new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService().Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue => issue.Code == "ROOM-006");
    }

    [Fact]
    public void AdjacentBacktrackingWallsAreRejected()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        AddWall(project, room, new Point2D(0, 0), new Point2D(5000, 0));
        AddWall(project, room, new Point2D(5000, 0), new Point2D(2500, 0));
        AddWall(project, room, new Point2D(2500, 0), new Point2D(0, 4000));
        AddWall(project, room, new Point2D(0, 4000), new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService().Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue => issue.Code == "ROOM-006");
    }

    [Fact]
    public void GeometryIntegrityErrorIdentifiesTheConflictingWalls()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        var first = AddWall(
            project,
            room,
            new Point2D(0, 0),
            new Point2D(5000, 4000));

        AddWall(
            project,
            room,
            new Point2D(5000, 4000),
            new Point2D(0, 4000));

        var third = AddWall(
            project,
            room,
            new Point2D(0, 4000),
            new Point2D(5000, 0));

        AddWall(
            project,
            room,
            new Point2D(5000, 0),
            new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService().Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue =>
                issue.Code == "ROOM-006" &&
                issue.Message.Contains(first.Id.ToString()) &&
                issue.Message.Contains(third.Id.ToString()));
    }

    private static Wall AddWall(
        ProjectModel project,
        Room room,
        Point2D start,
        Point2D end)
    {
        var wall = new Wall(
            start,
            end,
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        project.Add(wall);
        room.AddWall(wall.Id);

        return wall;
    }
}
