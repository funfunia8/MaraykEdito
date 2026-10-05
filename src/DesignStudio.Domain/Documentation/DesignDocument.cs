using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Documentation;

public enum DesignDocumentType
{
    FloorPlan,
    FurniturePlan,
    CeilingPlan,
    LightingPlan,
    FinishPlan,
    Elevation,
    Section,
    Detail,
    ShopDrawing,
    Schedule,
    Presentation
}

public sealed record DesignDocument(
    EntityId Id,
    EntityId ProjectId,
    string Number,
    string Title,
    DesignDocumentType Type,
    string Revision,
    string Scale,
    string Units,
    string? SourceObjectId = null,
    string? TemplateCode = null,
    IReadOnlyList<string>? Notes = null);
