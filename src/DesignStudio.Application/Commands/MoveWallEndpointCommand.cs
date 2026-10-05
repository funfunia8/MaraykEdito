using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Identity;

namespace DesignStudio.Application.Commands;

public sealed class MoveWallEndpointCommand : ICommand
{
    private readonly Project _project;
    private readonly Room _room;
    private readonly EntityId _wallId;
    private readonly bool _moveStart;
    private readonly Point2D _newPoint;
    private readonly Dictionary<EntityId, (Point2D Start, Point2D End)> _oldGeometry = new();
    private bool _captured;

    public MoveWallEndpointCommand(Project project, Room room, EntityId wallId, bool moveStart, Point2D newPoint)
    {
        _project = project;
        _room = room;
        _wallId = wallId;
        _moveStart = moveStart;
        _newPoint = newPoint;
    }

    public string Name => "Move Wall Endpoint";

    public void Execute()
    {
        if (!_captured)
        {
            var target = _project.Get<Wall>(_wallId);
            var anchor = _moveStart ? target.Start : target.End;

            foreach (var id in _room.WallIds)
            {
                var wall = _project.Get<Wall>(id);
                if (wall.Start.DistanceTo(anchor) <= 0.001 || wall.End.DistanceTo(anchor) <= 0.001)
                    _oldGeometry[id] = (wall.Start, wall.End);
            }
            _captured = true;
        }

        foreach (var id in _oldGeometry.Keys)
        {
            var wall = _project.Get<Wall>(id);
            var start = wall.Start.DistanceTo(GetAnchor()) <= 0.001 ? _newPoint : wall.Start;
            var end = wall.End.DistanceTo(GetAnchor()) <= 0.001 ? _newPoint : wall.End;
            wall.SetGeometry(start, end);
        }
    }

    private Point2D GetAnchor()
    {
        var old = _oldGeometry[_wallId];
        return _moveStart ? old.Start : old.End;
    }

    public void Undo()
    {
        foreach (var pair in _oldGeometry)
            _project.Get<Wall>(pair.Key).SetGeometry(pair.Value.Start, pair.Value.End);
    }
}
