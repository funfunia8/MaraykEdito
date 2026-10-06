using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Project;

public readonly record struct BoundaryEdge
{
    public BoundaryEdge(
        EntityId wallId)
        : this(
            wallId,
            span: null,
            IsReversed: false)
    {
    }

    public BoundaryEdge(
        EntityId wallId,
        bool IsReversed)
        : this(
            wallId,
            span: null,
            IsReversed)
    {
    }

    public BoundaryEdge(
        EntityId wallId,
        WallSpan span,
        bool IsReversed = false)
        : this(
            wallId,
            (WallSpan?)span,
            IsReversed)
    {
    }

    private BoundaryEdge(
        EntityId wallId,
        WallSpan? span,
        bool IsReversed)
    {
        WallId = wallId;
        Span = span;
        this.IsReversed = IsReversed;
    }

    public EntityId WallId { get; }

    /// <summary>
    /// Physical span of the wall used by this boundary edge.
    /// Null means the complete wall for backward compatibility.
    /// </summary>
    public WallSpan? Span { get; }

    /// <summary>
    /// Traversal direction through the physical wall span.
    /// False means StartOffset -> EndOffset.
    /// True means EndOffset -> StartOffset.
    /// </summary>
    public bool IsReversed { get; }

    public bool IsWholeWall => Span is null;

    public WallSpan GetEffectiveSpan(Length wallLength)
    {
        if (Span is not null)
            return Span.Value;

        if (!double.IsFinite(wallLength.Millimeters) ||
            wallLength.Millimeters <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wallLength),
                "Wall length must be a positive finite value.");
        }

        return new WallSpan(
            Length.FromMillimeters(0),
            wallLength);
    }

    /// <summary>
    /// Resolves this boundary edge into its directed physical wall segment.
    /// The returned segment follows the boundary traversal direction.
    /// </summary>
    public WallSegment ResolveSegment(Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        if (!double.IsFinite(wall.LengthMm) ||
            wall.LengthMm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wall),
                "Wall length must be a positive finite value.");
        }

        var span = GetEffectiveSpan(
            Length.FromMillimeters(wall.LengthMm));

        if (!span.IsWithin(
                Length.FromMillimeters(wall.LengthMm)))
        {
            throw new InvalidOperationException(
                $"Boundary edge span is outside wall {wall.Id}.");
        }

        var startOffsetMm = IsReversed
            ? span.EndOffset.Millimeters
            : span.StartOffset.Millimeters;

        var endOffsetMm = IsReversed
            ? span.StartOffset.Millimeters
            : span.EndOffset.Millimeters;

        return new WallSegment(
            PointAtOffset(
                wall,
                startOffsetMm),
            PointAtOffset(
                wall,
                endOffsetMm),
            wall.Thickness);
    }

    private static Point2D PointAtOffset(
        Wall wall,
        double offsetMm)
    {
        var lengthMm = wall.LengthMm;

        if (!double.IsFinite(offsetMm) ||
            offsetMm < 0 ||
            offsetMm > lengthMm)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offsetMm));
        }

        var ratio = offsetMm / lengthMm;

        return new Point2D(
            wall.Start.X +
            ((wall.End.X - wall.Start.X) * ratio),
            wall.Start.Y +
            ((wall.End.Y - wall.Start.Y) * ratio));
    }
}
