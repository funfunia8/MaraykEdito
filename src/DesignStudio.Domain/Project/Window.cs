using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Project;

public sealed class Window : ProjectObject
{
    public Window(EntityId hostWallId, Length offsetFromWallStart, Length width, Length height, Length sillHeight, EntityId? id = null)
        : base(id)
    {
        SetCategory(ObjectCategory.Opening);
        HostWallId = hostWallId;
        SetParent(hostWallId);
        SetDimensions(offsetFromWallStart, width, height, sillHeight);
    }

    public override string Type => "Window";
    public EntityId HostWallId { get; private set; }
    public Length OffsetFromWallStart { get; private set; }
    public Length Width { get; private set; }
    public Length Height { get; private set; }
    public Length SillHeight { get; private set; }

    public void SetDimensions(Length offset, Length width, Length height, Length sillHeight)
    {
        if (offset.Millimeters < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        if (width.Millimeters <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height.Millimeters <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (sillHeight.Millimeters < 0) throw new ArgumentOutOfRangeException(nameof(sillHeight));
        OffsetFromWallStart = offset;
        Width = width;
        Height = height;
        SillHeight = sillHeight;
    }
}
