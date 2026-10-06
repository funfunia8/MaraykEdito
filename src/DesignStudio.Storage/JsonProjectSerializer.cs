using System.Text.Json;
using System.Text.Json.Serialization;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Products;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Storage;

public sealed class JsonProjectSerializer : IProjectSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(
        Project project,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(destination);

        var document = ToDocument(project);

        await JsonSerializer.SerializeAsync(
            destination,
            document,
            _options,
            cancellationToken);
    }

    public async Task<Project> LoadAsync(
        Stream source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        using var json = await JsonDocument.ParseAsync(
            source,
            cancellationToken: cancellationToken);

        if (!json.RootElement.TryGetProperty(
                "schemaVersion",
                out var schemaVersionElement) ||
            !schemaVersionElement.TryGetInt32(
                out var schemaVersion))
        {
            throw new InvalidDataException(
                "Project document does not contain a valid schema version.");
        }

        return schemaVersion switch
        {
            1 => FromDocument(
                MigrateV3ToV4(
                    MigrateV2ToV3(
                        MigrateV1(
                            json.RootElement.Deserialize<ProjectDocumentV1>(
                                _options)
                            ?? throw new InvalidDataException(
                                "Project document is invalid."))))),

            2 => FromDocument(
                MigrateV3ToV4(
                    MigrateV2ToV3(
                        json.RootElement.Deserialize<ProjectDocumentV2>(
                            _options)
                        ?? throw new InvalidDataException(
                            "Project document is invalid.")))),

            3 => FromDocument(
                MigrateV3ToV4(
                    json.RootElement.Deserialize<ProjectDocumentV3>(
                        _options)
                    ?? throw new InvalidDataException(
                        "Project document is invalid."))),

            4 => FromDocument(
                json.RootElement.Deserialize<ProjectDocumentV4>(
                    _options)
                ?? throw new InvalidDataException(
                    "Project document is invalid.")),

            _ => throw new NotSupportedException(
                $"Unsupported project schema version: {schemaVersion}. " +
                "Supported project schemas: 1, 2, 3, and 4.")
        };
    }

    private static ProjectDocumentV2 MigrateV1(
        ProjectDocumentV1 v1)
    {
        return new ProjectDocumentV2
        {
            SchemaVersion = 2,
            ProjectId = v1.ProjectId,
            Name = v1.Name,
            ProductDefinitions = new List<ProductDefinitionRecordV2>(),
            Objects = v1.Objects
                .Select(record => new ProjectObjectRecordV2
                {
                    Id = record.Id,
                    Type = record.Type,
                    Data = record.Data,
                    Metadata = null
                })
                .ToList()
        };
    }

    private static ProjectDocumentV3 MigrateV2ToV3(
        ProjectDocumentV2 v2)
    {
        return new ProjectDocumentV3
        {
            SchemaVersion = 3,
            ProjectId = v2.ProjectId,
            Name = v2.Name,
            ProductDefinitions = v2.ProductDefinitions
                .Select(definition => new ProductDefinitionRecordV3
                {
                    Id = definition.Id,
                    Code = definition.Code,
                    Name = definition.Name,
                    Category = definition.Category,
                    Manufacturer = definition.Manufacturer,
                    Model = definition.Model
                })
                .ToList(),
            Objects = v2.Objects
                .Select(record =>
                {
                    var data = record.Data;

                    if (record.Type == "Room")
                    {
                        var roomData =
                            record.Data.Deserialize<RoomData>(
                                SerializerOptions())
                            ?? throw new InvalidDataException(
                                "Invalid room data.");

                        data = JsonSerializer.SerializeToElement(
                            new RoomDataV3(
                                roomData.Name,
                                roomData.HeightMm,
                                roomData.WallIds
                                    .Select(
                                        wallId =>
                                            new BoundaryEdgeDataV3(
                                                wallId,
                                                false))
                                    .ToList(),
                                roomData.FloorId),
                            SerializerOptions());
                    }

                    return new ProjectObjectRecordV3
                    {
                        Id = record.Id,
                        Type = record.Type,
                        Data = data,
                        Metadata = record.Metadata
                    };
                })
                .ToList()
        };
    }

    private static ProjectDocumentV4 MigrateV3ToV4(
        ProjectDocumentV3 v3)
    {
        return new ProjectDocumentV4
        {
            SchemaVersion = 4,
            ProjectId = v3.ProjectId,
            Name = v3.Name,
            ProductDefinitions = v3.ProductDefinitions
                .Select(definition => new ProductDefinitionRecordV4
                {
                    Id = definition.Id,
                    Code = definition.Code,
                    Name = definition.Name,
                    Category = definition.Category,
                    Manufacturer = definition.Manufacturer,
                    Model = definition.Model
                })
                .ToList(),

            Objects = v3.Objects
                .Select(record =>
                {
                    var data = record.Data;

                    if (record.Type == "Room")
                    {
                        var roomData =
                            record.Data.Deserialize<RoomDataV3>(
                                SerializerOptions())
                            ?? throw new InvalidDataException(
                                "Invalid Schema 3 room data.");

                        data = JsonSerializer.SerializeToElement(
                            new RoomDataV4(
                                roomData.Name,
                                roomData.HeightMm,
                                roomData.BoundaryEdges
                                    .Select(
                                        edge =>
                                            new BoundaryEdgeDataV4(
                                                edge.WallId,
                                                edge.IsReversed,
                                                null,
                                                null))
                                    .ToList(),
                                roomData.FloorId),
                            SerializerOptions());
                    }

                    return new ProjectObjectRecordV4
                    {
                        Id = record.Id,
                        Type = record.Type,
                        Data = data,
                        Metadata = record.Metadata
                    };
                })
                .ToList()
        };
    }

    private ProjectDocumentV4 ToDocument(
        Project project)
    {
        var doc = new ProjectDocumentV4
        {
            SchemaVersion = 4,
            ProjectId = project.Id.Value.ToString("D"),
            Name = project.Name
        };

        foreach (var definition in project.ProductDefinitions)
        {
            doc.ProductDefinitions.Add(
                new ProductDefinitionRecordV4
                {
                    Id = definition.Id.Value.ToString("D"),
                    Code = definition.Code,
                    Name = definition.Name,
                    Category = definition.Category.ToString(),
                    Manufacturer = definition.Manufacturer,
                    Model = definition.Model
                });
        }

        foreach (var material in project.Materials)
        {
            var data = new MaterialData(
                material.Code,
                material.Name,
                material.Kind,
                material.Thickness.Millimeters,
                material.SheetWidth.Millimeters,
                material.SheetHeight.Millimeters,
                material.BandWidth.Millimeters,
                material.GrainDirection,
                material.MaterialCategory,
                material.Manufacturer,
                material.Supplier,
                material.Color,
                material.Finish,
                material.Unit);

            doc.Objects.Add(
                new ProjectObjectRecordV4
                {
                    Id = material.Id.Value.ToString("D"),
                    Type = material.Type,
                    Data = JsonSerializer.SerializeToElement(
                        data,
                        _options),
                    Metadata = ToMetadata(material)
                });
        }

        foreach (var obj in project.Objects)
        {
            object data = obj switch
            {
                Building building =>
                    new BuildingData(
                        building.Name),

                Floor floor =>
                    new FloorData(
                        floor.Name,
                        ToNullableId(floor.BuildingId)),

                Room room =>
                    new RoomDataV4(
                        room.Name,
                        room.Height.Millimeters,
                        room.Boundary.OuterLoop.Edges
                            .Select(
                                edge =>
                                    ToBoundaryEdgeData(edge))
                            .ToList(),
                        ToNullableId(room.FloorId)),

                Wall wall =>
                    new WallData(
                        wall.Start.X,
                        wall.Start.Y,
                        wall.End.X,
                        wall.End.Y,
                        wall.Thickness.Millimeters,
                        wall.Height.Millimeters),

                Door door =>
                    new DoorData(
                        door.HostWallId.Value.ToString("D"),
                        door.OffsetFromWallStart.Millimeters,
                        door.Width.Millimeters,
                        door.Height.Millimeters),

                Window window =>
                    new WindowData(
                        window.HostWallId.Value.ToString("D"),
                        window.OffsetFromWallStart.Millimeters,
                        window.Width.Millimeters,
                        window.Height.Millimeters,
                        window.SillHeight.Millimeters),

                Cabinet cabinet =>
                    new CabinetData(
                        cabinet.Name,
                        cabinet.Width.Millimeters,
                        cabinet.Height.Millimeters,
                        cabinet.Depth.Millimeters,
                        cabinet.PanelThickness.Millimeters,
                        cabinet.BackThickness.Millimeters,
                        cabinet.ConstructionMethod,
                        cabinet.ShelfCount,
                        cabinet.DoorCount,
                        ToNullableId(cabinet.CarcassMaterialId),
                        ToNullableId(cabinet.BackMaterialId),
                        ToNullableId(cabinet.DoorMaterialId),
                        ToNullableId(cabinet.EdgeBandMaterialId),
                        ToNullableId(cabinet.HostRoomId),
                        cabinet.Position.X,
                        cabinet.Position.Y,
                        cabinet.RotationDegrees,
                        ToNullableId(cabinet.ProductDefinitionId)),

                HardwareItem hardware =>
                    new HardwareData(
                        hardware.Code,
                        hardware.Name,
                        hardware.Kind,
                        hardware.Unit),

                _ =>
                    throw new NotSupportedException(
                        $"Unsupported project object: {obj.GetType().Name}")
            };

            doc.Objects.Add(
                new ProjectObjectRecordV4
                {
                    Id = obj.Id.Value.ToString("D"),
                    Type = obj.Type,
                    Data = JsonSerializer.SerializeToElement(
                        data,
                        _options),
                    Metadata = ToMetadata(obj)
                });
        }

        return doc;
    }

    private static BoundaryEdgeDataV4 ToBoundaryEdgeData(
        BoundaryEdge edge)
    {
        if (edge.Span is null)
        {
            return new BoundaryEdgeDataV4(
                edge.WallId.Value.ToString("D"),
                edge.IsReversed,
                null,
                null);
        }

        var span = edge.Span.Value;

        return new BoundaryEdgeDataV4(
            edge.WallId.Value.ToString("D"),
            edge.IsReversed,
            span.StartOffset.Millimeters,
            span.EndOffset.Millimeters);
    }

    private static Project FromDocument(
        ProjectDocumentV4 doc)
    {
        if (!Guid.TryParse(
                doc.ProjectId,
                out var projectGuid))
        {
            throw new InvalidDataException(
                "Invalid project id.");
        }

        var project =
            Project.Restore(
                new EntityId(projectGuid),
                doc.Name);

        foreach (var record in doc.ProductDefinitions)
        {
            if (!Guid.TryParse(
                    record.Id,
                    out var idGuid))
            {
                throw new InvalidDataException(
                    $"Invalid product definition id: {record.Id}");
            }

            if (!Enum.TryParse<ProductCategory>(
                    record.Category,
                    true,
                    out var category))
            {
                throw new InvalidDataException(
                    $"Invalid product definition category: {record.Category}");
            }

            var definition =
                new ProductDefinition(
                    new EntityId(idGuid),
                    record.Code,
                    record.Name,
                    category,
                    Manufacturer: record.Manufacturer,
                    Model: record.Model);

            project.AddProductDefinition(definition);
        }

        foreach (var record in doc.Objects)
        {
            if (!Guid.TryParse(
                    record.Id,
                    out var idGuid))
            {
                throw new InvalidDataException(
                    $"Invalid object id: {record.Id}");
            }

            var id = new EntityId(idGuid);

            switch (record.Type)
            {
                case "Building":
                {
                    var data =
                        record.Data.Deserialize<BuildingData>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid building data.");

                    project.Add(
                        new Building(
                            data.Name,
                            id));

                    break;
                }

                case "Floor":
                {
                    var data =
                        record.Data.Deserialize<FloorData>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid floor data.");

                    project.Add(
                        new Floor(
                            data.Name,
                            ParseNullableId(data.BuildingId),
                            id));

                    break;
                }

                case "Room":
                {
                    var data =
                        record.Data.Deserialize<RoomDataV4>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid room data.");

                    var room =
                        new Room(
                            data.Name,
                            id,
                            ParseNullableId(data.FloorId));

                    room.SetHeight(
                        Length.FromMillimeters(
                            data.HeightMm));

                    foreach (var edge in data.BoundaryEdges)
                    {
                        if (!Guid.TryParse(
                                edge.WallId,
                                out var wallGuid))
                        {
                            throw new InvalidDataException(
                                $"Invalid room boundary wall id: {edge.WallId}");
                        }

                        var wallId =
                            new EntityId(wallGuid);

                        room.AddBoundaryEdge(
                            CreateBoundaryEdge(
                                wallId,
                                edge));
                    }

                    project.Add(room);

                    break;
                }

                case "Wall":
                {
                    var data =
                        record.Data.Deserialize<WallData>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid wall data.");

                    project.Add(
                        new Wall(
                            new Point2D(
                                data.StartX,
                                data.StartY),
                            new Point2D(
                                data.EndX,
                                data.EndY),
                            Length.FromMillimeters(
                                data.ThicknessMm),
                            Length.FromMillimeters(
                                data.HeightMm),
                            id));

                    break;
                }

                case "Door":
                {
                    var data =
                        record.Data.Deserialize<DoorData>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid door data.");

                    if (!Guid.TryParse(
                            data.HostWallId,
                            out var wallGuid))
                    {
                        throw new InvalidDataException(
                            "Invalid door host wall id.");
                    }

                    project.Add(
                        new Door(
                            new EntityId(wallGuid),
                            Length.FromMillimeters(
                                data.OffsetMm),
                            Length.FromMillimeters(
                                data.WidthMm),
                            Length.FromMillimeters(
                                data.HeightMm),
                            id));

                    break;
                }

                case "Window":
                {
                    var data =
                        record.Data.Deserialize<WindowData>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid window data.");

                    if (!Guid.TryParse(
                            data.HostWallId,
                            out var wallGuid))
                    {
                        throw new InvalidDataException(
                            "Invalid window host wall id.");
                    }

                    project.Add(
                        new Window(
                            new EntityId(wallGuid),
                            Length.FromMillimeters(
                                data.OffsetMm),
                            Length.FromMillimeters(
                                data.WidthMm),
                            Length.FromMillimeters(
                                data.HeightMm),
                            Length.FromMillimeters(
                                data.SillMm),
                            id));

                    break;
                }

                case "Cabinet":
                {
                    var data =
                        record.Data.Deserialize<CabinetData>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid cabinet data.");

                    var cabinet =
                        new Cabinet(
                            data.Name,
                            Length.FromMillimeters(
                                data.WidthMm),
                            Length.FromMillimeters(
                                data.HeightMm),
                            Length.FromMillimeters(
                                data.DepthMm),
                            Length.FromMillimeters(
                                data.PanelThicknessMm),
                            Length.FromMillimeters(
                                data.BackThicknessMm),
                            data.ConstructionMethod,
                            data.ShelfCount,
                            data.DoorCount,
                            id);

                    cabinet.SetMaterialAssignments(
                        ParseNullableId(
                            data.CarcassMaterialId),
                        ParseNullableId(
                            data.BackMaterialId),
                        ParseNullableId(
                            data.DoorMaterialId),
                        ParseNullableId(
                            data.EdgeBandMaterialId));

                    cabinet.SetPlacement(
                        ParseNullableId(
                            data.HostRoomId),
                        new Point2D(
                            data.PositionX ?? 0,
                            data.PositionY ?? 0),
                        data.RotationDegrees ?? 0);

                    cabinet.SetProductDefinition(
                        ParseNullableId(
                            data.ProductDefinitionId));

                    project.Add(cabinet);

                    break;
                }

                case "HardwareItem":
                {
                    var data =
                        record.Data.Deserialize<HardwareData>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid hardware data.");

                    project.Add(
                        new HardwareItem(
                            data.Code,
                            data.Name,
                            data.Kind,
                            data.Unit,
                            id));

                    break;
                }

                case "Material":
                {
                    var data =
                        record.Data.Deserialize<MaterialData>(
                            SerializerOptions())
                        ?? throw new InvalidDataException(
                            "Invalid material data.");

                    var material =
                        new Material(
                            data.Code,
                            data.Name,
                            data.Kind,
                            Length.FromMillimeters(
                                data.ThicknessMm),
                            Length.FromMillimeters(
                                data.SheetWidthMm),
                            Length.FromMillimeters(
                                data.SheetHeightMm),
                            Length.FromMillimeters(
                                data.BandWidthMm),
                            data.GrainDirection,
                            id);

                    material.SetCatalogMetadata(
                        data.Category,
                        data.Manufacturer,
                        data.Supplier,
                        data.Color,
                        data.Finish,
                        data.Unit ?? "piece");

                    project.AddMaterial(material);

                    break;
                }

                default:
                    throw new NotSupportedException(
                        $"Unsupported project object type: {record.Type}");
            }

            if (record.Metadata is not null)
            {
                if (project.TryGet(
                        id,
                        out var restoredObject) &&
                    restoredObject is not null)
                {
                    ApplyMetadata(
                        restoredObject,
                        record.Metadata);
                }
                else if (
                    project.TryGetMaterial(
                        id,
                        out var restoredMaterial) &&
                    restoredMaterial is not null)
                {
                    ApplyMetadata(
                        restoredMaterial,
                        record.Metadata);
                }
            }
        }

        return project;
    }

    private static BoundaryEdge CreateBoundaryEdge(
        EntityId wallId,
        BoundaryEdgeDataV4 data)
    {
        var hasStart =
            data.StartOffsetMm.HasValue;

        var hasEnd =
            data.EndOffsetMm.HasValue;

        if (hasStart != hasEnd)
        {
            throw new InvalidDataException(
                $"Boundary edge for wall {wallId} contains only one span offset.");
        }

        if (!hasStart)
        {
            return new BoundaryEdge(
                wallId,
                data.IsReversed);
        }

        var startMm =
            data.StartOffsetMm!.Value;

        var endMm =
            data.EndOffsetMm!.Value;

        if (!double.IsFinite(startMm) ||
            !double.IsFinite(endMm))
        {
            throw new InvalidDataException(
                $"Boundary edge for wall {wallId} contains non-finite span offsets.");
        }

        try
        {
            var span =
                new WallSpan(
                    Length.FromMillimeters(startMm),
                    Length.FromMillimeters(endMm));

            return new BoundaryEdge(
                wallId,
                span,
                data.IsReversed);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new InvalidDataException(
                $"Boundary edge for wall {wallId} contains an invalid span.",
                exception);
        }
    }

    private static DesignObjectMetadataData ToMetadata(
        ProjectObject obj)
        => new()
        {
            Category =
                obj.Category.ToString(),

            ParentId =
                obj.ParentId?.Value.ToString("D"),

            LayerId =
                obj.Metadata.LayerId?.Value.ToString("D"),

            PositionX =
                obj.Placement.Position.X,

            PositionY =
                obj.Placement.Position.Y,

            RotationDegrees =
                obj.Placement.RotationDegrees,

            IsVisible =
                obj.Metadata.IsVisible,

            IsLocked =
                obj.Metadata.IsLocked
        };

    private static void ApplyMetadata(
        ProjectObject obj,
        DesignObjectMetadataData metadata)
    {
        if (Enum.TryParse<ObjectCategory>(
                metadata.Category,
                true,
                out var category))
        {
            obj.SetCategory(category);
        }

        obj.SetParent(
            ParseNullableId(
                metadata.ParentId));

        obj.SetLayer(
            ParseNullableId(
                metadata.LayerId));

        obj.SetPlacement(
            new Placement2D(
                new Point2D(
                    metadata.PositionX,
                    metadata.PositionY),
                metadata.RotationDegrees));

        obj.SetVisibility(
            metadata.IsVisible);

        obj.SetLocked(
            metadata.IsLocked);
    }

    private static JsonSerializerOptions SerializerOptions()
        => new()
        {
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase,

            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    private static string? ToNullableId(
        EntityId? id)
        => id?.Value.ToString("D");

    private static EntityId? ParseNullableId(
        string? value)
        => Guid.TryParse(
            value,
            out var guid)
            ? new EntityId(guid)
            : null;

    private sealed record BuildingData(
        string Name);

    private sealed record FloorData(
        string Name,
        string? BuildingId = null);

    private sealed record RoomData(
        string Name,
        double HeightMm,
        List<string> WallIds,
        string? FloorId = null);

    private sealed record RoomDataV3(
        string Name,
        double HeightMm,
        List<BoundaryEdgeDataV3> BoundaryEdges,
        string? FloorId = null);

    private sealed record RoomDataV4(
        string Name,
        double HeightMm,
        List<BoundaryEdgeDataV4> BoundaryEdges,
        string? FloorId = null);

    private sealed record BoundaryEdgeDataV3(
        string WallId,
        bool IsReversed);

    private sealed record BoundaryEdgeDataV4(
        string WallId,
        bool IsReversed,
        double? StartOffsetMm,
        double? EndOffsetMm);

    private sealed record WallData(
        double StartX,
        double StartY,
        double EndX,
        double EndY,
        double ThicknessMm,
        double HeightMm);

    private sealed record DoorData(
        string HostWallId,
        double OffsetMm,
        double WidthMm,
        double HeightMm);

    private sealed record WindowData(
        string HostWallId,
        double OffsetMm,
        double WidthMm,
        double HeightMm,
        double SillMm);

    private sealed record CabinetData(
        string Name,
        double WidthMm,
        double HeightMm,
        double DepthMm,
        double PanelThicknessMm,
        double BackThicknessMm,
        CabinetConstructionMethod ConstructionMethod,
        int ShelfCount,
        int DoorCount,
        string? CarcassMaterialId = null,
        string? BackMaterialId = null,
        string? DoorMaterialId = null,
        string? EdgeBandMaterialId = null,
        string? HostRoomId = null,
        double? PositionX = null,
        double? PositionY = null,
        double? RotationDegrees = null,
        string? ProductDefinitionId = null);

    private sealed record HardwareData(
        string Code,
        string Name,
        HardwareKind Kind,
        HardwareUnit Unit);

    private sealed record MaterialData(
        string Code,
        string Name,
        MaterialKind Kind,
        double ThicknessMm,
        double SheetWidthMm,
        double SheetHeightMm,
        double BandWidthMm,
        MaterialGrainDirection GrainDirection,
        MaterialCategory Category = MaterialCategory.Generic,
        string? Manufacturer = null,
        string? Supplier = null,
        string? Color = null,
        string? Finish = null,
        string? Unit = null);
}
