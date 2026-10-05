using DesignStudio.Domain.Project;

namespace DesignStudio.Storage;

public interface IProjectSerializer
{
    Task SaveAsync(Project project, Stream destination, CancellationToken cancellationToken = default);
    Task<Project> LoadAsync(Stream source, CancellationToken cancellationToken = default);
}
