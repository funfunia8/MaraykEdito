namespace DesignStudio.Application.Commands;

public sealed class CommandHistory
{
    private readonly Stack<ICommand> _undo = new();
    private readonly Stack<ICommand> _redo = new();

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public void Execute(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        command.Execute();
        _undo.Push(command);
        _redo.Clear();
    }

    public void Undo()
    {
        if (_undo.Count == 0) return;
        var command = _undo.Pop();
        command.Undo();
        _redo.Push(command);
    }

    public void Redo()
    {
        if (_redo.Count == 0) return;
        var command = _redo.Pop();
        command.Execute();
        _undo.Push(command);
    }

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }
}
