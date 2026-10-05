using DesignStudio.Application.Products;
using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetClearanceTests
{
    [Fact]
    public void Cabinet_Outside_Room_Is_Rejected()
    {
        var project = new ProjectModel("Clearance Test");
        var room = new IntelligentRoomService().CreateRectangularRoom(project, "Kitchen", 5000, 4000);
        var first = new CabinetService().Create(project, widthMm: 600, depthMm: 560).Cabinet;
        first.SetPlacement(room.Id, new Point2D(-500, 2000));

        var result = new CabinetPlacementValidationService().Validate(project, room);

        Assert.Contains(result.Issues, x => x.Code == "CAB-PLACE-002");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Overlapping_Cabinets_Are_Rejected()
    {
        var project = new ProjectModel("Collision Test");
        var room = new IntelligentRoomService().CreateRectangularRoom(project, "Kitchen", 5000, 4000);
        var service = new CabinetService();
        var first = service.Create(project, widthMm: 600, depthMm: 560).Cabinet;
        var second = service.Create(project, widthMm: 600, depthMm: 560).Cabinet;
        first.SetPlacement(room.Id, new Point2D(1000, 1000));
        second.SetPlacement(room.Id, new Point2D(1200, 1000));

        var result = new CabinetPlacementValidationService().Validate(project, room);

        Assert.Contains(result.Issues, x => x.Code == "CAB-PLACE-005");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Cabinet_Near_Door_Clearance_Is_Rejected()
    {
        var project = new ProjectModel("Opening Clearance Test");
        var roomService = new IntelligentRoomService();
        var room = roomService.CreateRectangularRoom(project, "Kitchen", 5000, 4000);
        roomService.AddDoor(project, room, 0, 2200, 900);

        var cabinet = new CabinetService().Create(project, widthMm: 600, depthMm: 560).Cabinet;
        cabinet.SetPlacement(room.Id, new Point2D(2500, 450));

        var result = new CabinetPlacementValidationService().Validate(project, room);

        Assert.Contains(result.Issues, x => x.Code == "CAB-PLACE-004");
        Assert.False(result.IsValid);
    }
}
