using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class RoomTopologyValidationTests
{
    [Fact]
    public void RectangularRoomHasValidClosedTopology()
    {
        var project = new DesignStudio.Domain.Project.Project("Topology");
        var roomService = new IntelligentRoomService();

        var room = roomService.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        var validation = roomService.Validate(project, room);

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code == "ROOM-004");

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code == "ROOM-005");
    }

    [Fact]
    public void DisconnectedWallChainIsRejected()
    {
        var project = new DesignStudio.Domain.Project.Project("Topology");
        var room = new Room("Room");

        AddWall(
            project,
            room,
            new Point2D(0, 0),
            new Point2D(5000, 0));

        AddWall(
            project,
            room,
            new Point2D(5000, 0),
            new Point2D(5000, 4000));

        AddWall(
            project,
            room,
            new Point2D(5000, 4000),
            new Point2D(0, 4000));

        AddWall(
            project,
            room,
            new Point2D(100, 4000),
            new Point2D(0, 0));

        project.Add(room);

        var validation = new RoomValidationService()
            .Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue => issue.Code == "ROOM-004");
    }

    [Fact]
    public void OpenRoomBoundaryIsRejected()
    {
        var project = new DesignStudio.Domain.Project.Project("Topology");
        var room = new Room("Room");

        AddWall(
            project,
            room,
            new Point2D(0, 0),
            new Point2D(5000, 0));

        AddWall(
            project,
            room,
            new Point2D(5000, 0),
            new Point2D(5000, 4000));

        AddWall(
            project,
            room,
            new Point2D(5000, 4000),
            new Point2D(0, 4000));

        AddWall(
            project,
            room,
            new Point2D(0, 4000),
            new Point2D(0, 500));

        project.Add(room);

        var validation = new RoomValidationService()
            .Validate(project, room);

        Assert.Contains(
            validation.Issues,
            issue => issue.Code == "ROOM-005");
    }

    [Fact]
    public void ResizingWallEndPreservesRoomTopology()
    {
        var project = new DesignStudio.Domain.Project.Project("Topology");
        var roomService = new IntelligentRoomService();

        var room = roomService.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        roomService.ResizeWallEnd(
            project,
            room,
            0,
            new Point2D(6200, 0));

        var first = project.Get<Wall>(room.WallIds[0]);
        var second = project.Get<Wall>(room.WallIds[1]);

        Assert.Equal(
            first.End.X,
            second.Start.X,
            3);

        Assert.Equal(
            first.End.Y,
            second.Start.Y,
            3);

        var validation = roomService.Validate(project, room);

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code == "ROOM-004");

        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code == "ROOM-005");
    }

    private static void AddWall(
        DesignStudio.Domain.Project.Project project,
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
    }
}
