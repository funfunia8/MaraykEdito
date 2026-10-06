using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Tests;

public sealed class WallSpanTests
{
    [Fact]
    public void ConstructorPreservesOffsetsAndCalculatesLength()
    {
        var span = new WallSpan(
            Length.FromMillimeters(1000),
            Length.FromMillimeters(3500));

        Assert.Equal(1000, span.StartOffset.Millimeters, 3);
        Assert.Equal(3500, span.EndOffset.Millimeters, 3);
        Assert.Equal(2500, span.Length.Millimeters, 3);
    }

    [Fact]
    public void NegativeStartOffsetIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WallSpan(
                Length.FromMillimeters(-1),
                Length.FromMillimeters(1000)));
    }

    [Fact]
    public void EndNotGreaterThanStartIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WallSpan(
                Length.FromMillimeters(1000),
                Length.FromMillimeters(1000)));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WallSpan(
                Length.FromMillimeters(1000),
                Length.FromMillimeters(900)));
    }

    [Fact]
    public void NonFiniteOffsetIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WallSpan(
                Length.FromMillimeters(double.NaN),
                Length.FromMillimeters(1000)));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WallSpan(
                Length.FromMillimeters(0),
                Length.FromMillimeters(double.PositiveInfinity)));
    }

    [Fact]
    public void SpanWithinWallLengthIsAccepted()
    {
        var span = new WallSpan(
            Length.FromMillimeters(1000),
            Length.FromMillimeters(3500));

        Assert.True(
            span.IsWithin(
                Length.FromMillimeters(5000)));
    }

    [Fact]
    public void SpanBeyondWallLengthIsRejectedByRangeValidation()
    {
        var span = new WallSpan(
            Length.FromMillimeters(1000),
            Length.FromMillimeters(5500));

        Assert.False(
            span.IsWithin(
                Length.FromMillimeters(5000)));
    }

    [Fact]
    public void ContainsOffsetUsesTolerance()
    {
        var span = new WallSpan(
            Length.FromMillimeters(1000),
            Length.FromMillimeters(3500));

        Assert.True(
            span.ContainsOffset(
                Length.FromMillimeters(999.9995)));

        Assert.True(
            span.ContainsOffset(
                Length.FromMillimeters(3500.0005)));

        Assert.False(
            span.ContainsOffset(
                Length.FromMillimeters(998)));
    }

    [Fact]
    public void DisjointSpansDoNotOverlap()
    {
        var first = new WallSpan(
            Length.FromMillimeters(0),
            Length.FromMillimeters(1000));

        var second = new WallSpan(
            Length.FromMillimeters(1000),
            Length.FromMillimeters(2000));

        Assert.False(first.Overlaps(second));
        Assert.False(second.Overlaps(first));
    }

    [Fact]
    public void PartiallyOverlappingSpansOverlap()
    {
        var first = new WallSpan(
            Length.FromMillimeters(500),
            Length.FromMillimeters(2500));

        var second = new WallSpan(
            Length.FromMillimeters(2000),
            Length.FromMillimeters(3500));

        Assert.True(first.Overlaps(second));
        Assert.True(second.Overlaps(first));
    }
}
