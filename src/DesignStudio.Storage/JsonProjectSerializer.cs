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

    public async Task SaveAsync(Project project, Stream destination, CancellationToken cancellationToken = default)
    {
        var document = ToDocument(project);
        await JsonSerializer.SerializeAsync(destination, document, _options, cancellationToken);
    }

    public async Task<Project> LoadAsync(Stream source, CancellationToken cancellationToken = default)
    {
        var document = await JsonSerializer.DeserializeAsync<ProjectDocumentV2>(source, _options, cancellationToken)
            ?? throw new InvalidDataException("Project document is empty.");

        if (document.SchemaVersion is not (1 or 2))
            throw new NotSupportedException($"Unsupported project schema version: {document.SchemaVersion}");

        return FromDocument(document);
    }

    private ProjectDocumentV2 ToDocument(Project project)
    {
        var doc = new ProjectDocumentV2
        {
            ProjectId = project.Id.Value.ToString("D"),
            Name = project.Name
        };

        foreach (var definition in project.ProductDefinitions)
        {
            doc.ProductDefinitions.Add(new ProductDefinitionRecordV2
            {
                Id = definition.Id.Value.ToString("D"),
                Code = definition.Code,
                Name = definition.Name,
                Category = definition.Category.ToString(),
                Manufacturer = definition.Manufacturer,
                Model = definition.Model
            });
        }

        foreach (var obj in project.Objects)
        {
            object data = obj switch
            {
                Building b => new BuildingData(b.Name),
                Floor f => new FloorData(f.Name, ToNullableId(f.BuildingId)),
                Room r => new RoomData(r.Name, r.Height.Millimeters, r.WallIds.Select(x => x.Value.ToString("D")).ToList(), ToNullableId(r.FloorId)),
                Wall w => new WallData(w.Start.X, w.Start.Y, w.End.X, w.End.Y, w.Thickness.Millimeters, w.Height.Millimeters),
                Door d => new DoorData(d.HostWallId.Value.ToString("D"), d.OffsetFromWallStart.Millimeters, d.Width.Millimeters, d.Height.Millimeters),
                Window w => new WindowData(w.HostWallId.Value.ToString("D"), w.OffsetFromWallStart.Millimeters, w.Width.Millimeters, w.Height.Millimeters, w.SillHeight.Millimeters),
                Cabinet c => new CabinetData(
                    c.Name, c.Width.Millimeters, c.Height.Millimeters, c.Depth.Millimeters,
                    c.PanelThickness.Millimeters, c.BackThickness.Millimeters, c.ConstructionMethod,
                    c.ShelfCount, c.DoorCount,
                    ToNullableId(c.CarcassMaterialId), ToNullableId(c.BackMaterialId),
                    ToNullableId(c.DoorMaterialId), ToNullableId(c.EdgeBandMaterialId),
                    ToNullableId(c.HostRoomId), c.Position.X, c.Position.Y, c.RotationDegrees,
                    ToNullableId(c.ProductDefinitionId)),
                HardwareItem h => new HardwareData(
                    h.Code, h.Name, h.Kind, h.Unit),
                Material m => new MaterialData(
                    m.Code, m.Name, m.Kind, m.Thickness.Millimeters,
                    m.SheetWidth.Millimeters, m.SheetHeight.Millimeters, m.BandWidth.Millimeters, m.GrainDirection,
                    m.MaterialCategory, m.Manufacturer, m.Supplier, m.Color, m.Finish, m.Unit),
                _ => throw new NotSupportedException($"Unsupported project object: {obj.GetType().Name}")
            };

            doc.Objects.Add(new ProjectObjectRecordV2
            {
                Id = obj.Id.Value.ToString("D"),
                Type = obj.Type,
                Data = JsonSerializer.SerializeToElement(data, _options),
                Metadata = ToMetadata(obj)
            });
        }

        return doc;
    }

    private static Project FromDocument(ProjectDocumentV2 doc)
    {
        if (!Guid.TryParse(doc.ProjectId, out var projectGuid))
            throw new InvalidDataException("Invalid project id.");

        var project = Project.Restore(new EntityId(projectGuid), doc.Name);

        foreach (var record in doc.ProductDefinitions)
        {
            if (!Guid.TryParse(record.Id, out var idGuid))
                throw new InvalidDataException($"Invalid product definition id: {record.Id}");

            if (!Enum.TryParse<ProductCategory>(record.Category, true, out var category))
                throw new InvalidDataException($"Invalid product definition category: {record.Category}");

            var definition = new ProductDefinition(
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
            if (!Guid.TryParse(record.Id, out var idGuid))
                throw new InvalidDataException($"Invalid object id: {record.Id}");

            var id = new EntityId(idGuid);

            switch (record.Type)
            {
                case "Building":
                {
                    var data = record.Data.Deserialize<BuildingData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid building data.");
                    project.Add(new Building(data.Name, id));
                    break;
                }
                case "Floor":
                {
                    var data = record.Data.Deserialize<FloorData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid floor data.");
                    project.Add(new Floor(data.Name, ParseNullableId(data.BuildingId), id));
                    break;
                }
                case "Room":
                {
                    var data = record.Data.Deserialize<RoomData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid room data.");
                    var room = new Room(data.Name, id, ParseNullableId(data.FloorId));
                    room.SetHeight(Length.FromMillimeters(data.HeightMm));
                    foreach (var wallId in data.WallIds)
                        if (Guid.TryParse(wallId, out var g)) room.AddWall(new EntityId(g));
                    project.Add(room);
                    break;
                }
                case "Wall":
                {
                    var data = record.Data.Deserialize<WallData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid wall data.");
                    project.Add(new Wall(
                        new Point2D(data.StartX, data.StartY),
                        new Point2D(data.EndX, data.EndY),
                        Length.FromMillimeters(data.ThicknessMm),
                        Length.FromMillimeters(data.HeightMm),
                        id));
                    break;
                }
                case "Door":
                {
                    var data = record.Data.Deserialize<DoorData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid door data.");
                    if (!Guid.TryParse(data.HostWallId, out var wallGuid)) throw new InvalidDataException("Invalid door host wall id.");
                    project.Add(new Door(new EntityId(wallGuid), Length.FromMillimeters(data.OffsetMm), Length.FromMillimeters(data.WidthMm), Length.FromMillimeters(data.HeightMm), id));
                    break;
                }
                case "Cabinet":
                {
                    var data = record.Data.Deserialize<CabinetData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid cabinet data.");
                    var cabinet = new Cabinet(
                        data.Name,
                        Length.FromMillimeters(data.WidthMm),
                        Length.FromMillimeters(data.HeightMm),
                        Length.FromMillimeters(data.DepthMm),
                        Length.FromMillimeters(data.PanelThicknessMm),
                        Length.FromMillimeters(data.BackThicknessMm),
                        data.ConstructionMethod,
                        data.ShelfCount,
                        data.DoorCount,
                        id);
                    cabinet.SetMaterialAssignments(
                        ParseNullableId(data.CarcassMaterialId),
                        ParseNullableId(data.BackMaterialId),
                        ParseNullableId(data.DoorMaterialId),
                        ParseNullableId(data.EdgeBandMaterialId));
                    cabinet.SetPlacement(
                        ParseNullableId(data.HostRoomId),
                        new Point2D(data.PositionX ?? 0, data.PositionY ?? 0),
                        data.RotationDegrees ?? 0);
                    cabinet.SetProductDefinition(
                        ParseNullableId(data.ProductDefinitionId));
                    project.Add(cabinet);
                    break;
                }
                case "Window":
                {
                    var data = record.Data.Deserialize<WindowData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid window data.");
                    if (!Guid.TryParse(data.HostWallId, out var wallGuid)) throw new InvalidDataException("Invalid window host wall id.");
                    project.Add(new Window(new EntityId(wallGuid), Length.FromMillimeters(data.OffsetMm), Length.FromMillimeters(data.WidthMm), Length.FromMillimeters(data.HeightMm), Length.FromMillimeters(data.SillMm), id));
                    break;
                }
                case "HardwareItem":
                {
                    var data = record.Data.Deserialize<HardwareData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid hardware data.");
                    project.Add(new HardwareItem(data.Code, data.Name, data.Kind, data.Unit, id));
                    break;
                }
                case "Material":
                {
                    var data = record.Data.Deserialize<MaterialData>(_SerializerOptions()) ?? throw new InvalidDataException("Invalid material data.");
                    var material = new Material(
                        data.Code,
                        data.Name,
                        data.Kind,
                        Length.FromMillimeters(data.ThicknessMm),
                        Length.FromMillimeters(data.SheetWidthMm),
                        Length.FromMillimeters(data.SheetHeightMm),
                        Length.FromMillimeters(data.BandWidthMm),
                        data.GrainDirection,
                        id);
                    material.SetCatalogMetadata(data.Category, data.Manufacturer, data.Supplier, data.Color, data.Finish, data.Unit ?? "piece");
                    project.Add(material);
                    break;
                }
                default:
                    throw new NotSupportedException($"Unsupported project object type: {record.Type}");
            }

            if (record.Metadata is not null && project.TryGet(id, out var restoredObject) && restoredObject is not null)
                ApplyMetadata(restoredObject, record.Metadata);
        }

        return project;
    }

    private static DesignObjectMetadataData ToMetadata(ProjectObject obj)
        => new()
        {
            Category = obj.Category.ToString(),
            ParentId = obj.ParentId?.Value.ToString("D"),
            LayerId = obj.Metadata.LayerId?.Value.ToString("D"),
            PositionX = obj.Placement.Position.X,
            PositionY = obj.Placement.Position.Y,
            RotationDegrees = obj.Placement.RotationDegrees,
            IsVisible = obj.Metadata.IsVisible,
            IsLocked = obj.Metadata.IsLocked
        };

    private static void ApplyMetadata(ProjectObject obj, DesignObjectMetadataData metadata)
    {
        if (Enum.TryParse<ObjectCategory>(metadata.Category, true, out var category))
            obj.SetCategory(category);
        obj.SetParent(ParseNullableId(metadata.ParentId));
        obj.SetLayer(ParseNullableId(metadata.LayerId));
        obj.SetPlacement(new Placement2D(new Point2D(metadata.PositionX, metadata.PositionY), metadata.RotationDegrees));
        obj.SetVisibility(metadata.IsVisible);
        obj.SetLocked(metadata.IsLocked);
    }

    private static JsonSerializerOptions _SerializerOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private static string? ToNullableId(EntityId? id) => id?.Value.ToString("D");

    private static EntityId? ParseNullableId(string? value)
        => Guid.TryParse(value, out var guid) ? new EntityId(guid) : null;

    private sealed record BuildingData(string Name);
    private sealed record FloorData(string Name, string? BuildingId = null);
    private sealed record RoomData(string Name, double HeightMm, List<string> WallIds, string? FloorId = null);
    private sealed record WallData(double StartX, double StartY, double EndX, double EndY, double ThicknessMm, double HeightMm);
    private sealed record DoorData(string HostWallId, double OffsetMm, double WidthMm, double HeightMm);
    private sealed record WindowData(string HostWallId, double OffsetMm, double WidthMm, double HeightMm, double SillMm);
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
    private sealed record HardwareData(string Code, string Name, HardwareKind Kind, HardwareUnit Unit);
    private sealed record MaterialData(
        string Code, string Name, MaterialKind Kind, double ThicknessMm,
        double SheetWidthMm, double SheetHeightMm, double BandWidthMm,
        MaterialGrainDirection GrainDirection,
        MaterialCategory Category = MaterialCategory.Generic,
        string? Manufacturer = null, string? Supplier = null, string? Color = null,
        string? Finish = null, string? Unit = null);
}