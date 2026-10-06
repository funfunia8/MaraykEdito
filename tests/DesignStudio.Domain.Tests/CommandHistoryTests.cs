using DesignStudio.Application.Commands;
using DesignStudio.Application.Rooms;
using DesignStudio.Application.Shell;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class CommandHistoryTests
{
    [Fact]
    public void RollbackLastExecutionDoesNotEnterRedoHistory()
    {
        var state = new WorkspaceState();
        var workspace = new WorkspaceController(state);

        workspace.CreateProject("Test");
        workspace.CreateDefaultRoom();

        var doorId = workspace.AddDoorToActiveWall(
            0,
            offsetMm: 500,
            widthMm: 900);

        Assert.True(workspace.CanUndo);
        Assert.False(workspace.CanRedo);

        Assert.Throws<InvalidOperationException>(
            () => workspace.ResizeActiveOpening(
                doorId,
                widthMm: 4600,
                heightMm: 2100,
                sillHeightMm: null));

        var door = state.ActiveProject!.Get<Door>(doorId);

        Assert.Equal(
            900,
            door.Width.Millimeters,
            3);

        Assert.True(workspace.CanUndo);
        Assert.False(workspace.CanRedo);
    }

    [Fact]
    public void RollbackLastExecutionRestoresRedoHistoryThatExistedBeforeExecution()
    {
        var project = new ProjectModel("Test");
        var service = new IntelligentRoomService();
        var room = service.CreateRectangularRoom(
            project,
            "Room",
            5000,
            4000);

        var wall = project.Get<Wall>(
            room.WallIds[0]);

        var history = new CommandHistory();

        history.Execute(
            new ResizeWallCommand(
                wall,
                new Point2D(6000, 0)));

        history.Undo();

        Assert.True(history.CanRedo);
        Assert.Equal(
            5000,
            wall.LengthMm,
            3);

        history.Execute(
            new ResizeWallCommand(
                wall,
                new Point2D(5500, 0)));

        Assert.False(history.CanRedo);
        Assert.Equal(
            5500,
            wall.LengthMm,
            3);

        history.RollbackLastExecution();

        Assert.Equal(
            5000,
            wall.LengthMm,
            3);

        Assert.False(history.CanUndo);
        Assert.True(history.CanRedo);

        history.Redo();

        Assert.Equal(
            6000,
            wall.LengthMm,
            3);
    }

    [Fact]
    public void ResizeActiveWallPreservesTopologyAndUndoRedo()
    {
        var state = new WorkspaceState();
        var workspace = new WorkspaceController(state);

        workspace.CreateProject("Test");
        workspace.CreateDefaultRoom();

        var project = state.ActiveProject!;
        var room = state.ActiveRoom!;

        var first = project.Get<Wall>(room.WallIds[0]);
        var second = project.Get<Wall>(room.WallIds[1]);

        var originalFirstEnd = first.End;
        var originalSecondStart = second.Start;

        var newEnd = new Point2D(6200, 0);

        workspace.ResizeActiveWall(0, newEnd);

        Assert.Equal(newEnd.X, first.End.X, 3);
        Assert.Equal(newEnd.Y, first.End.Y, 3);

        Assert.Equal(newEnd.X, second.Start.X, 3);
        Assert.Equal(newEnd.Y, second.Start.Y, 3);

        Assert.True(workspace.CanUndo);

        workspace.Undo();

        Assert.Equal(originalFirstEnd.X, first.End.X, 3);
        Assert.Equal(originalFirstEnd.Y, first.End.Y, 3);

        Assert.Equal(originalSecondStart.X, second.Start.X, 3);
        Assert.Equal(originalSecondStart.Y, second.Start.Y, 3);

        Assert.True(workspace.CanRedo);

        workspace.Redo();

        Assert.Equal(newEnd.X, first.End.X, 3);
        Assert.Equal(newEnd.Y, first.End.Y, 3);

        Assert.Equal(newEnd.X, second.Start.X, 3);
        Assert.Equal(newEnd.Y, second.Start.Y, 3);
    }

    [Fact]
    public void ResizeActiveWallRejectsSelfIntersectionAndRestoresAllGeometry()
    {
        var state = new WorkspaceState();
        var workspace = new WorkspaceController(state);

        workspace.CreateProject("Test");
        workspace.CreateDefaultRoom();

        var project = state.ActiveProject!;
        var room = state.ActiveRoom!;

        var walls = room.WallIds
            .Select(project.Get<Wall>)
            .ToList();

        var originalGeometry = walls
            .Select(wall => (wall.Id, wall.Start, wall.End))
            .ToDictionary(
                x => x.Id,
                x => (x.Start, x.End));

        Assert.Throws<InvalidOperationException>(
            () => workspace.ResizeActiveWall(
                0,
                new Point2D(1000, 5000)));

        foreach (var wall in walls)
        {
            var original = originalGeometry[wall.Id];

            Assert.Equal(original.Start.X, wall.Start.X, 3);
            Assert.Equal(original.Start.Y, wall.Start.Y, 3);
            Assert.Equal(original.End.X, wall.End.X, 3);
            Assert.Equal(original.End.Y, wall.End.Y, 3);
        }

        Assert.False(workspace.CanUndo);
        Assert.False(workspace.CanRedo);

        var validation = new RoomValidationService()
            .Validate(project, room);

        Assert.True(validation.IsValid);
        Assert.DoesNotContain(
            validation.Issues,
            issue => issue.Code == "ROOM-006");
    }
}
