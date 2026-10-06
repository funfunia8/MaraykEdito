using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;

namespace DesignStudio.Domain.Tests;

public sealed class BoundaryEdgeSemanticsTests
{
    [Fact]
    public void BoundaryEdgeDefaultsToForwardTraversal()
    {
        var wallId = EntityId.New();

        var edge = new BoundaryEdge(wallId);

        Assert.Equal(wallId, edge.WallId);
        Assert.False(edge.IsReversed);
    }

    [Fact]
    public void BoundaryEdgeCanRepresentReverseTraversal()
    {
        var wallId = EntityId.New();

        var edge = new BoundaryEdge(
            wallId,
            IsReversed: true);

        Assert.Equal(wallId, edge.WallId);
        Assert.True(edge.IsReversed);
    }

    [Fact]
    public void BoundaryLoopPreservesDirectedEdgeOrder()
    {
        var firstWall = EntityId.New();
        var secondWall = EntityId.New();

        var room = new Room("Room");

        room.AddBoundaryEdge(
            new BoundaryEdge(firstWall));

        room.AddBoundaryEdge(
            new BoundaryEdge(
                secondWall,
                IsReversed: true));

        Assert.Equal(
            firstWall,
            room.Boundary.OuterLoop.Edges[0].WallId);

        Assert.False(
            room.Boundary.OuterLoop.Edges[0].IsReversed);

        Assert.Equal(
            secondWall,
            room.Boundary.OuterLoop.Edges[1].WallId);

        Assert.True(
            room.Boundary.OuterLoop.Edges[1].IsReversed);
    }

    [Fact]
    public void WallIdsRemainACompatibilityProjectionOfDirectedEdges()
    {
        var firstWall = EntityId.New();
        var secondWall = EntityId.New();

        var room = new Room("Room");

        room.AddBoundaryEdge(
            new BoundaryEdge(firstWall));

        room.AddBoundaryEdge(
            new BoundaryEdge(
                secondWall,
                IsReversed: true));

        Assert.Equal(
            new[] { firstWall, secondWall },
            room.Boundary.OuterLoop.WallIds);
    }

    [Fact]
    public void CompatibilityAddWallCreatesForwardBoundaryEdge()
    {
        var wallId = EntityId.New();

        var room = new Room("Room");

        room.AddWall(wallId);

        Assert.Single(room.Boundary.OuterLoop.Edges);

        Assert.Equal(
            wallId,
            room.Boundary.OuterLoop.Edges[0].WallId);

        Assert.False(
            room.Boundary.OuterLoop.Edges[0].IsReversed);
    }
}
