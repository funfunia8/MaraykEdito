using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Manufacturing;

public sealed record CabinetCutListLine(
    CabinetPartType PartType,
    string Name,
    EntityId? MaterialId,
    EntityId? EdgeBandMaterialId,
    Length Width,
    Length Height,
    Length Thickness,
    int Quantity,
    EdgeBandFlags EdgeBanding,
    bool GrainAlongWidth)
{
    public double AreaSquareMeters => (Width.Millimeters * Height.Millimeters * Quantity) / 1_000_000.0;

    public string MaterialKey => MaterialId?.Value.ToString("D") ?? string.Empty;
}

public sealed record CabinetCutList(
    IReadOnlyList<CabinetCutListLine> Lines)
{
    public int TotalPartCount => Lines.Sum(x => x.Quantity);
    public double TotalPanelAreaSquareMeters => Lines.Sum(x => x.AreaSquareMeters);
}
