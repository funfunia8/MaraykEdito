using DesignStudio.Domain.Project;

namespace DesignStudio.Storage.Recovery;

public interface IProjectRecoveryStore
{
    bool Exists(string recoveryPath);
    DateTimeOffset? GetLastWriteTime(string recoveryPath);
    Task SaveAsync(Project project, string recoveryPath, CancellationToken cancellationToken = default);
    Task<Project> LoadAsync(string recoveryPath, CancellationToken cancellationToken = default);
    void Delete(string recoveryPath);
}
