using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Application.Products;

public sealed class CabinetMaterialService
{
    public Material GetOrCreatePanel(Project project, string code, string name, double thicknessMm, double sheetWidthMm = 2800, double sheetHeightMm = 2070)
        => GetOrCreate(project, code, name, MaterialKind.Sheet, thicknessMm, sheetWidthMm, sheetHeightMm, 0, MaterialGrainDirection.AlongWidth);

    public Material GetOrCreateEdgeBand(Project project, string code, string name, double thicknessMm, double bandWidthMm)
        => GetOrCreate(project, code, name, MaterialKind.EdgeBand, thicknessMm, 0, 0, bandWidthMm, MaterialGrainDirection.AlongHeight);

    private static Material GetOrCreate(
        Project project,
        string code,
        string name,
        MaterialKind kind,
        double thicknessMm,
        double sheetWidthMm,
        double sheetHeightMm,
        double bandWidthMm,
        MaterialGrainDirection grainDirection)
    {
        ArgumentNullException.ThrowIfNull(project);
        var existing = project.Objects.OfType<Material>().FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) return existing;

        var material = new Material(
            code,
            name,
            kind,
            Length.FromMillimeters(thicknessMm),
            Length.FromMillimeters(sheetWidthMm),
            Length.FromMillimeters(sheetHeightMm),
            Length.FromMillimeters(bandWidthMm),
            grainDirection);
        project.Add(material);
        return material;
    }
}
