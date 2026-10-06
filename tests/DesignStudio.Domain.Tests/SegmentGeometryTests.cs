using DesignStudio.Domain.Geometry;

namespace DesignStudio.Domain.Tests;

public sealed class SegmentGeometryTests
{
    [Fact]
    public void CrossingSegmentsAreProperIntersection()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 10),
            new Point2D(0, 10),
            new Point2D(10, 0));

        Assert.Equal(
            SegmentRelation.ProperIntersection,
            relation);
    }

    [Fact]
    public void EndpointContactIsTouch()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10, 0),
            new Point2D(10, 10));

        Assert.Equal(
            SegmentRelation.Touch,
            relation);
    }

    [Fact]
    public void CollinearPartialOverlapIsOverlap()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(5, 0),
            new Point2D(15, 0));

        Assert.Equal(
            SegmentRelation.Overlap,
            relation);
    }

    [Fact]
    public void CollinearEndpointContactIsTouch()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10, 0),
            new Point2D(20, 0));

        Assert.Equal(
            SegmentRelation.Touch,
            relation);
    }

    [Fact]
    public void DisjointSegmentsAreNone()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(20, 0),
            new Point2D(30, 0));

        Assert.Equal(
            SegmentRelation.None,
            relation);
    }

    [Fact]
    public void CrossingOutsideSegmentBoundsIsNone()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(5, 0),
            new Point2D(10, -5),
            new Point2D(10, 5));

        Assert.Equal(
            SegmentRelation.None,
            relation);
    }

    [Fact]
    public void ClassificationIsSymmetric()
    {
        var firstStart = new Point2D(0, 0);
        var firstEnd = new Point2D(10, 10);
        var secondStart = new Point2D(0, 10);
        var secondEnd = new Point2D(10, 0);

        var forward = SegmentGeometry.Classify(
            firstStart,
            firstEnd,
            secondStart,
            secondEnd);

        var reverse = SegmentGeometry.Classify(
            secondStart,
            secondEnd,
            firstStart,
            firstEnd);

        Assert.Equal(forward, reverse);
    }

    [Fact]
    public void CollinearGapWithinToleranceIsTouch()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10.0005, 0),
            new Point2D(20, 0));

        Assert.Equal(
            SegmentRelation.Touch,
            relation);
    }

    [Fact]
    public void CollinearGapBeyondToleranceIsNone()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10.01, 0),
            new Point2D(20, 0));

        Assert.Equal(
            SegmentRelation.None,
            relation);
    }

    [Fact]
    public void EndpointDistanceWithinToleranceIsTouch()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10.0005, 0),
            new Point2D(10.0005, 10));

        Assert.Equal(
            SegmentRelation.Touch,
            relation);
    }

    [Fact]
    public void EndpointDistanceBeyondToleranceIsNone()
    {
        var relation = SegmentGeometry.Classify(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10.01, 0),
            new Point2D(10.01, 10));

        Assert.Equal(
            SegmentRelation.None,
            relation);
    }

    [Fact]
    public void NegativeToleranceIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => SegmentGeometry.Classify(
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(0, 1),
                new Point2D(10, 1),
                -0.001));
    }
}
