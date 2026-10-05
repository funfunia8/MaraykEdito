using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class MaterialDomainContractTests
{
    [Fact]
    public void MaterialKind_IsIndependentFromMaterialCategory()
    {
        var material = new Material(
            "MDF-18",
            "MDF 18mm",
            MaterialKind.Sheet,
            new Length(18),
            new Length(1220),
            new Length(2440),
            new Length(0));

        Assert.Equal(MaterialKind.Sheet, material.Kind);
        Assert.Equal(MaterialCategory.Generic, material.MaterialCategory);

        material.SetCatalogMetadata(MaterialCategory.Wood);

        Assert.Equal(MaterialKind.Sheet, material.Kind);
        Assert.Equal(MaterialCategory.Wood, material.MaterialCategory);
    }

    [Fact]
    public void SheetMaterial_UsesSheetStockDimensions()
    {
        var material = new Material(
            "MDF-18",
            "MDF 18mm",
            MaterialKind.Sheet,
            new Length(18),
            new Length(1220),
            new Length(2440),
            new Length(0));

        Assert.True(material.UsesSheetStockDimensions);
        Assert.False(material.UsesEdgeBandStockWidth);
    }

    [Fact]
    public void EdgeBandMaterial_UsesEdgeBandStockWidth()
    {
        var material = new Material(
            "EB-22",
            "Edge Band 22mm",
            MaterialKind.EdgeBand,
            new Length(1),
            new Length(0),
            new Length(0),
            new Length(22));

        Assert.False(material.UsesSheetStockDimensions);
        Assert.True(material.UsesEdgeBandStockWidth);
    }

    [Fact]
    public void GenericMaterial_DoesNotUseSpecializedStockDimensions()
    {
        var material = new Material(
            "PAINT-W01",
            "Wall Paint",
            MaterialKind.Generic,
            new Length(1),
            new Length(0),
            new Length(0),
            new Length(0));

        Assert.Equal(MaterialKind.Generic, material.Kind);
        Assert.False(material.UsesSheetStockDimensions);
        Assert.False(material.UsesEdgeBandStockWidth);
    }

    [Fact]
    public void MaterialCategory_CanDescribeSemanticCategoryIndependentlyOfStockForm()
    {
        var material = new Material(
            "MDF-W01",
            "Wood Panel",
            MaterialKind.Sheet,
            new Length(18),
            new Length(1220),
            new Length(2440),
            new Length(0));

        material.SetCatalogMetadata(MaterialCategory.Wood);

        Assert.Equal(MaterialKind.Sheet, material.Kind);
        Assert.Equal(MaterialCategory.Wood, material.MaterialCategory);
        Assert.True(material.UsesSheetStockDimensions);
    }

    [Fact]
    public void LegacyEdgeBandCategoryValue_RemainsAvailableForCompatibility()
    {
#pragma warning disable CS0618
        var material = new Material(
            "LEGACY-EB",
            "Legacy Edge Band",
            MaterialKind.EdgeBand,
            new Length(1),
            new Length(0),
            new Length(0),
            new Length(22));

        material.SetCatalogMetadata(MaterialCategory.EdgeBand);
#pragma warning restore CS0618

        Assert.Equal(MaterialKind.EdgeBand, material.Kind);

#pragma warning disable CS0618
        Assert.Equal(MaterialCategory.EdgeBand, material.MaterialCategory);
#pragma warning restore CS0618
    }
}