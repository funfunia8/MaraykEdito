using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Application.Commands;

public sealed class ResizeOpeningCommand : ICommand
{
    private readonly Project _project;
    private readonly EntityId _openingId;
    private readonly double _newWidthMm;
    private readonly double _newHeightMm;
    private readonly double? _newSillMm;
    private double _oldWidthMm;
    private double _oldHeightMm;
    private double? _oldSillMm;
    private bool _captured;

    public ResizeOpeningCommand(Project project, EntityId openingId, double newWidthMm, double newHeightMm, double? newSillMm = null)
    {
        _project = project;
        _openingId = openingId;
        _newWidthMm = newWidthMm;
        _newHeightMm = newHeightMm;
        _newSillMm = newSillMm;
    }

    public string Name => "Resize Opening";

    public void Execute()
    {
        if (!_captured) Capture();
        SetDimensions(_newWidthMm, _newHeightMm, _newSillMm);
    }

    public void Undo()
    {
        if (!_captured) return;
        SetDimensions(_oldWidthMm, _oldHeightMm, _oldSillMm);
    }

    private void Capture()
    {
        if (!_project.TryGet(_openingId, out var obj))
            throw new KeyNotFoundException("Opening not found.");

        switch (obj)
        {
            case Door door:
                _oldWidthMm = door.Width.Millimeters;
                _oldHeightMm = door.Height.Millimeters;
                _oldSillMm = null;
                break;
            case Window window:
                _oldWidthMm = window.Width.Millimeters;
                _oldHeightMm = window.Height.Millimeters;
                _oldSillMm = window.SillHeight.Millimeters;
                break;
            default:
                throw new InvalidOperationException("Selected object is not an opening.");
        }

        _captured = true;
    }

    private void SetDimensions(double widthMm, double heightMm, double? sillMm)
    {
        if (!_project.TryGet(_openingId, out var obj))
            throw new KeyNotFoundException("Opening not found.");

        switch (obj)
        {
            case Door door:
                door.SetDimensions(door.OffsetFromWallStart, Length.FromMillimeters(widthMm), Length.FromMillimeters(heightMm));
                break;
            case Window window:
                window.SetDimensions(window.OffsetFromWallStart, Length.FromMillimeters(widthMm), Length.FromMillimeters(heightMm), Length.FromMillimeters(sillMm ?? window.SillHeight.Millimeters));
                break;
            default:
                throw new InvalidOperationException("Selected object is not an opening.");
        }
    }
}
