using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class BoundaryEdgeSemanticsTests
{
    [Fact]
    public void BoundaryEdgeDefaultsToWholeWallForwardTraversal()
    {
        var wallId = EntityId.New();

        var edge = new BoundaryEdge(wallId);

        Assert.Equal(wallId, edge.WallId);
        Assert.True(edge.IsWholeWall);
        Assert.Null(edge.Span);
        Assert.False(edge.IsReversed);
    }

    [Fact]
    public void BoundaryEdgeCanRepresentWholeWallReverseTraversal()
    {
        var wallId = EntityId.New();

        var edge = new BoundaryEdge(
            wallId,
            IsReversed: true);

        Assert.Equal(wallId, edge.WallId);
        Assert.True(edge.IsWholeWall);
        Assert.Null(edge.Span);
        Assert.True(edge.IsReversed);
    }

    [Fact]
    public void BoundaryEdgeCanRepresentPartialWallSpan()
    {
        var wallId = EntityId.New();

        var span = new WallSpan(
            Length.FromMillimeters(1000),
            Length.FromMillimeters(3500));

        var edge = new BoundaryEdge(
            wallId,
            span);

        Assert.Equal(wallId, edge.WallId);
        Assert.False(edge.IsWholeWall);
        Assert.Equal(span, edge.Span);
        Assert.False(edge.IsReversed);
    }

    [Fact]
    public void BoundaryEdgeCanRepresentReversedPartialWallSpan()
    {
        var wallId = EntityId.New();

        var span = new WallSpan(
            Length.FromMillimeters(1000),
            Length.FromMillimeters(3500));

        var edge = new BoundaryEdge(
            wallId,
            span,
            IsReversed: true);

        Assert.Equal(wallId, edge.WallId);
        Assert.Equal(span, edge.Span);
        Assert.True(edge.IsReversed);
    }

    [Fact]
    public void WholeWallEdgeResolvesAgainstPhysicalWallLength()
    {
        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(5000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        var edge = new BoundaryEdge(wall.Id);

        var segment = edge.ResolveSegment(wall);

        Assert.Equal(
            new Point2D(0, 0),
            segment.Start);

        Assert.Equal(
            new Point2D(5000, 0),
            segment.End);

        Assert.Equal(
            wall.Thickness,
            segment.Thickness);
    }

    [Fact]
    public void WholeWallReverseTraversalResolvesReversedSegment()
    {
        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(5000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        var edge = new BoundaryEdge(
            wall.Id,
            IsReversed: true);

        var segment = edge.ResolveSegment(wall);

        Assert.Equal(
            new Point2D(5000, 0),
            segment.Start);

        Assert.Equal(
            new Point2D(0, 0),
            segment.End);
    }

    [Fact]
    public void PartialBoundaryEdgeResolvesOnlyItsPhysicalSpan()
    {
        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(5000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        var edge = new BoundaryEdge(
            wall.Id,
            new WallSpan(
                Length.FromMillimeters(1000),
                Length.FromMillimeters(3500)));

        var segment = edge.ResolveSegment(wall);

        Assert.Equal(
            new Point2D(1000, 0),
            segment.Start);

        Assert.Equal(
            new Point2D(3500, 0),
            segment.End);

        Assert.Equal(
            2500,
            segment.LengthMm,
            3);
    }

    [Fact]
    public void ReversedPartialBoundaryEdgeReversesTheResolvedSegment()
    {
        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(5000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        var edge = new BoundaryEdge(
            wall.Id,
            new WallSpan(
                Length.FromMillimeters(1000),
                Length.FromMillimeters(3500)),
            IsReversed: true);

        var segment = edge.ResolveSegment(wall);

        Assert.Equal(
            new Point2D(3500, 0),
            segment.Start);

        Assert.Equal(
            new Point2D(1000, 0),
            segment.End);
    }

    [Fact]
    public void ResolvingOutOfRangePartialSpanIsRejected()
    {
        var wall = new Wall(
            new Point2D(0, 0),
            new Point2D(5000, 0),
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        var edge = new BoundaryEdge(
            wall.Id,
            new WallSpan(
                Length.FromMillimeters(1000),
                Length.FromMillimeters(5500)));

        Assert.Throws<InvalidOperationException>(
            () => edge.ResolveSegment(wall));
    }

    [Fact]
    public void BoundaryLoopAllowsDistinctNonOverlappingSpansOfTheSameWall()
    {
        var wallId = EntityId.New();

        var firstSpan = new WallSpan(
            Length.FromMillimeters(0),
            Length.FromMillimeters(2000));

        var secondSpan = new WallSpan(
            Length.FromMillimeters(2000),
            Length.FromMillimeters(5000));

        var room = new Room("Room");

        room.AddBoundaryEdge(
            new BoundaryEdge(
                wallId,
                firstSpan));

        room.AddBoundaryEdge(
            new BoundaryEdge(
                wallId,
                secondSpan));

        Assert.Equal(
            2,
            room.Boundary.OuterLoop.Edges.Count);

        Assert.Equal(
            new[] { wallId, wallId },
            room.WallIds);
    }

    [Fact]
    public void BoundaryLoopRejectsOverlappingSpansOfTheSameWall()
    {
        var wallId = EntityId.New();

        var firstSpan = new WallSpan(
            Length.FromMillimeters(0),
            Length.FromMillimeters(2000));

        var secondSpan = new WallSpan(
            Length.FromMillimeters(1500),
            Length.FromMillimeters(3000));

        var room = new Room("Room");

        room.AddBoundaryEdge(
            new BoundaryEdge(
                wallId,
                firstSpan));

        Assert.Throws<InvalidOperationException>(
            () => room.AddBoundaryEdge(
                new BoundaryEdge(
                    wallId,
                    secondSpan)));

        Assert.Single(
            room.Boundary.OuterLoop.Edges);
    }

    [Fact]
    public void BoundaryLoopRejectsWholeWallCombinedWithPartialSpan()
    {
        var wallId = EntityId.New();

        var room = new Room("Room");

        room.AddBoundaryEdge(
            new BoundaryEdge(wallId));

        Assert.Throws<InvalidOperationException>(
            () => room.AddBoundaryEdge(
                new BoundaryEdge(
                    wallId,
                    new WallSpan(
                        Length.FromMillimeters(1000),
                        Length.FromMillimeters(3000)))));
    }

    [Fact]
    public void BoundaryLoopDoesNotDuplicateTheSamePhysicalSpan()
    {
        var wallId = EntityId.New();

        var span = new WallSpan(
            Length.FromMillimeters(1000),
            Length.FromMillimeters(3000));

        var room = new Room("Room");

        room.AddBoundaryEdge(
            new BoundaryEdge(
                wallId,
                span));

        room.AddBoundaryEdge(
            new BoundaryEdge(
                wallId,
                span,
                IsReversed: true));

        Assert.Single(
            room.Boundary.OuterLoop.Edges);
    }
}
