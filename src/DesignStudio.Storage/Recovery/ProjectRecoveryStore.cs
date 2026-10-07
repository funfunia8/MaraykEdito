using DesignStudio.Application.Persistence;
using DesignStudio.Domain.Project;

namespace DesignStudio.Storage.Recovery;

public sealed class ProjectRecoveryStore : IProjectRecoveryStore
{
    private readonly IProjectSerializer _serializer;

    public ProjectRecoveryStore(IProjectSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        _serializer = serializer;
    }

    public bool Exists(string recoveryPath) =>
        File.Exists(recoveryPath);

    public DateTimeOffset? GetLastWriteTime(string recoveryPath) =>
        File.Exists(recoveryPath)
            ? File.GetLastWriteTimeUtc(recoveryPath)
            : null;

    public async Task SaveAsync(
        Project project,
        string recoveryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        var fullPath = Path.GetFullPath(recoveryPath);

        var directory =
            Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException(
                "Recovery path must include a directory.");

        Directory.CreateDirectory(directory);

        var tempPath =
            Path.Combine(
                directory,
                $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                64 * 1024,
                FileOptions.SequentialScan))
            {
                await _serializer.SaveAsync(
                    project,
                    stream,
                    cancellationToken).ConfigureAwait(false);

                await stream.FlushAsync(
                    cancellationToken).ConfigureAwait(false);
            }

            File.Move(
                tempPath,
                fullPath,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public async Task<Project> LoadAsync(
        string recoveryPath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(
            recoveryPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.SequentialScan);

        return await _serializer.LoadAsync(
            stream,
            cancellationToken).ConfigureAwait(false);
    }

    public void Delete(string recoveryPath)
    {
        if (File.Exists(recoveryPath))
            File.Delete(recoveryPath);
    }
}
