using System.Threading.Tasks;
using DesignStudio.Application.Products;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetPlacementTests
{
    [Fact]
    public async Task Cabinet_Can_Be_Placed_In_Room_And_RoundTrip_Preserves_Placement()
    {
        var project = new ProjectModel("Placement Test");
        var room = new Room("Kitchen");
        project.Add(room);

        var cabinet = new CabinetService().Create(project).Cabinet;
        cabinet.SetPlacement(room.Id, new Point2D(1250, 1800), 90);

        Assert.Equal(room.Id, cabinet.HostRoomId);
        Assert.Equal(1250, cabinet.Position.X);
        Assert.Equal(1800, cabinet.Position.Y);
        Assert.Equal(90, cabinet.RotationDegrees);

        var serializer = new JsonProjectSerializer();
        using var stream = new MemoryStream();
        await serializer.SaveAsync(project, stream);
        stream.Position = 0;
        var restored = await serializer.LoadAsync(stream);
        var restoredCabinet = restored.Objects.OfType<Cabinet>().Single();

        Assert.Equal(room.Id, restoredCabinet.HostRoomId);
        Assert.Equal(new Point2D(1250, 1800), restoredCabinet.Position);
        Assert.Equal(90, restoredCabinet.RotationDegrees);
    }
}
