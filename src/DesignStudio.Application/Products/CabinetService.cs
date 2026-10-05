using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Products;

public sealed class CabinetService
{
    private readonly CabinetParametricService _parametric = new();
    private readonly CabinetMaterialService _materials = new();

    public (Cabinet Cabinet, CabinetGenerationResult Generation) Create(
        Project project,
        string name = "Base Cabinet",
        double widthMm = 600,
        double heightMm = 720,
        double depthMm = 560,
        double panelThicknessMm = 18,
        double backThicknessMm = 6,
        CabinetConstructionMethod constructionMethod = CabinetConstructionMethod.CapturedBackGroove,
        int shelfCount = 2,
        int doorCount = 1)
    {
        ArgumentNullException.ThrowIfNull(project);
        var cabinet = new Cabinet(
            name,
            DesignStudio.Domain.Units.Length.FromMillimeters(widthMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(heightMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(depthMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(panelThicknessMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(backThicknessMm),
            constructionMethod,
            shelfCount,
            doorCount);

        project.Add(cabinet);

        var carcassMaterial = _materials.GetOrCreatePanel(project, "PANEL-18", "Panel 18mm", 18);
        var backMaterial = _materials.GetOrCreatePanel(project, "BACK-06", "Back 6mm", 6);
        var edgeBandMaterial = _materials.GetOrCreateEdgeBand(project, "EDGE-01", "Edge Band 1mm", 1, 23);
        cabinet.SetMaterialAssignments(carcassMaterial.Id, backMaterial.Id, carcassMaterial.Id, edgeBandMaterial.Id);

        var generation = _parametric.Generate(cabinet);
        if (!generation.Validation.IsValid)
        {
            project.Remove(cabinet.Id);
            throw new InvalidOperationException(string.Join(Environment.NewLine, generation.Validation.Issues.Select(x => x.Message)));
        }

        return (cabinet, generation);
    }

    public CabinetGenerationResult Regenerate(Cabinet cabinet) => _parametric.Generate(cabinet);

    public CabinetGenerationResult Update(
        Cabinet cabinet,
        double widthMm,
        double heightMm,
        double depthMm,
        double panelThicknessMm,
        double backThicknessMm,
        int shelfCount,
        int doorCount,
        CabinetConstructionMethod constructionMethod)
    {
        cabinet.SetDimensions(
            DesignStudio.Domain.Units.Length.FromMillimeters(widthMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(heightMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(depthMm));
        cabinet.SetPanelThickness(DesignStudio.Domain.Units.Length.FromMillimeters(panelThicknessMm));
        cabinet.SetBackThickness(DesignStudio.Domain.Units.Length.FromMillimeters(backThicknessMm));
        cabinet.SetShelfCount(shelfCount);
        cabinet.SetDoorCount(doorCount);
        cabinet.SetConstructionMethod(constructionMethod);

        var result = _parametric.Generate(cabinet);
        if (!result.Validation.IsValid)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Validation.Issues.Select(x => x.Message)));
        return result;
    }
}
