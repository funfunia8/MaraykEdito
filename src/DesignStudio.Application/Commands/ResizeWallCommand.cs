using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Commands;

public sealed class ResizeWallCommand : ICommand
{
    private readonly Wall _wall;
    private readonly Point2D _newEnd;
    private Point2D _oldEnd;
    private bool _captured;

    public ResizeWallCommand(Wall wall, Point2D newEnd)
    {
        _wall = wall;
        _newEnd = newEnd;
    }

    public string Name => "Resize Wall";

    public void Execute()
    {
        if (!_captured)
        {
            _oldEnd = _wall.End;
            _captured = true;
        }
        _wall.SetGeometry(_wall.Start, _newEnd);
    }

    public void Undo() => _wall.SetGeometry(_wall.Start, _oldEnd);
}
