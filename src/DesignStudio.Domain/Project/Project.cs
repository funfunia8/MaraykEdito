using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public sealed class Project
{
    private readonly Dictionary<EntityId, ProjectObject> _objects = new();

    public Project(string name)
    {
        Id = EntityId.New();
        Name = string.IsNullOrWhiteSpace(name) ? "Untitled Project" : name;
    }

    private Project(EntityId id, string name)
    {
        Id = id;
        Name = name;
    }

    public EntityId Id { get; }
    public string Name { get; private set; }
    public IReadOnlyCollection<ProjectObject> Objects => _objects.Values;

    public void Add(ProjectObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        if (!_objects.TryAdd(obj.Id, obj))
            throw new InvalidOperationException($"Object {obj.Id} already exists.");
    }

    public T Get<T>(EntityId id) where T : ProjectObject
        => _objects.TryGetValue(id, out var obj) && obj is T typed
            ? typed
            : throw new KeyNotFoundException($"Object {id} was not found as {typeof(T).Name}.");

    public bool TryGet(EntityId id, out ProjectObject? obj)
        => _objects.TryGetValue(id, out obj);

    public void Remove(EntityId id) => _objects.Remove(id);

    internal static Project Restore(EntityId id, string name)
        => new(id, name);
}
