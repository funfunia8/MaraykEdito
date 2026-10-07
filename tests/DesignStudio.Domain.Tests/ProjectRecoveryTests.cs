using System.Text;
using DesignStudio.Domain.Project;
using DesignStudio.Storage;
using DesignStudio.Storage.Recovery;


namespace DesignStudio.Domain.Tests;

public sealed class ProjectRecoveryTests
{
    [Fact]
    public async Task RecoveryStore_RoundTripsProject()
    {
        var root =
            Path.Combine(
                Path.GetTempPath(),
                "DesignStudioTests",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        var path =
            Path.Combine(
                root,
                "autosave.design");

        try
        {
            var project =
                new global::DesignStudio.Domain.Project.Project("Recovery Test");

            var room =
                new Room("Room");

            project.Add(room);

            var store =
                new ProjectRecoveryStore(
                    new JsonProjectSerializer());

            await store.SaveAsync(
                project,
                path);

            Assert.True(
                store.Exists(path));

            Assert.NotNull(
                store.GetLastWriteTime(path));

            var restored =
                await store.LoadAsync(path);

            Assert.Equal(
                project.Id,
                restored.Id);

            Assert.Equal(
                project.Name,
                restored.Name);

            Assert.Contains(
                restored.Objects,
                x => x.Id == room.Id);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(
                    root,
                    recursive: true);
        }
    }

    [Fact]
    public async Task RecoveryStore_Save_Is_Atomic_When_Serialization_Fails()
    {
        var root =
            Path.Combine(
                Path.GetTempPath(),
                "DesignStudioTests",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        var path =
            Path.Combine(
                root,
                "autosave.design");

        await File.WriteAllTextAsync(
            path,
            "previous-recovery-content");

        try
        {
            var store =
                new ProjectRecoveryStore(
                    new FailingProjectSerializer());

            var project =
                new global::DesignStudio.Domain.Project.Project("Failed Recovery");

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => store.SaveAsync(project, path));

            Assert.Equal(
                "previous-recovery-content",
                await File.ReadAllTextAsync(path));

            var temporaryFiles =
                Directory.GetFiles(
                    root,
                    ".autosave.design.*.tmp");

            Assert.Empty(
                temporaryFiles);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(
                    root,
                    recursive: true);
        }
    }

    private sealed class FailingProjectSerializer : IProjectSerializer
    {
        public async Task SaveAsync(
            global::DesignStudio.Domain.Project.Project project,
            Stream destination,
            CancellationToken cancellationToken = default)
        {
            var bytes =
                Encoding.UTF8.GetBytes(
                    "partial-recovery-content");

            await destination.WriteAsync(
                bytes,
                cancellationToken);

            throw new InvalidOperationException(
                "Simulated serialization failure.");
        }

        public Task<global::DesignStudio.Domain.Project.Project> LoadAsync(
            Stream source,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
