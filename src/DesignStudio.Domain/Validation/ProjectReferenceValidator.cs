using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using ProjectModel = DesignStudio.Domain.Project.Project;

namespace DesignStudio.Domain.Validation;

public sealed class ProjectReferenceValidator
{
    public ValidationResult Validate(ProjectModel project)
    {
        ArgumentNullException.ThrowIfNull(project);

        var result = new ValidationResult();

        foreach (var obj in project.Objects)
        {
            ValidateParent(project, obj, result);
            ValidateObjectReferences(project, obj, result);
        }

        ValidateProductDefinitionReferences(project, result);

        return result;
    }

    private static void ValidateParent(
        ProjectModel project,
        ProjectObject obj,
        ValidationResult result)
    {
        if (obj.ParentId is null)
            return;

        var parentId = obj.ParentId.Value;

        if (!project.TryGet(parentId, out var parent) || parent is null)
        {
            result.Error(
                "PROJECT-REF-001",
                $"{obj.Type} {obj.Id} references missing parent {parentId}.");

            return;
        }

        var expectedParentType = obj switch
        {
            Floor => typeof(Building),
            Room => typeof(Floor),
            Door => typeof(Wall),
            Window => typeof(Wall),
            Cabinet => typeof(Room),
            _ => null
        };

        if (expectedParentType is not null &&
            !expectedParentType.IsInstanceOfType(parent))
        {
            result.Error(
                "PROJECT-REF-002",
                $"{obj.Type} {obj.Id} must reference a {expectedParentType.Name}, " +
                $"but references {parent.Type} {parent.Id}.");
        }

        if (HasParentCycle(project, obj.Id))
        {
            result.Error(
                "PROJECT-REF-003",
                $"{obj.Type} {obj.Id} participates in a parent relationship cycle.");
        }
    }

    private static void ValidateObjectReferences(
        ProjectModel project,
        ProjectObject obj,
        ValidationResult result)
    {
        if (obj is Room room)
        {
            foreach (var wallId in room.WallIds)
            {
                if (!project.TryGet(wallId, out var wall) || wall is null)
                {
                    result.Error(
                        "PROJECT-REF-004",
                        $"Room {room.Id} references missing wall {wallId}.");

                    continue;
                }

                if (wall is not Wall)
                {
                    result.Error(
                        "PROJECT-REF-005",
                        $"Room {room.Id} wall reference {wallId} points to {wall.Type}.");
                }
            }
        }

        if (obj is Cabinet cabinet)
        {
            ValidateOptionalProductDefinition(
                project,
                cabinet.ProductDefinitionId,
                $"Cabinet {cabinet.Id}",
                result);

            ValidateOptionalMaterial(
                project,
                cabinet.CarcassMaterialId,
                "CarcassMaterialId",
                $"Cabinet {cabinet.Id}",
                result);

            ValidateOptionalMaterial(
                project,
                cabinet.BackMaterialId,
                "BackMaterialId",
                $"Cabinet {cabinet.Id}",
                result);

            ValidateOptionalMaterial(
                project,
                cabinet.DoorMaterialId,
                "DoorMaterialId",
                $"Cabinet {cabinet.Id}",
                result);

            ValidateOptionalMaterial(
                project,
                cabinet.EdgeBandMaterialId,
                "EdgeBandMaterialId",
                $"Cabinet {cabinet.Id}",
                result);
        }
    }

    private static void ValidateOptionalProductDefinition(
        ProjectModel project,
        EntityId? definitionId,
        string source,
        ValidationResult result)
    {
        if (definitionId is null)
            return;

        if (!project.TryGetProductDefinition(definitionId.Value, out _))
        {
            result.Error(
                "PROJECT-REF-006",
                $"{source} references missing product definition {definitionId.Value}.");
        }
    }

    private static void ValidateOptionalMaterial(
        ProjectModel project,
        EntityId? materialId,
        string propertyName,
        string source,
        ValidationResult result)
    {
        if (materialId is null)
            return;

        if (!project.TryGetMaterial(materialId.Value, out _))
        {
            result.Error(
                "PROJECT-REF-007",
                $"{source}.{propertyName} references missing material {materialId.Value}.");
        }
    }

    private static void ValidateProductDefinitionReferences(
        ProjectModel project,
        ValidationResult result)
    {
        foreach (var definition in project.ProductDefinitions)
        {
            if (definition.DesignObjectId is null)
                continue;

            if (!project.TryGet(definition.DesignObjectId.Value, out _))
            {
                result.Error(
                    "PROJECT-REF-008",
                    $"Product definition {definition.Id} references missing design object {definition.DesignObjectId.Value}.");
            }
        }
    }

    private static bool HasParentCycle(
        ProjectModel project,
        EntityId startId)
    {
        var visited = new HashSet<EntityId>();
        var currentId = startId;

        while (project.TryGet(currentId, out var current) && current is not null)
        {
            if (!visited.Add(current.Id))
                return true;

            if (current.ParentId is null)
                return false;

            currentId = current.ParentId.Value;
        }

        return false;
    }
}