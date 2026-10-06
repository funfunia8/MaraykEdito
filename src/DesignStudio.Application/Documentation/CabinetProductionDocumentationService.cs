using DesignStudio.Application.Products;
using DesignStudio.Domain.Documentation;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Application.Documentation;

public sealed class CabinetProductionDocumentationService
{
    public ProductionDocumentSet Build(
        Project project,
        Cabinet cabinet,
        CabinetGenerationResult generation,
        CabinetCutList cutList,
        NestingResult? nesting = null,
        string revision = "A")
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(cabinet);
        ArgumentNullException.ThrowIfNull(generation);
        ArgumentNullException.ThrowIfNull(cutList);

        var shopDrawings = BuildShopDrawings(cabinet, generation.Parts);
        var cutDocument = BuildCutDocument(project, cutList, nesting);
        var hardwarePlan = new Products.CabinetHardwarePlanningService().Build(project, cabinet);
        var assembly = BuildAssemblyDocument(cabinet, generation, hardwarePlan);
        var documentCode = $"CAB-{cabinet.Id.Value:N}".ToUpperInvariant();

        return new ProductionDocumentSet(
            cabinet.Id,
            documentCode,
            string.IsNullOrWhiteSpace(revision) ? "A" : revision.Trim().ToUpperInvariant(),
            shopDrawings,
            cutDocument,
            assembly);
    }

    private static ShopDrawingSet BuildShopDrawings(Cabinet cabinet, IReadOnlyList<CabinetPart> parts)
    {
        var layout = new CabinetLayoutService().Build(cabinet);

        var frontNotes = new List<string>
        {
            "Show overall cabinet width and height.",
            $"Door count: {cabinet.DoorCount}.",
            "Verify opening and hardware clearances before fabrication."
        };

        var sideNotes = new List<string>
        {
            $"Construction: {cabinet.ConstructionMethod}.",
            $"Back thickness: {cabinet.BackThickness.Millimeters:0.##} mm.",
            "Confirm back-groove dimensions from the manufacturing specification."
        };

        var topNotes = new List<string>
        {
            "Show overall depth and carcass thickness.",
            "Grain direction follows the assigned material rule."
        };

        var views = new[]
        {
            new ShopDrawingView(
                ShopDrawingViewType.FrontElevation,
                "Front Elevation",
                cabinet.Width,
                cabinet.Height,
                Length.FromMillimeters(0),
                new[]
                {
                    new DimensionNote("Overall Width", cabinet.Width),
                    new DimensionNote("Overall Height", cabinet.Height),
                    new DimensionNote("Clear Width", layout.ClearWidth),
                    new DimensionNote("Clear Height", layout.ClearHeight)
                },
                frontNotes),
            new ShopDrawingView(
                ShopDrawingViewType.SideElevation,
                "Side Elevation",
                cabinet.Depth,
                cabinet.Height,
                cabinet.Depth,
                new[]
                {
                    new DimensionNote("Overall Depth", cabinet.Depth),
                    new DimensionNote("Overall Height", cabinet.Height),
                    new DimensionNote("Back Thickness", cabinet.BackThickness)
                },
                sideNotes),
            new ShopDrawingView(
                ShopDrawingViewType.TopPlan,
                "Top Plan",
                cabinet.Width,
                cabinet.Depth,
                cabinet.Depth,
                new[]
                {
                    new DimensionNote("Overall Width", cabinet.Width),
                    new DimensionNote("Overall Depth", cabinet.Depth),
                    new DimensionNote("Panel Thickness", cabinet.PanelThickness)
                },
                topNotes)
        };

        var frontElements = new List<ShopDrawingElement>
        {
            new ShopDrawingElement(
                ShopDrawingElementType.LeftSide,
                1,
                "Left Side",
                Length.FromMillimeters(0),
                Length.FromMillimeters(0),
                cabinet.PanelThickness,
                cabinet.Height,
                cabinet.PanelThickness),

            new ShopDrawingElement(
                ShopDrawingElementType.RightSide,
                1,
                "Right Side",
                Length.FromMillimeters(
                    cabinet.Width.Millimeters -
                    cabinet.PanelThickness.Millimeters),
                Length.FromMillimeters(0),
                cabinet.PanelThickness,
                cabinet.Height,
                cabinet.PanelThickness),

            new ShopDrawingElement(
                ShopDrawingElementType.Bottom,
                1,
                "Bottom",
                Length.FromMillimeters(cabinet.PanelThickness.Millimeters),
                Length.FromMillimeters(0),
                layout.ClearWidth,
                cabinet.PanelThickness,
                cabinet.PanelThickness),

            new ShopDrawingElement(
                ShopDrawingElementType.Top,
                1,
                "Top",
                Length.FromMillimeters(cabinet.PanelThickness.Millimeters),
                Length.FromMillimeters(
                    cabinet.Height.Millimeters -
                    cabinet.PanelThickness.Millimeters),
                layout.ClearWidth,
                cabinet.PanelThickness,
                cabinet.PanelThickness)
        };
foreach (var shelf in layout.Shelves)
{
    frontElements.Add(
        new ShopDrawingElement(
            ShopDrawingElementType.Shelf,
            shelf.Index,
            $"Shelf {shelf.Index}",
            shelf.X,
            shelf.Y,
            shelf.Width,
            shelf.Thickness,
            shelf.Thickness));
}

        foreach (var door in layout.Doors)
        {
            frontElements.Add(
                new ShopDrawingElement(
                    ShopDrawingElementType.Door,
                    door.Index,
                    $"Door {door.Index}",
                    door.X,
                    door.Y,
                    door.Width,
                    door.Height,
                    door.Thickness));
        }

        var frontLayout = new CabinetFrontElevationLayout(
            cabinet.Width,
            cabinet.Height,
            layout.ClearWidth,
            layout.ClearHeight,
            frontElements);

        var partViews = parts
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .Select(x => new PartDrawingView(
                x.Type,
                x.Name,
                x.Width,
                x.Height,
                x.Thickness,
                x.Quantity,
                x.EdgeBanding,
                x.GrainAlongWidth))
            .ToList();

        return new ShopDrawingSet(
            views,
            partViews,
            frontLayout);
    }

    private static CutDocument BuildCutDocument(Project project, CabinetCutList cutList, NestingResult? nesting)
    {
        var lines = cutList.Lines
            .Select(line => new CutDocumentLine(
                line.PartType,
                line.Name,
                ResolveMaterialCode(project, line.MaterialId),
                line.Width,
                line.Height,
                line.Thickness,
                line.Quantity,
                line.EdgeBanding,
                line.GrainAlongWidth))
            .ToList();

        var sheets = (nesting?.Sheets ?? Array.Empty<NestingSheet>())
            .OrderBy(x => x.SheetIndex)
            .Select(sheet => new SheetCutDocument(
                sheet.SheetIndex,
                sheet.MaterialCode,
                sheet.Width,
                sheet.Height,
                sheet.UtilizationPercent,
                sheet.WasteAreaSquareMeters,
                sheet.Placements.Select(p => new SheetPlacementDocument(
                    p.SourceQuantityIndex,
                    p.PartType,
                    p.PartName,
                    p.X,
                    p.Y,
                    p.Width,
                    p.Height,
                    p.Rotated90Degrees,
                    p.EdgeBanding)).ToList(),
                sheet.CandidateRemnants.Select(r => new RemnantDocument(
                    r.X,
                    r.Y,
                    r.Width,
                    r.Height,
                    r.AreaSquareMeters)).ToList()))
            .ToList();

        return new CutDocument(
            lines,
            sheets,
            nesting?.UnplacedParts ?? Array.Empty<CabinetCutListLine>(),
            nesting?.IsFeasible ?? true);
    }

    private static AssemblyDocument BuildAssemblyDocument(Cabinet cabinet, CabinetGenerationResult generation, DesignStudio.Domain.Manufacturing.CabinetHardwarePlan hardwarePlan)
    {
        var steps = new List<AssemblyStep>
        {
            new(10, "Prepare Parts", "Verify each part against the shop drawing dimensions, material, grain and edge-banding requirements."),
            new(20, "Assemble Carcass", "Assemble left side, right side, bottom and top using the selected construction method."),
            new(30, "Install Back", generation.ManufacturingSpec.GrooveDepth.Millimeters > 0
                ? $"Fit the back into the prepared groove. Groove width {generation.ManufacturingSpec.GrooveWidth.Millimeters:0.##} and Groove depth {generation.ManufacturingSpec.GrooveDepth.Millimeters:0.##} mm."
                : "Install the back using the selected construction method."),
            new(40, "Install Shelves", cabinet.ShelfCount > 0
                ? $"Install {cabinet.ShelfCount} shelf position(s) and verify equal clearances."
                : "No fixed shelves are defined."),
            new(50, "Install Doors", cabinet.DoorCount > 0
                ? $"Install {cabinet.DoorCount} door(s), align reveals and verify operation."
                : "No doors are defined."),
            new(60, "Final Inspection", "Confirm overall dimensions, squareness, edge condition, hardware operation and finish quality before release.")
        };

        return new AssemblyDocument(steps, hardwarePlan.Lines);
    }

    private static string ResolveMaterialCode(Project project, DesignStudio.Domain.Identity.EntityId? materialId)
    {
        if (materialId is null)
            return "UNASSIGNED";

        return project.TryGetMaterial(materialId.Value, out var material) && material is not null
            ? material.Code
            : materialId.Value.Value.ToString("D");
    }
}
