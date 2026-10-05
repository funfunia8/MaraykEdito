using DesignStudio.Domain.Identity;

namespace DesignStudio.Domain.Project;

public enum HardwareKind
{
    Hinge,
    Handle,
    ShelfPin,
    Screw,
    Connector,
    Other
}

public enum HardwareUnit
{
    Piece,
    Pair,
    Set
}

public sealed class HardwareItem : ProjectObject
{
    public HardwareItem(
        string code,
        string name,
        HardwareKind kind,
        HardwareUnit unit = HardwareUnit.Piece,
        EntityId? id = null) : base(id)
    {
        Code = string.IsNullOrWhiteSpace(code) ? $"HW-{Id.Value:N}" : code.Trim();
        Name = string.IsNullOrWhiteSpace(name) ? Code : name.Trim();
        SetCategory(ObjectCategory.Product);
        Kind = kind;
        Unit = unit;
    }

    public override string Type => "HardwareItem";
    public string Code { get; private set; }
    public string Name { get; private set; }
    public HardwareKind Kind { get; private set; }
    public HardwareUnit Unit { get; private set; }
}
