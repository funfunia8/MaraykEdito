using DesignStudio.Application.Rooms;
using DesignStudio.Application.Shell;
using DesignStudio.Domain.Project;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class DirectEndpointEditingTests
{
    [Fact]
    public void MovingCornerMovesConnectedWallEndpoints()
    {
        var state = new WorkspaceState();
        var workspace = new WorkspaceController(state);
        workspace.CreateDefaultRoom();

        var room = state.ActiveRoom!;
        var project = state.ActiveProject!;
        var first = project.Get<Wall>(room.WallIds[0]);
        var oldCorner = first.End;

        workspace.MoveActiveWallEndpoint(0, false,
            new DesignStudio.Domain.Geometry.Point2D(oldCorner.X + 500, oldCorner.Y));

        var second = project.Get<Wall>(room.WallIds[1]);
        Assert.Equal(first.End.X, second.Start.X, 3);
        Assert.Equal(first.End.Y, second.Start.Y, 3);
    }
}
