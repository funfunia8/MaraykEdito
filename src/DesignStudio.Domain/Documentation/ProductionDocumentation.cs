using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Documentation;

public enum ShopDrawingViewType
{
    FrontElevation,
    SideElevation,
    TopPlan
}

public sealed record DimensionNote(string Label, Length Value);

public sealed record ShopDrawingView(
    ShopDrawingViewType ViewType,
    string Title,
    Length Width,
    Length Height,
    Length Depth,
    IReadOnlyList<DimensionNote> Dimensions,
    IReadOnlyList<string> Notes);

public sealed record ShopDrawingElement(
    ShopDrawingElementType ElementType,
    int Index,
    string Name,
    Length X,
    Length Y,
    Length Width,
    Length Height,
    Length Thickness);

public enum ShopDrawingElementType
{
    LeftSide,
    RightSide,
    Top,
    Bottom,
    Shelf,
    Door
}

public sealed record CabinetFrontElevationLayout(
    Length OverallWidth,
    Length OverallHeight,
    Length ClearWidth,
    Length ClearHeight,
    IReadOnlyList<ShopDrawingElement> Elements);

public sealed record PartDrawingView(
    CabinetPartType PartType,
    string PartName,
    Length Width,
    Length Height,
    Length Thickness,
    int Quantity,
    EdgeBandFlags EdgeBanding,
    bool GrainAlongWidth);

public sealed record ShopDrawingSet(
    IReadOnlyList<ShopDrawingView> Views,
    IReadOnlyList<PartDrawingView> PartViews,
    CabinetFrontElevationLayout? FrontElevationLayout = null);

public sealed record CutDocumentLine(
    CabinetPartType PartType,
    string PartName,
    string MaterialCode,
    Length Width,
    Length Height,
    Length Thickness,
    int Quantity,
    EdgeBandFlags EdgeBanding,
    bool GrainAlongWidth);

public sealed record SheetPlacementDocument(
    int ItemIndex,
    CabinetPartType PartType,
    string PartName,
    Length X,
    Length Y,
    Length Width,
    Length Height,
    bool Rotated90Degrees,
    EdgeBandFlags EdgeBanding);

public sealed record RemnantDocument(
    Length X,
    Length Y,
    Length Width,
    Length Height,
    double AreaSquareMeters);

public sealed record SheetCutDocument(
    int SheetIndex,
    string MaterialCode,
    Length Width,
    Length Height,
    double UtilizationPercent,
    double WasteAreaSquareMeters,
    IReadOnlyList<SheetPlacementDocument> Placements,
    IReadOnlyList<RemnantDocument> CandidateRemnants);

public sealed record CutDocument(
    IReadOnlyList<CutDocumentLine> Lines,
    IReadOnlyList<SheetCutDocument> Sheets,
    IReadOnlyList<CabinetCutListLine> UnplacedParts,
    bool NestingFeasible);

public sealed record AssemblyStep(int Sequence, string Title, string Instruction);

public sealed record AssemblyDocument(
    IReadOnlyList<AssemblyStep> Steps,
    IReadOnlyList<HardwarePlanLine>? Hardware = null)
{
    public IReadOnlyList<HardwarePlanLine> HardwareLines => Hardware ?? Array.Empty<HardwarePlanLine>();
}

public sealed record ProductionDocumentSet(
    EntityId CabinetId,
    string DocumentCode,
    string Revision,
    ShopDrawingSet ShopDrawings,
    CutDocument CutDocument,
    AssemblyDocument AssemblyDocument);
