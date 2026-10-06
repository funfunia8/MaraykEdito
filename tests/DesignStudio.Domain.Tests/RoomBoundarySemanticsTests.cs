using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class RoomBoundarySemanticsTests
{
    [Fact]
    public void RoomExposesAnExplicitOuterBoundary()
    {
        var project = new ProjectModel("Boundary");
        var service = new IntelligentRoomService();

        var room = service.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        Assert.NotNull(room.Boundary);
        Assert.NotNull(room.Boundary.OuterLoop);
    }

    [Fact]
    public void WallIdsAreACompatibilityProjectionOfTheOuterBoundary()
    {
        var project = new ProjectModel("Boundary");
        var service = new IntelligentRoomService();

        var room = service.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        Assert.Equal(
            room.Boundary.OuterLoop.WallIds,
            room.WallIds);
    }

    [Fact]
    public void OuterBoundaryPreservesWallOrder()
    {
        var project = new ProjectModel("Boundary");
        var service = new IntelligentRoomService();

        var room = service.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        Assert.Equal(
            room.WallIds.ToArray(),
            room.Boundary.OuterLoop.WallIds.ToArray());
    }

    [Fact]
    public void CreatedRoomBuildsItsExplicitOuterBoundary()
    {
        var project = new ProjectModel("Boundary");
        var service = new IntelligentRoomService();

        var room = service.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        Assert.Equal(
            4,
            room.Boundary.OuterLoop.WallIds.Count);
    }
}
