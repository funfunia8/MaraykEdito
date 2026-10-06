using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class SharedBoundaryWallTests
{
    [Fact]
    public void SharedWallCanBeTraversedInOppositeDirectionsByAdjacentRooms()
    {
        var project = new ProjectModel("Shared Boundary");

        var sharedWall = AddWall(
            project,
            new Point2D(5000, 0),
            new Point2D(5000, 4000));

        var roomA = new Room("Room A");
        AddBoundaryWall(
            project,
            roomA,
            new Point2D(0, 0),
            new Point2D(5000, 0));

        roomA.AddBoundaryEdge(
            new BoundaryEdge(sharedWall.Id));

        AddBoundaryWall(
            project,
            roomA,
            new Point2D(5000, 4000),
            new Point2D(0, 4000));

        AddBoundaryWall(
            project,
            roomA,
            new Point2D(0, 4000),
            new Point2D(0, 0));

        var roomB = new Room("Room B");

        AddBoundaryWall(
            project,
            roomB,
            new Point2D(5000, 0),
            new Point2D(9000, 0));

        AddBoundaryWall(
            project,
            roomB,
            new Point2D(9000, 0),
            new Point2D(9000, 4000));

        AddBoundaryWall(
            project,
            roomB,
            new Point2D(9000, 4000),
            new Point2D(5000, 4000));

        roomB.AddBoundaryEdge(
            new BoundaryEdge(
                sharedWall.Id,
                IsReversed: true));

        project.Add(roomA);
        project.Add(roomB);

        Assert.False(
            roomA.Boundary.OuterLoop.Edges[1].IsReversed);

        Assert.True(
            roomB.Boundary.OuterLoop.Edges[3].IsReversed);

        var validator = new RoomValidationService();

        var validationA = validator.Validate(project, roomA);
        var validationB = validator.Validate(project, roomB);

        Assert.DoesNotContain(
            validationA.Issues,
            issue => issue.Code is
                "ROOM-004" or
                "ROOM-005" or
                "ROOM-006" or
                "ROOM-007" or
                "ROOM-008");

        Assert.DoesNotContain(
            validationB.Issues,
            issue => issue.Code is
                "ROOM-004" or
                "ROOM-005" or
                "ROOM-006" or
                "ROOM-007" or
                "ROOM-008");
    }

    private static Wall AddBoundaryWall(
        ProjectModel project,
        Room room,
        Point2D start,
        Point2D end)
    {
        var wall = AddWall(project, start, end);
        room.AddWall(wall.Id);
        return wall;
    }

    private static Wall AddWall(
        ProjectModel project,
        Point2D start,
        Point2D end)
    {
        var wall = new Wall(
            start,
            end,
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        project.Add(wall);
        return wall;
    }
}
