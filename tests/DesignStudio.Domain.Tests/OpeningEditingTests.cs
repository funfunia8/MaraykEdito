using DesignStudio.Application.Commands;
using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class OpeningEditingTests
{
    [Fact]
    public void MovingDoorPreservesIdentityAndChangesOffset()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);
        var door = service.AddDoor(project, room, 0, 500, 900);
        var oldId = door.Id;

        var command = new MoveOpeningCommand(project, door.Id, 1200);
        command.Execute();

        Assert.Equal(oldId, door.Id);
        Assert.Equal(1200, door.OffsetFromWallStart.Millimeters, 3);
    }

    [Fact]
    public void UndoRestoresOpeningOffset()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);
        var window = service.AddWindow(project, room, 1, 800, 1200);

        var history = new CommandHistory();
        history.Execute(new MoveOpeningCommand(project, window.Id, 1500));

        Assert.Equal(1500, window.OffsetFromWallStart.Millimeters, 3);

        history.Undo();

        Assert.Equal(800, window.OffsetFromWallStart.Millimeters, 3);

        history.Redo();

        Assert.Equal(1500, window.OffsetFromWallStart.Millimeters, 3);
    }

    [Fact]
    public void OpeningMoveCannotExceedHostWall()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);
        var door = service.AddDoor(project, room, 0, 500, 900);

        var history = new CommandHistory();
        history.Execute(new MoveOpeningCommand(project, door.Id, 4500));

        var validation = service.Validate(project, room);

        Assert.False(validation.IsValid);
    }

    [Fact]
    public void ProjectionFollowsMovedOpening()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);
        var door = service.AddDoor(project, room, 0, 500, 900);

        var command = new MoveOpeningCommand(project, door.Id, 1200);
        command.Execute();

        var plan = new DesignStudio.Geometry.Derived.RoomProjectionService()
            .Build(project, room);

        var opening = Assert.Single(plan.Openings);

        Assert.Equal(1200, opening.Start.X, 3);
        Assert.Equal(2100, opening.End.X, 3);
    }
}

public sealed class OpeningDimensionConstraintTests
{
    [Fact]
    public void ResizingDoorChangesWidthAndPreservesIdentity()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);
        var door = service.AddDoor(project, room, 0, 500, 900);

        var history = new CommandHistory();
        var originalId = door.Id;

        history.Execute(
            new ResizeOpeningCommand(project, door.Id, 1000, 2100));

        Assert.Equal(originalId, door.Id);
        Assert.Equal(1000, door.Width.Millimeters, 3);
        Assert.Equal(2100, door.Height.Millimeters, 3);
    }

    [Fact]
    public void UndoRestoresWindowHeightAndSill()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);
        var window = service.AddWindow(project, room, 1, 800, 1200);

        var history = new CommandHistory();

        history.Execute(
            new ResizeOpeningCommand(project, window.Id, 1400, 1400, 750));

        Assert.Equal(1400, window.Width.Millimeters, 3);
        Assert.Equal(1400, window.Height.Millimeters, 3);
        Assert.Equal(750, window.SillHeight.Millimeters, 3);

        history.Undo();

        Assert.Equal(1200, window.Width.Millimeters, 3);
        Assert.Equal(1200, window.Height.Millimeters, 3);
        Assert.Equal(900, window.SillHeight.Millimeters, 3);
    }

    [Fact]
    public void OverlappingOpeningsAreRejectedByValidation()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);

        service.AddDoor(project, room, 0, 500, 900);

        var window = service.AddWindow(project, room, 0, 2000, 900);

        new MoveOpeningCommand(project, window.Id, 1000).Execute();

        var validation = service.Validate(project, room);

        Assert.Contains(
            validation.Issues,
            x => x.Code == "OPENING-005");
    }
}