using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Project;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public sealed class MultiRoomTests
{
    [Fact]
    public async Task Project_Can_Persist_Multiple_Rooms_And_Their_Walls()
    {
        var project = new ProjectModel("Multi Room");
        var service = new IntelligentRoomService();

        var room1 = service.CreateRectangularRoom(project, "Living Room", 5000, 4000);
        var room2 = service.CreateRectangularRoom(project, "Bedroom", 4000, 3500, originXmm: 6500);

        Assert.Equal(2, project.Objects.OfType<Room>().Count());
        Assert.NotEqual(room1.Id, room2.Id);
        Assert.Equal(4, room1.WallIds.Count);
        Assert.Equal(4, room2.WallIds.Count);
        Assert.NotEqual(project.Get<Wall>(room1.WallIds[0]).Start, project.Get<Wall>(room2.WallIds[0]).Start);

        var serializer = new JsonProjectSerializer();
        await using var stream = new MemoryStream();
        await serializer.SaveAsync(project, stream);
        stream.Position = 0;
        var loaded = await serializer.LoadAsync(stream);

        Assert.Equal(2, loaded.Objects.OfType<Room>().Count());
        Assert.Contains(loaded.Objects.OfType<Room>(), x => x.Id == room1.Id && x.Name == room1.Name);
        Assert.Contains(loaded.Objects.OfType<Room>(), x => x.Id == room2.Id && x.Name == room2.Name);
        Assert.All(loaded.Objects.OfType<Room>(), room => Assert.All(room.WallIds, wallId => Assert.NotNull(loaded.Get<Wall>(wallId))));
    }
}
