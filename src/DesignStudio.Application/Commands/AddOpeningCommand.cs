using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Commands;

public sealed class AddOpeningCommand : ICommand
{
    private readonly Project _project;
    private readonly Room _room;
    private readonly ProjectObject _opening;
    private readonly RoomValidationService _validator = new();

    public AddOpeningCommand(Project project, Room room, ProjectObject opening)
    {
        _project = project;
        _room = room;
        _opening = opening;
    }

    public string Name => $"Add {_opening.Type}";

    public void Execute()
    {
        _project.Add(_opening);
        var validation = _validator.Validate(_project, _room);
        if (!validation.IsValid)
        {
            _project.Remove(_opening.Id);
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, validation.Issues.Select(x => x.Message)));
        }
    }

    public void Undo() => _project.Remove(_opening.Id);
}
