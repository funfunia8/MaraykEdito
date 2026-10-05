using DesignStudio.Domain.Project;
using DesignStudio.Storage;
using DesignStudio.Storage.Recovery;

namespace DesignStudio.Domain.Tests;

public sealed class ProjectRecoveryTests
{
    [Fact]
    public async Task RecoveryStore_RoundTripsProject()
    {
        var root = Path.Combine(Path.GetTempPath(), "DesignStudioTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "autosave.design");
        try
        {
            var project = new ProjectModel("Recovery Test");
            var room = new Room("Room");
            project.Add(room);

            var store = new ProjectRecoveryStore(new JsonProjectSerializer());
            await store.SaveAsync(project, path);

            Assert.True(store.Exists(path));
            Assert.NotNull(store.GetLastWriteTime(path));

            var restored = await store.LoadAsync(path);
            Assert.Equal(project.Id, restored.Id);
            Assert.Equal(project.Name, restored.Name);
            Assert.Contains(restored.Objects, x => x.Id == room.Id);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
