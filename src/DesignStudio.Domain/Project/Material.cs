using System;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Units;

namespace DesignStudio.Domain.Project;

public enum MaterialKind
{
    /// <summary>
    /// Rigid sheet/board stock used as a panel-like material.
    /// </summary>
    Sheet,

    /// <summary>
    /// Edge-banding stock applied to the edge of a manufactured part.
    /// </summary>
    EdgeBand,

    /// <summary>
    /// Material without a specialized stock form in the current domain model.
    /// </summary>
    Generic
}

public enum MaterialCategory
{
    Panel,

    /// <summary>
    /// Legacy category value retained for backward-compatible persistence.
    /// New code should represent edge-band stock through MaterialKind.EdgeBand.
    /// </summary>
    [Obsolete("Use MaterialKind.EdgeBand for edge-band stock form. This category value is retained for legacy persistence compatibility.")]
    EdgeBand,

    Paint,
    Wallpaper,
    Tile,
    Stone,
    Wood,
    Glass,
    Metal,
    Fabric,
    Flooring,
    Ceiling,
    Generic
}

public enum MaterialGrainDirection
{
    None,
    AlongWidth,
    AlongHeight
}

public sealed class Material : ProjectObject
{
    public Material(
        string code,
        string name,
        MaterialKind kind,
        Length thickness,
        Length sheetWidth,
        Length sheetHeight,
        Length bandWidth,
        MaterialGrainDirection grainDirection = MaterialGrainDirection.None,
        EntityId? id = null) : base(id)
    {
        Code = string.IsNullOrWhiteSpace(code) ? $"MAT-{Id.Value:N}" : code.Trim();
        Name = string.IsNullOrWhiteSpace(name) ? Code : name.Trim();

        SetCategory(ObjectCategory.Finish);
        SetKind(kind);
        SetThickness(thickness);
        SetSheetSize(sheetWidth, sheetHeight);
        SetBandWidth(bandWidth);
        GrainDirection = grainDirection;
    }

    public override string Type => "Material";

    public string Code { get; private set; }

    public string Name { get; private set; }

    /// <summary>
    /// Describes the material's stock/manufacturing form.
    /// This is intentionally separate from MaterialCategory.
    /// </summary>
    public MaterialKind Kind { get; private set; }

    public Length Thickness { get; private set; }

    /// <summary>
    /// Width of sheet-like stock when applicable.
    /// </summary>
    public Length SheetWidth { get; private set; }

    /// <summary>
    /// Height/length of sheet-like stock when applicable.
    /// </summary>
    public Length SheetHeight { get; private set; }

    /// <summary>
    /// Width of edge-band stock when applicable.
    /// </summary>
    public Length BandWidth { get; private set; }

    public MaterialGrainDirection GrainDirection { get; private set; }

    /// <summary>
    /// Semantic/catalog classification of the material.
    /// This must not be used as a substitute for the stock form.
    /// </summary>
    public MaterialCategory MaterialCategory { get; private set; } = MaterialCategory.Generic;

    public string? Manufacturer { get; private set; }

    public string? Supplier { get; private set; }

    public string? Color { get; private set; }

    public string? Finish { get; private set; }

    public string Unit { get; private set; } = "piece";

    /// <summary>
    /// Indicates that sheet stock dimensions are meaningful for this material.
    /// </summary>
    public bool UsesSheetStockDimensions => Kind == MaterialKind.Sheet;

    /// <summary>
    /// Indicates that edge-band stock width is meaningful for this material.
    /// </summary>
    public bool UsesEdgeBandStockWidth => Kind == MaterialKind.EdgeBand;

    private void SetKind(MaterialKind kind) => Kind = kind;

    private void SetThickness(Length thickness)
    {
        if (thickness.Millimeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(thickness));

        Thickness = thickness;
    }

    public void SetSheetSize(Length width, Length height)
    {
        if (width.Millimeters < 0 || height.Millimeters < 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        SheetWidth = width;
        SheetHeight = height;
    }

    public void SetBandWidth(Length width)
    {
        if (width.Millimeters < 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        BandWidth = width;
    }

    public void SetGrainDirection(MaterialGrainDirection direction) => GrainDirection = direction;

    public void SetCatalogMetadata(
        MaterialCategory category,
        string? manufacturer = null,
        string? supplier = null,
        string? color = null,
        string? finish = null,
        string unit = "piece")
    {
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Material unit is required.", nameof(unit));

        MaterialCategory = category;
        Manufacturer = string.IsNullOrWhiteSpace(manufacturer) ? null : manufacturer.Trim();
        Supplier = string.IsNullOrWhiteSpace(supplier) ? null : supplier.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        Finish = string.IsNullOrWhiteSpace(finish) ? null : finish.Trim();
        Unit = unit.Trim();
    }
}