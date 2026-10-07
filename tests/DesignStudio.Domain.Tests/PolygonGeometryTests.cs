using DesignStudio.Domain.Geometry;

namespace DesignStudio.Domain.Tests;

public sealed class PolygonGeometryTests
{
    [Fact]
    public void CounterClockwiseRectangleHasPositiveSignedArea()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(5000, 0),
            new Point2D(5000, 4000),
            new Point2D(0, 4000)
        };

        var area = PolygonGeometry.SignedArea(points);

        Assert.Equal(20_000_000, area, 6);
    }

    [Fact]
    public void ClockwiseRectangleHasNegativeSignedArea()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(0, 4000),
            new Point2D(5000, 4000),
            new Point2D(5000, 0)
        };

        var area = PolygonGeometry.SignedArea(points);

        Assert.Equal(-20_000_000, area, 6);
    }

    [Fact]
    public void TriangleSignedAreaIsComputedCorrectly()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 3)
        };

        var area = PolygonGeometry.SignedArea(points);

        Assert.Equal(6, area, 6);
    }

    [Fact]
    public void CounterClockwisePolygonIsClassifiedCorrectly()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(5000, 0),
            new Point2D(5000, 4000),
            new Point2D(0, 4000)
        };

        var orientation =
            PolygonGeometry.GetOrientation(
                points,
                0.001);

        Assert.Equal(
            PolygonOrientation.CounterClockwise,
            orientation);
    }

    [Fact]
    public void ClockwisePolygonIsClassifiedCorrectly()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(0, 4000),
            new Point2D(5000, 4000),
            new Point2D(5000, 0)
        };

        var orientation =
            PolygonGeometry.GetOrientation(
                points,
                0.001);

        Assert.Equal(
            PolygonOrientation.Clockwise,
            orientation);
    }

    [Fact]
    public void CollinearPolygonIsDegenerate()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(5000, 0),
            new Point2D(10000, 0)
        };

        var orientation =
            PolygonGeometry.GetOrientation(
                points,
                0.001);

        Assert.Equal(
            PolygonOrientation.Degenerate,
            orientation);
    }

    [Fact]
    public void AreaWithinGeometricToleranceIsDegenerate()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(0.001, 0),
            new Point2D(0, 0.001)
        };

        var orientation =
            PolygonGeometry.GetOrientation(
                points,
                0.001);

        Assert.Equal(
            PolygonOrientation.Degenerate,
            orientation);
    }

    [Fact]
    public void AreaBeyondGeometricToleranceKeepsItsOrientation()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(1000, 0),
            new Point2D(1000, 0.01)
        };

        var orientation =
            PolygonGeometry.GetOrientation(
                points,
                0.001);

        Assert.Equal(
            PolygonOrientation.CounterClockwise,
            orientation);
    }

    [Fact]
    public void NegativeToleranceIsRejected()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(5000, 0),
            new Point2D(5000, 4000)
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                PolygonGeometry.GetOrientation(
                    points,
                    -0.001));
    }

    [Fact]
    public void FewerThanThreePointsAreRejected()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(5000, 0)
        };

        Assert.Throws<ArgumentException>(
            () =>
                PolygonGeometry.SignedArea(points));
    }

    [Fact]
    public void SelfIntersectingPolygonIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                PolygonGeometry.ValidateSimple(
                    new[]
                    {
                        new Point2D(0, 0),
                        new Point2D(5000, 4000),
                        new Point2D(0, 4000),
                        new Point2D(5000, 0)
                    }));
    }

    [Fact]
    public void NonAdjacentTouchIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                PolygonGeometry.ValidateSimple(
                    new[]
                    {
                        new Point2D(0, 0),
                        new Point2D(5000, 0),
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
                PolygonGeometry.ValidateSimple(
                    new[]
                    {
                        new Point2D(0, 0),
                        new Point2D(5000, 0),
                        new Point2D(5000, 0),
                        new Point2D(0, 4000)
                    }));
    }

    [Fact]
    public void ConcaveSimplePolygonIsAccepted()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(5000, 0),
            new Point2D(5000, 2000),
            new Point2D(3000, 2000),
            new Point2D(3000, 4000),
            new Point2D(0, 4000)
        };

        PolygonGeometry.ValidateSimple(points);
    }

    [Fact]
    public void NearZeroAreaPolygonIsDegenerate()
    {
        var orientation =
            PolygonGeometry.GetOrientation(
                new[]
                {
                    new Point2D(0, 0),
                    new Point2D(5000, 0),
                    new Point2D(2500, 0.0000000001)
                },
                0.001);

        Assert.Equal(
            PolygonOrientation.Degenerate,
            orientation);
    }

}
