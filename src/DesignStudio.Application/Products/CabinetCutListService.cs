using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Products;

public sealed class CabinetCutListService
{
    public CabinetCutList Build(CabinetGenerationResult generation)
    {
        ArgumentNullException.ThrowIfNull(generation);
        if (!generation.Validation.IsValid)
            return new CabinetCutList(Array.Empty<CabinetCutListLine>());

        var lines = generation.Parts
            .GroupBy(x => new
            {
                x.Type,
                x.Name,
                x.MaterialId,
                x.EdgeBandMaterialId,
                Width = x.Width.Millimeters,
                Height = x.Height.Millimeters,
                Thickness = x.Thickness.Millimeters,
                x.EdgeBanding,
                x.GrainAlongWidth
            })
            .Select(g => new CabinetCutListLine(
                g.Key.Type,
                g.Key.Name,
                g.Key.MaterialId,
                g.Key.EdgeBandMaterialId,
                DesignStudio.Domain.Units.Length.FromMillimeters(g.Key.Width),
                DesignStudio.Domain.Units.Length.FromMillimeters(g.Key.Height),
                DesignStudio.Domain.Units.Length.FromMillimeters(g.Key.Thickness),
                g.Sum(x => x.Quantity),
                g.Key.EdgeBanding,
                g.Key.GrainAlongWidth))
            .OrderBy(x => x.PartType)
            .ThenBy(x => x.Name)
            .ToList();

        return new CabinetCutList(lines);
    }
}
