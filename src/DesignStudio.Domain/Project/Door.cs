using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Project;

public sealed class Door : ProjectObject
{
    public Door(EntityId hostWallId, Length offsetFromWallStart, Length width, Length height, EntityId? id = null)
        : base(id)
    {
        SetCategory(ObjectCategory.Opening);
        SetParent(hostWallId);
        SetDimensions(offsetFromWallStart, width, height);
    }

    public override string Type => "Door";
    public EntityId HostWallId => ParentId
    ?? throw new InvalidOperationException("Door must have a host wall.");
    public Length OffsetFromWallStart { get; private set; }
    public Length Width { get; private set; }
    public Length Height { get; private set; }

    public void SetDimensions(Length offset, Length width, Length height)
    {
        if (offset.Millimeters < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        if (width.Millimeters <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height.Millimeters <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        OffsetFromWallStart = offset;
        Width = width;
        Height = height;
    }
}
