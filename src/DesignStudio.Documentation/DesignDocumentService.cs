using DesignStudio.Domain.Documentation;
using DesignStudio.Domain.Identity;

namespace DesignStudio.Documentation;

public interface IDesignDocumentService
{
    DesignDocument Create(EntityId projectId, string number, string title, DesignDocumentType type, EntityId? sourceObjectId = null, string revision = "A", string scale = "1:50", string units = "mm");
}

public sealed class DesignDocumentService : IDesignDocumentService
{
    public DesignDocument Create(EntityId projectId, string number, string title, DesignDocumentType type, EntityId? sourceObjectId = null, string revision = "A", string scale = "1:50", string units = "mm")
    {
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Document number is required.", nameof(number));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Document title is required.", nameof(title));
        return new DesignDocument(EntityId.New(), projectId, number.Trim(), title.Trim(), type,
            string.IsNullOrWhiteSpace(revision) ? "A" : revision.Trim().ToUpperInvariant(), scale.Trim(), units.Trim(), sourceObjectId?.Value.ToString("D"));
    }
}
