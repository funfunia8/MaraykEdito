using System.Globalization;
using DesignStudio.Application.Products;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Manufacturing;
using DesignStudio.Domain.Products;
using DesignStudio.Domain.Project;
using Xunit;

namespace DesignStudio.Domain.Tests;

public sealed class CabinetManufacturingDefinitionTests
{
    [Fact]
    public void Build_MapsCabinetPartsIntoGenericManufacturingParts()
    {
        var project = new ProjectModel("Cabinet Manufacturing");

        var created = new CabinetService().Create(
            project,
            shelfCount: 2,
            doorCount: 1);

        var definition =
            new CabinetManufacturingDefinitionService().Build(
                project,
                created.Cabinet,
                created.Generation,
                "CAB-MFG-001");

        Assert.Equal(
            created.Cabinet.Id,
            definition.SourceDesignObjectId);

        Assert.Null(definition.ProductDefinitionId);
        Assert.Equal("CAB-MFG-001", definition.Code);
        Assert.Equal("A", definition.Revision);

        Assert.Equal(
            created.Generation.Parts.Count,
            definition.Parts.Count);

        var leftSide = Assert.Single(
            definition.Parts,
            x => x.Code == "SIDE-L");

        Assert.Equal("Left Side", leftSide.Name);
        Assert.Equal(
            720,
            leftSide.Height.Millimeters);

        Assert.Equal(
            560,
            leftSide.Depth?.Millimeters);

        Assert.Equal(
            "LeftSide",
            leftSide.SourceComponent);
    }

    [Fact]
    public void Build_MapsMaterialAndEdgeBandReferences()
    {
        var project = new ProjectModel(
            "Cabinet Manufacturing Mapping");

        var created = new CabinetService().Create(
            project,
            shelfCount: 1,
            doorCount: 1);

        var definition =
            new CabinetManufacturingDefinitionService().Build(
                project,
                created.Cabinet,
                created.Generation,
                "CAB-MFG-002");

        var shelf = Assert.Single(
            definition.Parts,
            x => x.Code == "SHELF");

        Assert.Equal(
            created.Cabinet.CarcassMaterialId,
            shelf.MaterialId);

        Assert.Equal(
            created.Cabinet.EdgeBandMaterialId,
            shelf.EdgeBandMaterialId);

        Assert.Equal(
            ManufacturingEdgeFlags.Front,
            shelf.EdgeBanding);

        Assert.True(shelf.GrainAlongWidth);
    }

    [Fact]
    public void Build_CarriesProductDefinitionReference()
    {
        var project = new ProjectModel(
            "Cabinet Product Manufacturing");

        var created = new CabinetService().Create(project);

        var productDefinition = new ProductDefinition(
            EntityId.New(),
            "CAB-BASE",
            "Base Cabinet",
            ProductCategory.Kitchen);

        project.AddProductDefinition(productDefinition);
        created.Cabinet.SetProductDefinition(
            productDefinition.Id);

        var definition =
            new CabinetManufacturingDefinitionService().Build(
                project,
                created.Cabinet,
                created.Generation,
                "CAB-MFG-003",
                "b");

        Assert.Equal(
            created.Cabinet.Id,
            definition.SourceDesignObjectId);

        Assert.Equal(
            productDefinition.Id,
            definition.ProductDefinitionId);

        Assert.Equal("B", definition.Revision);
    }

    [Fact]
    public void Build_MapsHardwareIntoGenericManufacturingHardware()
    {
        var project = new ProjectModel(
            "Cabinet Hardware Manufacturing");

        var created = new CabinetService().Create(
            project,
            shelfCount: 2,
            doorCount: 1);

        var definition =
            new CabinetManufacturingDefinitionService().Build(
                project,
                created.Cabinet,
                created.Generation,
                "CAB-MFG-004");

        Assert.NotEmpty(definition.Hardware);

        var hinge = Assert.Single(
            definition.Hardware,
            x => x.Code == "HINGE-110");

        Assert.Equal(2, hinge.Quantity);
    }

    [Fact]
    public void Build_CreatesGenericGrooveOperationTargetingBackPart()
    {
        var project = new ProjectModel(
            "Cabinet Groove Manufacturing");

        var created = new CabinetService().Create(
            project,
            backThicknessMm: 8);

        var definition =
            new CabinetManufacturingDefinitionService().Build(
                project,
                created.Cabinet,
                created.Generation,
                "CAB-MFG-005");

        var operation = Assert.Single(
            definition.Operations,
            x => x.Code == "BACK-GROOVE");

        Assert.Equal(
            "Groove",
            operation.OperationType);

        Assert.Contains(
            "BACK",
            operation.PartCodes);

        Assert.NotNull(operation.Parameters);

        Assert.Equal(
            created.Generation.ManufacturingSpec.GrooveWidth.Millimeters
                .ToString("0.###", CultureInfo.InvariantCulture),
            operation.Parameters!["widthMm"]);

        Assert.Equal(
            created.Generation.ManufacturingSpec.GrooveDepth.Millimeters
                .ToString("0.###", CultureInfo.InvariantCulture),
            operation.Parameters["depthMm"]);

        Assert.Equal(
            created.Generation.ManufacturingSpec.GrooveOffsetFromRear.Millimeters
                .ToString("0.###", CultureInfo.InvariantCulture),
            operation.Parameters["offsetFromRearMm"]);
    }

    [Fact]
    public void Build_CreatesAssemblyReferencingGenericCodes()
    {
        var project = new ProjectModel(
            "Cabinet Assembly Manufacturing");

        var created = new CabinetService().Create(
            project,
            shelfCount: 2,
            doorCount: 1);

        var definition =
            new CabinetManufacturingDefinitionService().Build(
                project,
                created.Cabinet,
                created.Generation,
                "CAB-MFG-006");

        var assembly = Assert.Single(
            definition.Assemblies);

        Assert.Equal(
            "CABINET-ASSEMBLY",
            assembly.Code);

        Assert.Contains("SIDE-L", assembly.PartCodes);
        Assert.Contains("SIDE-R", assembly.PartCodes);
        Assert.Contains("TOP", assembly.PartCodes);
        Assert.Contains("BOTTOM", assembly.PartCodes);
        Assert.Contains("BACK", assembly.PartCodes);
        Assert.Contains("SHELF", assembly.PartCodes);
        Assert.Contains("DOOR", assembly.PartCodes);

        Assert.Contains(
            "HINGE-110",
            assembly.HardwareCodes);

        Assert.Contains(
            "SCREW-CARCASS",
            assembly.HardwareCodes);
    }

    [Fact]
    public void Build_MapsCabinetEdgeFlagsToGenericFlags()
    {
        var project = new ProjectModel(
            "Edge Mapping");

        var created = new CabinetService().Create(
            project,
            shelfCount: 0,
            doorCount: 1);

        var definition =
            new CabinetManufacturingDefinitionService().Build(
                project,
                created.Cabinet,
                created.Generation,
                "CAB-MFG-007");

        var door = Assert.Single(
            definition.Parts,
            x => x.Code == "DOOR");

        Assert.Equal(
            ManufacturingEdgeFlags.Left |
            ManufacturingEdgeFlags.Right |
            ManufacturingEdgeFlags.Top |
            ManufacturingEdgeFlags.Bottom,
            door.EdgeBanding);
    }

    [Fact]
    public void Build_DoesNotPublishManufacturingForInvalidGeneration()
    {
        var project = new ProjectModel(
            "Invalid Cabinet Manufacturing");

        var cabinet = new Cabinet(
            "Invalid",
            DesignStudio.Domain.Units.Length.FromMillimeters(100),
            DesignStudio.Domain.Units.Length.FromMillimeters(100),
            DesignStudio.Domain.Units.Length.FromMillimeters(100),
            DesignStudio.Domain.Units.Length.FromMillimeters(18),
            DesignStudio.Domain.Units.Length.FromMillimeters(6));

        project.Add(cabinet);

        var generation =
            new CabinetParametricService().Generate(cabinet);

        Assert.False(generation.Validation.IsValid);

        var definition =
            new CabinetManufacturingDefinitionService().Build(
                project,
                cabinet,
                generation,
                "CAB-MFG-008");

        Assert.Empty(definition.Parts);
        Assert.Empty(definition.Hardware);
        Assert.Empty(definition.Operations);
        Assert.Empty(definition.Assemblies);
    }
}