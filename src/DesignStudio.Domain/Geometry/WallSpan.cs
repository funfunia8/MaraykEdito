using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Geometry;

/// <summary>
/// A measured sub-range of a wall, expressed from the wall's physical Start point.
/// Direction is not part of this value; traversal direction belongs to BoundaryEdge.
/// </summary>
public readonly record struct WallSpan
{
    public WallSpan(
        Length startOffset,
        Length endOffset)
    {
        ValidateFinite(startOffset, nameof(startOffset));
        ValidateFinite(endOffset, nameof(endOffset));

        if (startOffset.Millimeters < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startOffset),
                "Wall span start offset cannot be negative.");
        }

        if (endOffset.Millimeters <= startOffset.Millimeters)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endOffset),
                "Wall span end offset must be greater than its start offset.");
        }

        StartOffset = startOffset;
        EndOffset = endOffset;
    }

    public Length StartOffset { get; }

    public Length EndOffset { get; }

    public Length Length =>
        Length.FromMillimeters(
            EndOffset.Millimeters - StartOffset.Millimeters);

    public bool ContainsOffset(
        Length offset,
        double toleranceMm = 0.001)
    {
        ValidateFinite(offset, nameof(offset));

        if (toleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toleranceMm));
        }

        return offset.Millimeters >= StartOffset.Millimeters - toleranceMm &&
               offset.Millimeters <= EndOffset.Millimeters + toleranceMm;
    }

    public bool IsWithin(
        Length wallLength,
        double toleranceMm = 0.001)
    {
        ValidateFinite(wallLength, nameof(wallLength));

        if (wallLength.Millimeters <= 0)
        {
            return false;
        }

        if (toleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toleranceMm));
        }

        return StartOffset.Millimeters >= -toleranceMm &&
               EndOffset.Millimeters <= wallLength.Millimeters + toleranceMm;
    }

    public bool Overlaps(
        WallSpan other,
        double toleranceMm = 0.001)
    {
        if (toleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toleranceMm));
        }

        return StartOffset.Millimeters <
               other.EndOffset.Millimeters - toleranceMm &&
               EndOffset.Millimeters >
               other.StartOffset.Millimeters + toleranceMm;
    }

    private static void ValidateFinite(
        Length value,
        string parameterName)
    {
        if (!double.IsFinite(value.Millimeters))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Wall span offsets must be finite numbers.");
        }
    }
}
