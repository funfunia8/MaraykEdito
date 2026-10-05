using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Products;

namespace DesignStudio.Domain.Project;

public sealed class Project
{
    private readonly Dictionary<EntityId, ProjectObject> _objects = new();
    private readonly Dictionary<EntityId, ProductDefinition> _productDefinitions = new();

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

    public IReadOnlyCollection<ProductDefinition> ProductDefinitions => _productDefinitions.Values;

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

    public void AddProductDefinition(ProductDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!_productDefinitions.TryAdd(definition.Id, definition))
            throw new InvalidOperationException($"Product definition {definition.Id} already exists.");
    }

    public ProductDefinition GetProductDefinition(EntityId id)
        => _productDefinitions.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Product definition {id} was not found.");

    public bool TryGetProductDefinition(EntityId id, out ProductDefinition? definition)
        => _productDefinitions.TryGetValue(id, out definition);

    public bool RemoveProductDefinition(EntityId id)
        => _productDefinitions.Remove(id);

    internal static Project Restore(EntityId id, string name)
        => new(id, name);
}
