using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class PropagationTests
{
    [Fact]
    public void RoomRegenerationReflectsWallResizeInBoth2DAnd3D()
    {
        var project = new ProjectModel("Propagation");
        var rooms = new IntelligentRoomService();
        var room = rooms.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        rooms.AddDoor(
            project,
            room,
            0,
            1000,
            900);

        var regeneration = new RoomRegenerationService();
        var before = regeneration.Regenerate(project, room);

        Assert.Equal(
            5000,
            Distance(
                before.Plan2D.Walls[0].Start,
                before.Plan2D.Walls[0].End),
            3);

        Assert.Equal(
            5000,
            Distance3(before.Scene3D.Walls[0]),
            3);

        rooms.ResizeWallEnd(
            project,
            room,
            0,
            new Point2D(6200, 0));

        var after = regeneration.Regenerate(project, room);

        Assert.Equal(
            6200,
            Distance(
                after.Plan2D.Walls[0].Start,
                after.Plan2D.Walls[0].End),
            3);

        Assert.Equal(
            6200,
            Distance3(after.Scene3D.Walls[0]),
            3);

        Assert.Single(after.Plan2D.Openings);
    }

    [Fact]
    public void OpeningProjectionFollowsHostWallDirection()
    {
        var project = new ProjectModel("Projection");
        var rooms = new IntelligentRoomService();
        var room = rooms.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        rooms.AddDoor(
            project,
            room,
            1,
            500,
            900);

        var state =
            new RoomRegenerationService()
                .Regenerate(project, room);

        var opening =
            Assert.Single(
                state.Plan2D.Openings);

        Assert.Equal(
            5000,
            opening.Start.X,
            3);

        Assert.Equal(
            500,
            opening.Start.Y,
            3);

        Assert.Equal(
            5000,
            opening.End.X,
            3);

        Assert.Equal(
            1400,
            opening.End.Y,
            3);
    }

    [Fact]
    public void PartialBoundarySpanPropagatesToBoth2DAnd3D()
    {
        var project = new ProjectModel("Partial Projection");
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
                    Length.FromMillimeters(3500))));

        AddWall(
            project,
            room,
            new Point2D(3500, 0),
            new Point2D(3500, 2500));

        AddWall(
            project,
            room,
            new Point2D(3500, 2500),
            new Point2D(1000, 2500));

        AddWall(
            project,
            room,
            new Point2D(1000, 2500),
            new Point2D(1000, 0));

        project.Add(room);

        var state =
            new RoomRegenerationService()
                .Regenerate(project, room);

        Assert.Equal(
            1000,
            state.Plan2D.Walls[0].Start.X,
            3);

        Assert.Equal(
            3500,
            state.Plan2D.Walls[0].End.X,
            3);

        Assert.Equal(
            2500,
            state.Plan2D.Walls[0].LengthMm,
            3);

        Assert.Equal(
            1000,
            state.Scene3D.Walls[0].StartX,
            3);

        Assert.Equal(
            3500,
            state.Scene3D.Walls[0].EndX,
            3);
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

    private static double Distance(
        Point2D a,
        Point2D b)
        => a.DistanceTo(b);

    private static double Distance3(
        DesignStudio.Geometry.Derived.Wall3D wall)
        => Math.Sqrt(
            Math.Pow(
                wall.EndX - wall.StartX,
                2) +
            Math.Pow(
                wall.EndY - wall.StartY,
                2));
}
