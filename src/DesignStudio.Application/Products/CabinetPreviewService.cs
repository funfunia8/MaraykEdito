using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Products;

public sealed record CabinetPreviewBox(
    CabinetPartType PartType,
    string Name,
    double X,
    double Y,
    double Z,
    double Width,
    double Height,
    double Depth);

public sealed record CabinetPreview(
    double Width,
    double Height,
    double Depth,
    IReadOnlyList<CabinetPreviewBox> Parts);

/// <summary>
/// Renderer-independent preview layout. A later WPF/3D renderer consumes this state;
/// no rendering dependency leaks into the parametric product model.
/// </summary>
public sealed class CabinetPreviewService
{
    public CabinetPreview Build(Cabinet cabinet, CabinetGenerationResult generation)
    {
        ArgumentNullException.ThrowIfNull(cabinet);
        ArgumentNullException.ThrowIfNull(generation);

        var p = cabinet.PanelThickness.Millimeters;
        var w = cabinet.Width.Millimeters;
        var h = cabinet.Height.Millimeters;
        var d = cabinet.Depth.Millimeters;
        var boxes = new List<CabinetPreviewBox>();

        foreach (var part in generation.Parts)
        {
            switch (part.Type)
            {
                case CabinetPartType.LeftSide:
                    boxes.Add(new(part.Type, part.Name, 0, 0, 0, p, h, d));
                    break;
                case CabinetPartType.RightSide:
                    boxes.Add(new(part.Type, part.Name, Math.Max(0, w - p), 0, 0, p, h, d));
                    break;
                case CabinetPartType.Bottom:
                    boxes.Add(new(part.Type, part.Name, p, 0, 0, Math.Max(0, w - 2 * p), p, d));
                    break;
                case CabinetPartType.Top:
                    boxes.Add(new(part.Type, part.Name, p, Math.Max(0, h - p), 0, Math.Max(0, w - 2 * p), p, d));
                    break;
                case CabinetPartType.Back:
                    boxes.Add(new(part.Type, part.Name, p, p, Math.Max(0, d - cabinet.BackThickness.Millimeters), Math.Max(0, w - 2 * p), Math.Max(0, h - 2 * p), cabinet.BackThickness.Millimeters));
                    break;
                default:
                    break;
            }
        }

        return new CabinetPreview(w, h, d, boxes);
    }
}
