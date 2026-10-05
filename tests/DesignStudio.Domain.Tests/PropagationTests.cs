using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class PropagationTests
{
    [Fact]
    public void RoomRegenerationReflectsWallResizeInBoth2DAnd3D()
    {
        var project = new ProjectModel("Propagation");
        var rooms = new IntelligentRoomService();
        var room = rooms.CreateRectangularRoom(project, "Room", 5000, 4000);
        rooms.AddDoor(project, room, 0, 1000, 900);

        var regeneration = new RoomRegenerationService();
        var before = regeneration.Regenerate(project, room);

        Assert.Equal(
            5000,
            Distance(before.Plan2D.Walls[0].Start, before.Plan2D.Walls[0].End),
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
            Distance(after.Plan2D.Walls[0].Start, after.Plan2D.Walls[0].End),
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
        var room = rooms.CreateRectangularRoom(project, "Room", 5000, 4000);

        rooms.AddDoor(project, room, 1, 500, 900);

        var state = new RoomRegenerationService()
            .Regenerate(project, room);

        var opening = Assert.Single(state.Plan2D.Openings);

        Assert.Equal(5000, opening.Start.X, 3);
        Assert.Equal(500, opening.Start.Y, 3);
        Assert.Equal(5000, opening.End.X, 3);
        Assert.Equal(1400, opening.End.Y, 3);
    }

    private static double Distance(
        DesignStudio.Domain.Geometry.Point2D a,
        DesignStudio.Domain.Geometry.Point2D b)
        => a.DistanceTo(b);

    private static double Distance3(
        DesignStudio.Geometry.Derived.Wall3D w)
        => Math.Sqrt(
            Math.Pow(w.EndX - w.StartX, 2)
            + Math.Pow(w.EndY - w.StartY, 2));
}