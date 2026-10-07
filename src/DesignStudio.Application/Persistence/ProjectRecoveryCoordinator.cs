using DesignStudio.Application.Shell;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Persistence;

public sealed class ProjectRecoveryCoordinator
{
    private readonly WorkspaceController _workspace;
    private readonly IProjectRecoveryStore _recoveryStore;

    public ProjectRecoveryCoordinator(
        WorkspaceController workspace,
        IProjectRecoveryStore recoveryStore,
        string recoveryPath)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(recoveryStore);

        if (string.IsNullOrWhiteSpace(recoveryPath))
            throw new ArgumentException(
                "A recovery path is required.",
                nameof(recoveryPath));

        _workspace = workspace;
        _recoveryStore = recoveryStore;
        RecoveryPath = Path.GetFullPath(recoveryPath);
    }

    public string RecoveryPath { get; }

    public bool HasRecoverySnapshot =>
        _recoveryStore.Exists(RecoveryPath);

    public DateTimeOffset? RecoverySnapshotTime =>
        _recoveryStore.GetLastWriteTime(RecoveryPath);

    public async Task<bool> AutoSaveAsync(
        CancellationToken cancellationToken = default)
    {
        var project = _workspace.State.ActiveProject;

        if (project is null || !_workspace.State.IsDirty)
            return false;

        await _recoveryStore.SaveAsync(
            project,
            RecoveryPath,
            cancellationToken).ConfigureAwait(false);

        return true;
    }

    public async Task<bool> RecoverAsync(
        CancellationToken cancellationToken = default)
    {
        if (!HasRecoverySnapshot)
            return false;

        var project = await _recoveryStore.LoadAsync(
            RecoveryPath,
            cancellationToken).ConfigureAwait(false);

        _workspace.OpenProject(project);
        _workspace.State.SetFilePath(null);
        _workspace.State.MarkDirty();

        return true;
    }

    public void ClearRecoverySnapshot()
    {
        _recoveryStore.Delete(RecoveryPath);
    }

    public void ClearAfterSuccessfulSaveOrOpen()
    {
        ClearRecoverySnapshot();
    }
}
