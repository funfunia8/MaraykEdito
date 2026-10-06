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

        var orientation = PolygonGeometry.GetOrientation(
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

        var orientation = PolygonGeometry.GetOrientation(
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

        var orientation = PolygonGeometry.GetOrientation(
            points,
            0.001);

        Assert.Equal(
            PolygonOrientation.Degenerate,
            orientation);
    }

    [Fact]
    public void NearZeroAreaWithinGeometricToleranceIsDegenerate()
    {
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(1000, 0),
            new Point2D(1000, 0.0005)
        };

        var orientation = PolygonGeometry.GetOrientation(
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

        var orientation = PolygonGeometry.GetOrientation(
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
            () => PolygonGeometry.GetOrientation(points, -0.001));
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
            () => PolygonGeometry.SignedArea(points));
    }
}
