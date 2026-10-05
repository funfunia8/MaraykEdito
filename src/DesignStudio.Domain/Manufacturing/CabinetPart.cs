using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Manufacturing;

public enum CabinetPartType
{
    LeftSide,
    RightSide,
    Top,
    Bottom,
    Back,
    Shelf,
    Door
}

[Flags]
public enum EdgeBandFlags
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
    Front = 16,
    Back = 32
}

public sealed record CabinetPart(
    CabinetPartType Type,
    string Name,
    Length Width,
    Length Height,
    Length Depth,
    Length Thickness,
    int Quantity,
    EdgeBandFlags EdgeBanding,
    bool GrainAlongWidth,
    EntityId? MaterialId = null,
    EntityId? EdgeBandMaterialId = null);
