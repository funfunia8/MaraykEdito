using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Products;

namespace DesignStudio.Domain.Project;

public sealed class Project
{
    private readonly Dictionary<EntityId, ProjectObject> _objects = new();
    private readonly Dictionary<EntityId, ProductDefinition> _productDefinitions = new();
    private readonly Dictionary<EntityId, Material> _materials = new();

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

    public IReadOnlyCollection<Material> Materials => _materials.Values;

    public void Add(ProjectObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj is Material)
            throw new InvalidOperationException("Materials must be added through AddMaterial.");

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

    public void AddMaterial(Material material)
    {
        ArgumentNullException.ThrowIfNull(material);

        if (!_materials.TryAdd(material.Id, material))
            throw new InvalidOperationException($"Material {material.Id} already exists.");
    }

    public Material GetMaterial(EntityId id)
        => _materials.TryGetValue(id, out var material)
            ? material
            : throw new KeyNotFoundException($"Material {id} was not found.");

    public bool TryGetMaterial(EntityId id, out Material? material)
        => _materials.TryGetValue(id, out material);

    public bool RemoveMaterial(EntityId id)
        => _materials.Remove(id);

    internal static Project Restore(EntityId id, string name)
        => new(id, name);
}