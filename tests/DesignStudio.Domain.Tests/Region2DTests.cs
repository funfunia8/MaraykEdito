using DesignStudio.Domain.Geometry;

namespace DesignStudio.Domain.Tests;

public sealed class Region2DTests
{
    [Fact]
    public void RectangleReportsAreaAndOrientation()
    {
        var region =
            new Region2D(
                new[]
                {
                    new Point2D(0, 0),
                    new Point2D(5000, 0),
                    new Point2D(5000, 4000),
                    new Point2D(0, 4000)
                });

        Assert.Equal(
            20_000_000,
            region.AreaMm2,
            6);

        Assert.Equal(
            20_000_000,
            region.SignedArea,
            6);

        Assert.Equal(
            PolygonOrientation.CounterClockwise,
            region.Orientation);
    }

    [Fact]
    public void VerticesAreExposedAsReadOnly()
    {
        var region =
            CreateRectangle();

        var list =
            Assert.IsAssignableFrom<IList<Point2D>>(
                region.Vertices);

        Assert.True(
            list.IsReadOnly);

        Assert.Throws<NotSupportedException>(
            () =>
                list[0] =
                    new Point2D(9999, 9999));

        Assert.Equal(
            new Point2D(0, 0),
            region.Vertices[0]);
    }

    [Fact]
    public void InsidePointIsClassifiedAsInside()
    {
        var region =
            CreateRectangle();

        var containment =
            region.Classify(
                new Point2D(2500, 2000));

        Assert.Equal(
            PointContainment.Inside,
            containment);

        Assert.True(
            region.Contains(
                new Point2D(2500, 2000)));

        Assert.True(
            region.ContainsInterior(
                new Point2D(2500, 2000)));
    }

    [Fact]
    public void OutsidePointIsClassifiedAsOutside()
    {
        var region =
            CreateRectangle();

        var containment =
            region.Classify(
                new Point2D(6000, 2000));

        Assert.Equal(
            PointContainment.Outside,
            containment);

        Assert.False(
            region.Contains(
                new Point2D(6000, 2000)));

        Assert.False(
            region.ContainsInterior(
                new Point2D(6000, 2000)));
    }

    [Fact]
    public void PointOnEdgeIsClassifiedAsBoundary()
    {
        var region =
            CreateRectangle();

        Assert.Equal(
            PointContainment.Boundary,
            region.Classify(
                new Point2D(2500, 0)));

        Assert.True(
            region.Contains(
                new Point2D(2500, 0)));

        Assert.False(
            region.ContainsInterior(
                new Point2D(2500, 0)));
    }

    [Fact]
    public void PointOnVertexIsClassifiedAsBoundary()
    {
        var region =
            CreateRectangle();

        Assert.Equal(
            PointContainment.Boundary,
            region.Classify(
                new Point2D(0, 0)));
    }

    [Fact]
    public void ConcaveRegionClassifiesNotchAsOutside()
    {
        var region =
            new Region2D(
                new[]
                {
                    new Point2D(0, 0),
                    new Point2D(5000, 0),
                    new Point2D(5000, 2000),
                    new Point2D(3000, 2000),
                    new Point2D(3000, 4000),
                    new Point2D(0, 4000)
                });

        Assert.Equal(
            PointContainment.Inside,
            region.Classify(
                new Point2D(1000, 3000)));

        Assert.Equal(
            PointContainment.Inside,
            region.Classify(
                new Point2D(4500, 1000)));

        Assert.Equal(
            PointContainment.Outside,
            region.Classify(
                new Point2D(4000, 3000)));
    }

    [Fact]
    public void ReversedRegionStillSupportsContainment()
    {
        var region =
            new Region2D(
                new[]
                {
                    new Point2D(0, 4000),
                    new Point2D(5000, 4000),
                    new Point2D(5000, 0),
                    new Point2D(0, 0)
                });

        Assert.Equal(
            PolygonOrientation.Clockwise,
            region.Orientation);

        Assert.Equal(
            PointContainment.Inside,
            region.Classify(
                new Point2D(2500, 2000)));
    }

    [Fact]
    public void PointWithinBoundaryToleranceIsBoundary()
    {
        var region =
            CreateRectangle();

        Assert.Equal(
            PointContainment.Boundary,
            region.Classify(
                new Point2D(2500, 0.0005),
                0.001));

        Assert.Equal(
            PointContainment.Inside,
            region.Classify(
                new Point2D(2500, 0.01),
                0.001));
    }

    [Fact]
    public void FewerThanThreeVerticesAreRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new Region2D(
                    new[]
                    {
                        new Point2D(0, 0),
                        new Point2D(5000, 0)
                    }));
    }

    [Fact]
    public void DegenerateRegionIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new Region2D(
                    new[]
                    {
                        new Point2D(0, 0),
                        new Point2D(5000, 0),
                        new Point2D(10000, 0)
                    }));
    }

    [Fact]
    public void NonFiniteVertexIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new Region2D(
                    new[]
                    {
                        new Point2D(0, 0),
                        new Point2D(double.NaN, 0),
                        new Point2D(0, 5000)
                    }));
    }

    [Fact]
    public void NonFinitePointIsRejected()
    {
        var region =
            CreateRectangle();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                region.Classify(
                    new Point2D(
                        double.NaN,
                        1000)));
    }

    [Fact]
    public void NegativeContainmentToleranceIsRejected()
    {
        var region =
            CreateRectangle();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                region.Classify(
                    new Point2D(2500, 2000),
                    -0.001));
    }

    [Fact]
    public void SelfIntersectingRegionIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new Region2D(
                    new[]
                    {
                        new Point2D(0, 0),
                        new Point2D(5000, 4000),
                        new Point2D(0, 4000),
                        new Point2D(5000, 0)
                    }));
    }

    [Fact]
    public void ZeroLengthBoundaryEdgeIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new Region2D(
                    new[]
                    {
                        new Point2D(0, 0),
                        new Point2D(5000, 0),
                        new Point2D(5000, 0),
                        new Point2D(0, 4000)
                    }));
    }

    [Fact]
    public void ConcaveSimpleRegionIsAccepted()
    {
        var region =
            new Region2D(
                new[]
                {
                    new Point2D(0, 0),
                    new Point2D(5000, 0),
                    new Point2D(5000, 2000),
                    new Point2D(3000, 2000),
                    new Point2D(3000, 4000),
                    new Point2D(0, 4000)
                });

        Assert.Equal(
            PointContainment.Inside,
            region.Classify(
                new Point2D(1000, 3000)));
    }

    private static Region2D CreateRectangle()
        => new(
            new[]
            {
                new Point2D(0, 0),
                new Point2D(5000, 0),
                new Point2D(5000, 4000),
                new Point2D(0, 4000)
            });
}
