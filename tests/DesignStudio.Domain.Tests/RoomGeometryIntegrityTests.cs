using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class RoomGeometryIntegrityTests
{
    [Fact]
    public void SimpleClosedRoomHasNoBoundaryGeometryErrors()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        AddWall(project, room, new Point2D(0, 0), new Point2D(5000, 0));
        AddWall(project, room, new Point2D(5000, 0), new Point2D(5000, 4000));
        AddWall(project, room, new Point2D(5000, 4000), new Point2D(0, 4000));
        AddWall(project, room, new Point2D(0, 4000), new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService()
            .Validate(project, room);

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code is
                "ROOM-006" or
                "ROOM-007" or
                "ROOM-008" or
                "ROOM-009");
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

        var validation = new RoomValidationService()
            .Validate(project, room);

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

        var validation = new RoomValidationService()
            .Validate(project, room);

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

        var validation = new RoomValidationService()
            .Validate(project, room);

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

        var validation = new RoomValidationService()
            .Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue =>
                issue.Code == "ROOM-006" &&
                issue.Message.Contains(first.Id.ToString()) &&
                issue.Message.Contains(third.Id.ToString()));
    }

    [Fact]
    public void ClockwiseOuterBoundaryIsRejected()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        AddWall(project, room, new Point2D(0, 0), new Point2D(0, 4000));
        AddWall(project, room, new Point2D(0, 4000), new Point2D(5000, 4000));
        AddWall(project, room, new Point2D(5000, 4000), new Point2D(5000, 0));
        AddWall(project, room, new Point2D(5000, 0), new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService()
            .Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue => issue.Code == "ROOM-007");
    }

    [Fact]
    public void ConcaveCounterClockwiseRoomHasValidOuterBoundaryOrientation()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        AddWall(project, room, new Point2D(0, 0), new Point2D(5000, 0));
        AddWall(project, room, new Point2D(5000, 0), new Point2D(5000, 2000));
        AddWall(project, room, new Point2D(5000, 2000), new Point2D(3000, 2000));
        AddWall(project, room, new Point2D(3000, 2000), new Point2D(3000, 4000));
        AddWall(project, room, new Point2D(3000, 4000), new Point2D(0, 4000));
        AddWall(project, room, new Point2D(0, 4000), new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService()
            .Validate(project, room);

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code is
                "ROOM-006" or
                "ROOM-007" or
                "ROOM-008");
    }

    [Fact]
    public void PartialBoundaryEdgeUsesItsPhysicalWallSpan()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        var baseWall = new Wall(
            new Point2D(0, 0),
            new Point2D(5000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        project.Add(baseWall);

        room.AddBoundaryEdge(
            new BoundaryEdge(
                baseWall.Id,
                new WallSpan(
                    Length.FromMillimeters(1000),
                    Length.FromMillimeters(4000))));

        AddWall(
            project,
            room,
            new Point2D(4000, 0),
            new Point2D(4000, 3000));

        AddWall(
            project,
            room,
            new Point2D(4000, 3000),
            new Point2D(1000, 3000));

        AddWall(
            project,
            room,
            new Point2D(1000, 3000),
            new Point2D(1000, 0));

        project.Add(room);

        var validation = new RoomValidationService()
            .Validate(project, room);

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
    public void ReversedPartialBoundaryEdgeUsesReversedPhysicalSpan()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        // Physical wall direction is right-to-left.
        var baseWall = new Wall(
            new Point2D(5000, 0),
            new Point2D(0, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        project.Add(baseWall);

        // Physical offsets 1000..4000 correspond to points
        // (4000,0)..(1000,0). Reversing traversal produces
        // (1000,0)..(4000,0), which is the CCW bottom edge.
        room.AddBoundaryEdge(
            new BoundaryEdge(
                baseWall.Id,
                new WallSpan(
                    Length.FromMillimeters(1000),
                    Length.FromMillimeters(4000)),
                IsReversed: true));

        AddWall(
            project,
            room,
            new Point2D(4000, 0),
            new Point2D(4000, 3000));

        AddWall(
            project,
            room,
            new Point2D(4000, 3000),
            new Point2D(1000, 3000));

        AddWall(
            project,
            room,
            new Point2D(1000, 3000),
            new Point2D(1000, 0));

        project.Add(room);

        var validation = new RoomValidationService()
            .Validate(project, room);

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
    public void BoundarySpanOutsideWallLengthIsRejected()
    {
        var project = new ProjectModel("Geometry");
        var room = new Room("Room");

        var baseWall = new Wall(
            new Point2D(0, 0),
            new Point2D(5000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        project.Add(baseWall);

        room.AddBoundaryEdge(
            new BoundaryEdge(
                baseWall.Id,
                new WallSpan(
                    Length.FromMillimeters(1000),
                    Length.FromMillimeters(5500))));

        AddWall(
            project,
            room,
            new Point2D(5000, 0),
            new Point2D(5000, 4000));

        AddWall(
            project,
            room,
            new Point2D(5000, 4000),
            new Point2D(1000, 4000));

        AddWall(
            project,
            room,
            new Point2D(1000, 4000),
            new Point2D(1000, 0));

        project.Add(room);

        var validation = new RoomValidationService()
            .Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue => issue.Code == "ROOM-009");
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
