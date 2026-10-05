using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Project;

public enum CabinetConstructionMethod
{
    ButtJoint,
    CapturedBackGroove
}

public sealed class Cabinet : ProjectObject
{
    public Cabinet(
        string name,
        Length width,
        Length height,
        Length depth,
        Length panelThickness,
        Length backThickness,
        CabinetConstructionMethod constructionMethod = CabinetConstructionMethod.CapturedBackGroove,
        int shelfCount = 2,
        int doorCount = 1,
        EntityId? id = null) : base(id)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Cabinet" : name;
        SetDimensions(width, height, depth);
        SetPanelThickness(panelThickness);
        SetBackThickness(backThickness);
        SetConstructionMethod(constructionMethod);
        SetShelfCount(shelfCount);
        SetDoorCount(doorCount);
        SetCategory(ObjectCategory.Furniture);
    }

    public override string Type => "Cabinet";
    public string Name { get; private set; }
    public Length Width { get; private set; }
    public Length Height { get; private set; }
    public Length Depth { get; private set; }
    public Length PanelThickness { get; private set; }
    public Length BackThickness { get; private set; }
    public CabinetConstructionMethod ConstructionMethod { get; private set; }
    public int ShelfCount { get; private set; }
    public int DoorCount { get; private set; }
    public EntityId? ProductDefinitionId { get; private set; }
    public EntityId? CarcassMaterialId { get; private set; }
    public EntityId? BackMaterialId { get; private set; }
    public EntityId? DoorMaterialId { get; private set; }
    public EntityId? EdgeBandMaterialId { get; private set; }
    public EntityId? HostRoomId { get; private set; }
    public Point2D Position => Placement.Position;
    public double RotationDegrees => Placement.RotationDegrees;

    public void SetDimensions(Length width, Length height, Length depth)
    {
        ValidatePositive(width, nameof(width));
        ValidatePositive(height, nameof(height));
        ValidatePositive(depth, nameof(depth));
        Width = width;
        Height = height;
        Depth = depth;
    }

    public void SetPanelThickness(Length thickness)
    {
        ValidatePositive(thickness, nameof(thickness));
        PanelThickness = thickness;
    }

    public void SetBackThickness(Length thickness)
    {
        ValidatePositive(thickness, nameof(thickness));
        BackThickness = thickness;
    }

    public void SetConstructionMethod(CabinetConstructionMethod method) => ConstructionMethod = method;

    public void SetShelfCount(int count)
    {
        if (count < 0 || count > 32) throw new ArgumentOutOfRangeException(nameof(count));
        ShelfCount = count;
    }

    public void SetProductDefinition(EntityId? productDefinitionId)
        => ProductDefinitionId = productDefinitionId;

    public void SetPlacement(EntityId? hostRoomId, Point2D position, double rotationDegrees = 0)
    {
        if (double.IsNaN(rotationDegrees) || double.IsInfinity(rotationDegrees))
            throw new ArgumentOutOfRangeException(nameof(rotationDegrees));

        HostRoomId = hostRoomId;
        SetParent(hostRoomId);
        SetPlacement(new Placement2D(position, rotationDegrees));
    }

    public void SetMaterialAssignments(
        EntityId? carcassMaterialId,
        EntityId? backMaterialId,
        EntityId? doorMaterialId,
        EntityId? edgeBandMaterialId)
    {
        CarcassMaterialId = carcassMaterialId;
        BackMaterialId = backMaterialId;
        DoorMaterialId = doorMaterialId;
        EdgeBandMaterialId = edgeBandMaterialId;
    }

    public void SetDoorCount(int count)
    {
        if (count < 0 || count > 4) throw new ArgumentOutOfRangeException(nameof(count));
        DoorCount = count;
    }

    private static void ValidatePositive(Length value, string parameterName)
    {
        if (value.Millimeters <= 0) throw new ArgumentOutOfRangeException(parameterName);
    }
}