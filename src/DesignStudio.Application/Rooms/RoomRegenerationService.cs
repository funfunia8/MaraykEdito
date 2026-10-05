using DesignStudio.Application.Products;
using DesignStudio.Domain.Project;
using DesignStudio.Geometry.Derived;

namespace DesignStudio.Application.Rooms;

public sealed class RoomRegenerationService
{
    private readonly RoomValidationService _validation = new();
    private readonly RoomProjectionService _projection = new();
    private readonly RoomSceneBuilder _scene = new();
    private readonly CabinetPlacementValidationService _cabinetPlacement = new();

    public RoomDerivedState Regenerate(Project project, Room room)
    {
        var validation = _validation.Validate(project, room);
        var cabinetValidation = _cabinetPlacement.Validate(project, room);
        foreach (var issue in cabinetValidation.Issues)
            validation.Add(issue.Severity, issue.Code, issue.Message);

        if (!validation.IsValid)
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, validation.Issues.Select(x => x.Message)));

        var plan2D = _projection.Build(project, room);
        var scene3D = _scene.Build(project, room);

        return new RoomDerivedState
        {
            Validation = validation,
            Plan2D = plan2D,
            Scene3D = scene3D
        };
    }
}

public sealed class RoomDerivedState
{
    public required DesignStudio.Domain.Validation.ValidationResult Validation { get; init; }
    public required RoomPlan2D Plan2D { get; init; }
    public required RoomScene3D Scene3D { get; init; }
}
