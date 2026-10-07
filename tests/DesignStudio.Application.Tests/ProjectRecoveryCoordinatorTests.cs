using DesignStudio.Application.Persistence;
using DesignStudio.Application.Shell;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Tests;

public sealed class ProjectRecoveryCoordinatorTests
{
    [Fact]
    public async Task AutoSave_Skips_Clean_Workspace()
    {
        var workspace = CreateWorkspace();
        workspace.CreateProject("Clean");

        var store = new FakeRecoveryStore();
        var coordinator = CreateCoordinator(workspace, store);

        var saved = await coordinator.AutoSaveAsync();

        Assert.False(saved);
        Assert.False(store.SaveCalled);
    }

    [Fact]
    public async Task AutoSave_Writes_Dirty_Project_And_Leaves_Workspace_Dirty()
    {
        var workspace = CreateWorkspace();
        workspace.CreateProject("Dirty");
        workspace.CreateDefaultRoom();

        var store = new FakeRecoveryStore();
        var coordinator = CreateCoordinator(workspace, store);

        var saved = await coordinator.AutoSaveAsync();

        Assert.True(saved);
        Assert.True(store.SaveCalled);
        Assert.Same(
            workspace.State.ActiveProject,
            store.SavedProject);
        Assert.True(workspace.State.IsDirty);
    }

    [Fact]
    public async Task AutoSave_Does_Not_Clear_Dirty_State_When_Store_Fails()
    {
        var workspace = CreateWorkspace();
        workspace.CreateProject("Dirty");
        workspace.CreateDefaultRoom();

        var store = new FakeRecoveryStore
        {
            SaveException = new IOException("Simulated recovery failure.")
        };

        var coordinator = CreateCoordinator(workspace, store);

        var exception =
            await Assert.ThrowsAsync<IOException>(
                () => coordinator.AutoSaveAsync());

        Assert.Equal(
            "Simulated recovery failure.",
            exception.Message);
        Assert.True(workspace.State.IsDirty);
    }

    [Fact]
    public async Task Recover_Restores_Project_As_Dirty_With_No_FilePath()
    {
        var workspace = CreateWorkspace();

        var recoveryWorkspace = CreateWorkspace();
        recoveryWorkspace.CreateProject("Recovered");
        recoveryWorkspace.CreateDefaultRoom();

        var recoveryProject =
            recoveryWorkspace.State.ActiveProject!;

        var room =
            Assert.Single(
                recoveryProject.Objects.OfType<Room>());

        var store = new FakeRecoveryStore
        {
            HasSnapshot = true,
            LoadedProject = recoveryProject
        };

        var coordinator = CreateCoordinator(workspace, store);

        var recovered = await coordinator.RecoverAsync();

        Assert.True(recovered);
        Assert.Same(
            recoveryProject,
            workspace.State.ActiveProject);
        Assert.True(workspace.State.IsDirty);
        Assert.Null(workspace.State.FilePath);
        Assert.Same(
            room,
            Assert.Single(
                workspace.State.ActiveProject!.Objects.OfType<Room>()));
    }

    [Fact]
    public async Task Recover_Returns_False_When_No_Snapshot_Exists()
    {
        var workspace = CreateWorkspace();
        workspace.CreateProject("Current");

        var store = new FakeRecoveryStore
        {
            HasSnapshot = false
        };

        var coordinator = CreateCoordinator(workspace, store);

        var recovered = await coordinator.RecoverAsync();

        Assert.False(recovered);
        Assert.False(store.LoadCalled);
        Assert.Equal(
            "Current",
            workspace.State.ActiveProject!.Name);
    }

    [Fact]
    public void ClearAfterSuccessfulSaveOrOpen_Deletes_Recovery()
    {
        var workspace = CreateWorkspace();
        workspace.CreateProject("Current");

        var store = new FakeRecoveryStore
        {
            HasSnapshot = true
        };

        var coordinator = CreateCoordinator(workspace, store);

        coordinator.ClearAfterSuccessfulSaveOrOpen();

        Assert.True(store.DeleteCalled);
    }

    [Fact]
    public void ClearRecoverySnapshot_Deletes_Recovery()
    {
        var workspace = CreateWorkspace();
        workspace.CreateProject("Current");

        var store = new FakeRecoveryStore
        {
            HasSnapshot = true
        };

        var coordinator = CreateCoordinator(workspace, store);

        coordinator.ClearRecoverySnapshot();

        Assert.True(store.DeleteCalled);
    }

    private static WorkspaceController CreateWorkspace()
        => new(new WorkspaceState());

    private static ProjectRecoveryCoordinator CreateCoordinator(
        WorkspaceController workspace,
        FakeRecoveryStore store)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "DesignStudioTests",
            Guid.NewGuid().ToString("N"),
            "autosave.design");

        return new ProjectRecoveryCoordinator(
            workspace,
            store,
            path);
    }

    private sealed class FakeRecoveryStore : IProjectRecoveryStore
    {
        public bool HasSnapshot { get; set; }

        public bool SaveCalled { get; private set; }

        public bool LoadCalled { get; private set; }

        public bool DeleteCalled { get; private set; }

        public Project? SavedProject { get; private set; }

        public Project? LoadedProject { get; set; }

        public Exception? SaveException { get; set; }

        public bool Exists(string recoveryPath) =>
            HasSnapshot;

        public DateTimeOffset? GetLastWriteTime(string recoveryPath) =>
            HasSnapshot
                ? DateTimeOffset.UtcNow
                : null;

        public Task SaveAsync(
            Project project,
            string recoveryPath,
            CancellationToken cancellationToken = default)
        {
            SaveCalled = true;

            if (SaveException is not null)
                throw SaveException;

            SavedProject = project;
            HasSnapshot = true;

            return Task.CompletedTask;
        }

        public Task<Project> LoadAsync(
            string recoveryPath,
            CancellationToken cancellationToken = default)
        {
            LoadCalled = true;

            return Task.FromResult(
                LoadedProject
                ?? throw new InvalidOperationException(
                    "No fake recovery project was configured."));
        }

        public void Delete(string recoveryPath)
        {
            DeleteCalled = true;
            HasSnapshot = false;
        }
    }
}