using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;

namespace DesignStudio.Manufacturing;

public interface IManufacturingDefinitionService
{
    ManufacturingDefinition Build(Project project, ProjectObject designObject, string code, string revision = "A");
}

public sealed class ManufacturingDefinitionService : IManufacturingDefinitionService
{
    public ManufacturingDefinition Build(Project project, ProjectObject designObject, string code, string revision = "A")
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(designObject);
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Manufacturing code is required.", nameof(code));

        return new ManufacturingDefinition(
            DesignStudio.Domain.Identity.EntityId.New(),
            designObject.Id,
            code.Trim(),
            string.IsNullOrWhiteSpace(revision) ? "A" : revision.Trim().ToUpperInvariant(),
            Array.Empty<ManufacturingPart>(),
            Array.Empty<ManufacturingHardware>(),
            Array.Empty<ManufacturingOperation>(),
            Array.Empty<ManufacturingAssembly>());
    }
}
