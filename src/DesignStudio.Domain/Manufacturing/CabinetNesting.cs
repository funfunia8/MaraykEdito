using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Manufacturing;

public sealed record NestingOptions(
    Length Kerf,
    Length SheetMargin,
    bool AllowRotation = true,
    Length MinimumRemnantWidth = default,
    Length MinimumRemnantHeight = default)
{
    public static NestingOptions Default => new(
        Length.FromMillimeters(3),
        Length.FromMillimeters(10),
        true,
        Length.FromMillimeters(200),
        Length.FromMillimeters(300));
}

public sealed record NestingPlacement(
    CabinetPartType PartType,
    string PartName,
    EntityId? MaterialId,
    Length X,
    Length Y,
    Length Width,
    Length Height,
    bool Rotated90Degrees,
    int SourceQuantityIndex,
    EdgeBandFlags EdgeBanding);

public sealed record NestingRemnant(
    Length X,
    Length Y,
    Length Width,
    Length Height,
    double AreaSquareMeters)
{
    public bool IsUsable => Width.Millimeters > 0 && Height.Millimeters > 0;
}

public sealed record NestingSheet(
    int SheetIndex,
    EntityId MaterialId,
    string MaterialCode,
    Length Width,
    Length Height,
    IReadOnlyList<NestingPlacement> Placements,
    IReadOnlyList<NestingRemnant> CandidateRemnants)
{
    public double SheetAreaSquareMeters => Width.Millimeters * Height.Millimeters / 1_000_000.0;
    public double UsedAreaSquareMeters => Placements.Sum(x => x.Width.Millimeters * x.Height.Millimeters) / 1_000_000.0;
    public double WasteAreaSquareMeters => Math.Max(0, SheetAreaSquareMeters - UsedAreaSquareMeters);
    public double UtilizationPercent => SheetAreaSquareMeters <= 0 ? 0 : UsedAreaSquareMeters / SheetAreaSquareMeters * 100.0;
}

public sealed record NestingResult(
    IReadOnlyList<NestingSheet> Sheets,
    IReadOnlyList<CabinetCutListLine> UnplacedParts)
{
    public bool IsFeasible => UnplacedParts.Count == 0;
    public int SheetCount => Sheets.Count;
    public double TotalWasteAreaSquareMeters => Sheets.Sum(x => x.WasteAreaSquareMeters);
}
