using System.Text.Json;

namespace DesignStudio.Storage;

public sealed class ProjectDocumentV3
{
    public int SchemaVersion { get; set; } = 3;
    public string ProjectId { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ProductDefinitionRecordV3> ProductDefinitions { get; set; } = new();
    public List<ProjectObjectRecordV3> Objects { get; set; } = new();
}

public sealed class ProductDefinitionRecordV3
{
    public string Id { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
}

public sealed class ProjectObjectRecordV3
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public JsonElement Data { get; set; }
    public DesignObjectMetadataData? Metadata { get; set; }
}
