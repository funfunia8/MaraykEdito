using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Project;
using DesignStudio.Storage;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class IntelligentRoomTests
{
    [Fact]
    public void RectangularRoomCreatesFourWallsWithStableRelationships()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();

        var room = service.CreateRectangularRoom(project, "Living Room", 5000, 4000);

        Assert.Equal(4, room.WallIds.Count);
        Assert.Equal(5, project.Objects.Count);
        foreach (var wallId in room.WallIds)
            Assert.IsType<Wall>(project.Get<Wall>(wallId));
    }

    [Fact]
    public void DoorAndWindowReferenceTheirHostWall()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);

        var door = service.AddDoor(project, room, 0, 1000, 900);
        var window = service.AddWindow(project, room, 1, 800, 1200);

        Assert.Equal(room.WallIds[0], door.HostWallId);
        Assert.Equal(room.WallIds[1], window.HostWallId);
    }

    [Fact]
    public async Task ProjectRoundTripsThroughVersionedJson()
    {
        var project = new ProjectModel("Round Trip");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Bedroom", 4200, 3600);
        service.AddDoor(project, room, 0, 900, 800);
        service.AddWindow(project, room, 2, 1200, 1400);

        var serializer = new JsonProjectSerializer();
        await using var stream = new MemoryStream();
        await serializer.SaveAsync(project, stream);
        stream.Position = 0;

        var restored = await serializer.LoadAsync(stream);

        Assert.Equal(project.Id, restored.Id);
        Assert.Equal(project.Objects.Count, restored.Objects.Count);
        var restoredRoom = Assert.Single(restored.Objects.OfType<Room>());
        Assert.Equal(4, restoredRoom.WallIds.Count);
        Assert.Single(restored.Objects.OfType<Door>());
        Assert.Single(restored.Objects.OfType<Window>());
    }
}
