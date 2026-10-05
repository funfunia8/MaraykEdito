using System.Text.Json;
using DesignStudio.Domain.Project;

namespace DesignStudio.Storage;

public sealed class ProjectDocumentV1
{
    public int SchemaVersion { get; set; } = 1;
    public string ProjectId { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ProjectObjectRecordV1> Objects { get; set; } = new();
}

public sealed class ProjectObjectRecordV1
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public JsonElement Data { get; set; }
}
