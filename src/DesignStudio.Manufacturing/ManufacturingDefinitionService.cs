using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Project;

namespace DesignStudio.Manufacturing;

public interface IManufacturingDefinitionService
{
    ManufacturingDefinition Build(
        Project project,
        ProjectObject designObject,
        string code,
        string revision = "A",
        EntityId? productDefinitionId = null);
}

public sealed class ManufacturingDefinitionService : IManufacturingDefinitionService
{
    public ManufacturingDefinition Build(
        Project project,
        ProjectObject designObject,
        string code,
        string revision = "A",
        EntityId? productDefinitionId = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(designObject);

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Manufacturing code is required.", nameof(code));

        if (!project.TryGet(designObject.Id, out var registeredObject) ||
            registeredObject is null)
        {
            throw new InvalidOperationException(
                $"Design object {designObject.Id} does not belong to the supplied project.");
        }

        if (productDefinitionId is not null &&
            !project.TryGetProductDefinition(productDefinitionId.Value, out _))
        {
            throw new InvalidOperationException(
                $"Product definition {productDefinitionId.Value} was not found in the supplied project.");
        }

        return new ManufacturingDefinition(
            EntityId.New(),
            designObject.Id,
            code.Trim(),
            NormalizeRevision(revision),
            Array.Empty<ManufacturingPart>(),
            Array.Empty<ManufacturingHardware>(),
            Array.Empty<ManufacturingOperation>(),
            Array.Empty<ManufacturingAssembly>(),
            productDefinitionId);
    }

    private static string NormalizeRevision(string revision)
        => string.IsNullOrWhiteSpace(revision)
            ? "A"
            : revision.Trim().ToUpperInvariant();
}