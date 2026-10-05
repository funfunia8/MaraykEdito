using DesignStudio.Application.Shell;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class OpeningCreationTests
{
    [Fact]
    public void CreateDoor_UsesSelectedWall_AndSupportsUndoRedo()
    {
        var state = new WorkspaceState();
        var workspace = new WorkspaceController(state);
        workspace.CreateProject("Test");
        workspace.CreateDefaultRoom();

        var id = workspace.AddDoorToActiveWall(0);
        Assert.True(state.ActiveProject!.TryGet(id, out var created));
        var door = Assert.IsType<Door>(created);
        Assert.Equal(state.ActiveRoom!.WallIds[0], door.HostWallId);
        Assert.Equal(900, door.Width.Millimeters);

        workspace.Undo();
        Assert.False(state.ActiveProject.TryGet(id, out _));

        workspace.Redo();
        Assert.True(state.ActiveProject.TryGet(id, out var restored));
        Assert.Equal(id, restored!.Id);
    }

    [Fact]
    public void CreateWindow_RejectsOverlap()
    {
        var state = new WorkspaceState();
        var workspace = new WorkspaceController(state);
        workspace.CreateProject("Test");
        workspace.CreateDefaultRoom();

        workspace.AddDoorToActiveWall(0, offsetMm: 1000, widthMm: 900);
        Assert.Throws<InvalidOperationException>(() =>
            workspace.AddWindowToActiveWall(0, offsetMm: 1200, widthMm: 1200));
    }
}
