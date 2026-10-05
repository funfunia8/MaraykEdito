using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Commands;

public sealed class MoveCabinetCommand : ICommand
{
    private readonly Project _project;
    private readonly EntityId _cabinetId;
    private readonly Point2D _newPosition;
    private Point2D _oldPosition;

    public MoveCabinetCommand(Project project, EntityId cabinetId, Point2D newPosition)
    {
        _project = project;
        _cabinetId = cabinetId;
        _newPosition = newPosition;
    }

    public string Name => "Move Cabinet";

    public void Execute()
    {
        var cabinet = _project.Get<Cabinet>(_cabinetId);
        _oldPosition = cabinet.Position;
        cabinet.SetPlacement(cabinet.HostRoomId, _newPosition, cabinet.RotationDegrees);
    }

    public void Undo()
    {
        var cabinet = _project.Get<Cabinet>(_cabinetId);
        cabinet.SetPlacement(cabinet.HostRoomId, _oldPosition, cabinet.RotationDegrees);
    }
}
