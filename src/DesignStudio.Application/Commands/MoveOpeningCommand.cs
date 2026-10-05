using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Application.Commands;

public sealed class MoveOpeningCommand : ICommand
{
    private readonly Project _project;
    private readonly EntityId _openingId;
    private readonly double _newOffsetMm;
    private double _oldOffsetMm;
    private bool _captured;

    public MoveOpeningCommand(Project project, EntityId openingId, double newOffsetMm)
    {
        _project = project;
        _openingId = openingId;
        _newOffsetMm = newOffsetMm;
    }

    public string Name => "Move Opening";

    public void Execute()
    {
        if (_project.Get<ProjectObject>(_openingId) is not (Door or Window))
            throw new InvalidOperationException("Opening not found.");

        if (!_captured)
        {
            _oldOffsetMm = GetOffsetMm();
            _captured = true;
        }

        SetOffset(_newOffsetMm);
    }

    public void Undo() => SetOffset(_oldOffsetMm);

    private double GetOffsetMm()
    {
        if (_project.TryGet(_openingId, out var obj) && obj is Door door)
            return door.OffsetFromWallStart.Millimeters;
        if (_project.TryGet(_openingId, out obj) && obj is Window window)
            return window.OffsetFromWallStart.Millimeters;
        throw new KeyNotFoundException();
    }

    private void SetOffset(double offsetMm)
    {
        if (_project.TryGet(_openingId, out var obj) && obj is Door door)
        {
            door.SetDimensions(Length.FromMillimeters(offsetMm), door.Width, door.Height);
            return;
        }

        if (_project.TryGet(_openingId, out obj) && obj is Window window)
        {
            window.SetDimensions(Length.FromMillimeters(offsetMm), window.Width, window.Height, window.SillHeight);
            return;
        }

        throw new KeyNotFoundException();
    }
}
