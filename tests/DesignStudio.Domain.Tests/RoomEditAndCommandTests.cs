using DesignStudio.Application.Commands;
using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class RoomEditAndCommandTests
{
    [Fact]
    public void WallResizeKeepsWallIdentityStable()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);
        var wall = project.Get<Wall>(room.WallIds[0]);
        var id = wall.Id;

        service.ResizeWallEnd(project, room, 0, new Point2D(6000, 0));

        Assert.Equal(id, wall.Id);
        Assert.Equal(6000, wall.LengthMm, 3);
    }

    [Fact]
    public void MoveWallEndpointRejectsCollapsedConnectedWallAtomically()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        var first = project.Get<Wall>(room.WallIds[0]);
        var second = project.Get<Wall>(room.WallIds[1]);

        var originalFirstStart = first.Start;
        var originalFirstEnd = first.End;
        var originalSecondStart = second.Start;
        var originalSecondEnd = second.End;

        var command = new MoveWallEndpointCommand(
            project,
            room,
            first.Id,
            moveStart: false,
            newPoint: second.End);

        Assert.Throws<ArgumentException>(() => command.Execute());

        Assert.Equal(originalFirstStart.X, first.Start.X, 3);
        Assert.Equal(originalFirstStart.Y, first.Start.Y, 3);
        Assert.Equal(originalFirstEnd.X, first.End.X, 3);
        Assert.Equal(originalFirstEnd.Y, first.End.Y, 3);

        Assert.Equal(originalSecondStart.X, second.Start.X, 3);
        Assert.Equal(originalSecondStart.Y, second.Start.Y, 3);
        Assert.Equal(originalSecondEnd.X, second.End.X, 3);
        Assert.Equal(originalSecondEnd.Y, second.End.Y, 3);
    }

    [Fact]
    public void InvalidOpeningIsRejected()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);

        Assert.Throws<InvalidOperationException>((Action)(() => service.AddDoor(project, room, 0, 4500, 1000)));
    }

    [Fact]
    public void UndoRedoRestoresWallGeometry()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);
        var wall = project.Get<Wall>(room.WallIds[0]);

        var history = new CommandHistory();
        history.Execute(new ResizeWallCommand(wall, new Point2D(6000, 0)));
        Assert.Equal(6000, wall.LengthMm, 3);

        history.Undo();
        Assert.Equal(5000, wall.LengthMm, 3);

        history.Redo();
        Assert.Equal(6000, wall.LengthMm, 3);
    }

[Fact]
public void ValidOpeningsAreAddedExactlyOnce()
{
    var project = new ProjectModel("Test");
    var service = new IntelligentRoomService();
    var room = service.CreateRectangularRoom(project, "Room", 5000, 4000);

    var door = service.AddDoor(project, room, 0, 1000, 900);
    var window = service.AddWindow(project, room, 1, 800, 1200);

    Assert.Single(project.Objects.OfType<Door>());
    Assert.Single(project.Objects.OfType<Window>());
    Assert.Contains(door, project.Objects);
    Assert.Contains(window, project.Objects);
}

}
