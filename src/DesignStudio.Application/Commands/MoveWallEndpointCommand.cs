using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Commands;

public sealed class MoveWallEndpointCommand : ICommand
{
    private const double GeometryToleranceMm = 0.001;

    private readonly Project _project;
    private readonly Room _room;
    private readonly EntityId _wallId;
    private readonly bool _moveStart;
    private readonly Point2D _newPoint;
    private readonly Dictionary<EntityId, (Point2D Start, Point2D End)> _oldGeometry = new();

    private bool _captured;

    public MoveWallEndpointCommand(
        Project project,
        Room room,
        EntityId wallId,
        bool moveStart,
        Point2D newPoint)
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
            CaptureOriginalGeometry();

        var anchor = GetAnchor();

        var newGeometry = new Dictionary<EntityId, (Point2D Start, Point2D End)>();

        foreach (var id in _oldGeometry.Keys)
        {
            var wall = _project.Get<Wall>(id);

            var start = wall.Start.DistanceTo(anchor) <= GeometryToleranceMm
                ? _newPoint
                : wall.Start;

            var end = wall.End.DistanceTo(anchor) <= GeometryToleranceMm
                ? _newPoint
                : wall.End;

            if (start.DistanceTo(end) <= GeometryToleranceMm)
            {
                throw new ArgumentException(
                    $"Moving wall endpoint would collapse wall {wall.Id}.");
            }

            newGeometry[id] = (start, end);
        }

        foreach (var pair in newGeometry)
        {
            _project
                .Get<Wall>(pair.Key)
                .SetGeometry(pair.Value.Start, pair.Value.End);
        }
    }

    public void Undo()
    {
        foreach (var pair in _oldGeometry)
        {
            _project
                .Get<Wall>(pair.Key)
                .SetGeometry(pair.Value.Start, pair.Value.End);
        }
    }

    private void CaptureOriginalGeometry()
    {
        var target = _project.Get<Wall>(_wallId);
        var anchor = _moveStart ? target.Start : target.End;

        foreach (var id in _room.WallIds)
        {
            var wall = _project.Get<Wall>(id);

            if (wall.Start.DistanceTo(anchor) <= GeometryToleranceMm ||
                wall.End.DistanceTo(anchor) <= GeometryToleranceMm)
            {
                _oldGeometry[id] = (wall.Start, wall.End);
            }
        }

        if (!_oldGeometry.ContainsKey(_wallId))
        {
            throw new InvalidOperationException(
                $"Wall {_wallId} is not part of the active room geometry.");
        }

        _captured = true;
    }

    private Point2D GetAnchor()
    {
        var old = _oldGeometry[_wallId];
        return _moveStart ? old.Start : old.End;
    }
}
