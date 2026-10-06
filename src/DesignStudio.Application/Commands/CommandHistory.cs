namespace DesignStudio.Application.Commands;

public sealed class CommandHistory
{
    private readonly Stack<ICommand> _undo = new();
    private readonly Stack<ICommand> _redo = new();

    private IReadOnlyList<ICommand>? _redoBeforeLastExecution;
    private bool _canRollbackLastExecution;

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public void Execute(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        _redoBeforeLastExecution = null;
        _canRollbackLastExecution = false;

        command.Execute();

        _redoBeforeLastExecution = _redo.ToArray();

        _undo.Push(command);
        _redo.Clear();

        _canRollbackLastExecution = true;
    }

    public void RollbackLastExecution()
    {
        if (!_canRollbackLastExecution || _undo.Count == 0)
            return;

        var command = _undo.Pop();

        command.Undo();

        _redo.Clear();

        if (_redoBeforeLastExecution is not null)
        {
            for (var index = _redoBeforeLastExecution.Count - 1; index >= 0; index--)
                _redo.Push(_redoBeforeLastExecution[index]);
        }

        _redoBeforeLastExecution = null;
        _canRollbackLastExecution = false;
    }

    public void Undo()
    {
        if (_undo.Count == 0)
            return;

        var command = _undo.Peek();

        command.Undo();

        _undo.Pop();
        _redo.Push(command);

        _redoBeforeLastExecution = null;
        _canRollbackLastExecution = false;
    }

    public void Redo()
    {
        if (_redo.Count == 0)
            return;

        var command = _redo.Peek();

        command.Execute();

        _redo.Pop();
        _undo.Push(command);

        _redoBeforeLastExecution = null;
        _canRollbackLastExecution = false;
    }

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();

        _redoBeforeLastExecution = null;
        _canRollbackLastExecution = false;
    }
}
