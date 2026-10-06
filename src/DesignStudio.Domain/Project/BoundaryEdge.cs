using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public readonly record struct BoundaryEdge(
    EntityId WallId,
    bool IsReversed = false);
