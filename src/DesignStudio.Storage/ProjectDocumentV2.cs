using System.Text.Json;

namespace DesignStudio.Storage;

public sealed class ProjectDocumentV2
{
    public int SchemaVersion { get; set; } = 2;
    public string ProjectId { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ProductDefinitionRecordV2> ProductDefinitions { get; set; } = new();
    public List<ProjectObjectRecordV2> Objects { get; set; } = new();
}

public sealed class ProductDefinitionRecordV2
{
    public string Id { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
}

public sealed class ProjectObjectRecordV2
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public JsonElement Data { get; set; }
    public DesignObjectMetadataData? Metadata { get; set; }
}

public sealed class DesignObjectMetadataData
{
    public string Category { get; set; } = "Reference";
    public string? ParentId { get; set; }
    public string? LayerId { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double RotationDegrees { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; }
}