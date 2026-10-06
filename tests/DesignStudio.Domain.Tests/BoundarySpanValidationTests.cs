using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class BoundarySpanValidationTests
{
    [Fact]
    public void ConcaveBoundaryCanUseTwoDistinctSpansOfTheSamePhysicalWall()
    {
        var project = new ProjectModel("Boundary Spans");
        var room = new Room("Concave Room");

        AddWall(
            project,
            room,
            new Point2D(0, 0),
            new Point2D(5000, 0));

        var rightWall = new Wall(
            new Point2D(5000, 0),
            new Point2D(5000, 4000),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        project.Add(rightWall);

        room.AddBoundaryEdge(
            new BoundaryEdge(
                rightWall.Id,
                new WallSpan(
                    Length.FromMillimeters(0),
                    Length.FromMillimeters(1000))));

        AddWall(
            project,
            room,
            new Point2D(5000, 1000),
            new Point2D(3000, 1000));

        AddWall(
            project,
            room,
            new Point2D(3000, 1000),
            new Point2D(3000, 3000));

        AddWall(
            project,
            room,
            new Point2D(3000, 3000),
            new Point2D(5000, 3000));

        room.AddBoundaryEdge(
            new BoundaryEdge(
                rightWall.Id,
                new WallSpan(
                    Length.FromMillimeters(3000),
                    Length.FromMillimeters(4000))));

        AddWall(
            project,
            room,
            new Point2D(5000, 4000),
            new Point2D(0, 4000));

        AddWall(
            project,
            room,
            new Point2D(0, 4000),
            new Point2D(0, 0));

        project.Add(room);

        var validation =
            new RoomValidationService()
                .Validate(project, room);

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code == "ROOM-003");

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code is
                "ROOM-004" or
                "ROOM-005" or
                "ROOM-006" or
                "ROOM-007" or
                "ROOM-008" or
                "ROOM-009");
    }

    [Fact]
    public void OpeningOnUnusedPartOfWallIsNotAnOpeningOfThisRoom()
    {
        var project = new ProjectModel("Boundary Opening");
        var room = new Room("Concave Room");

        AddWall(
            project,
            room,
            new Point2D(0, 0),
            new Point2D(5000, 0));

        var rightWall = new Wall(
            new Point2D(5000, 0),
            new Point2D(5000, 4000),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        project.Add(rightWall);

        room.AddBoundaryEdge(
            new BoundaryEdge(
                rightWall.Id,
                new WallSpan(
                    Length.FromMillimeters(0),
                    Length.FromMillimeters(1000))));

        AddWall(
            project,
            room,
            new Point2D(5000, 1000),
            new Point2D(3000, 1000));

        AddWall(
            project,
            room,
            new Point2D(3000, 1000),
            new Point2D(3000, 3000));

        AddWall(
            project,
            room,
            new Point2D(3000, 3000),
            new Point2D(5000, 3000));

        room.AddBoundaryEdge(
            new BoundaryEdge(
                rightWall.Id,
                new WallSpan(
                    Length.FromMillimeters(3000),
                    Length.FromMillimeters(4000))));

        AddWall(
            project,
            room,
            new Point2D(5000, 4000),
            new Point2D(0, 4000));

        AddWall(
            project,
            room,
            new Point2D(0, 4000),
            new Point2D(0, 0));

        var door = new Door(
            rightWall.Id,
            Length.FromMillimeters(1500),
            Length.FromMillimeters(900),
            Length.FromMillimeters(2100));

        project.Add(door);
        project.Add(room);

        var validation =
            new RoomValidationService()
                .Validate(project, room);

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code is
                "OPENING-001" or
                "OPENING-002" or
                "OPENING-005");
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
