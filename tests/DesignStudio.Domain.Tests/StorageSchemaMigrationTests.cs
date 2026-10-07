using System.Text;
using System.Text.Json;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public sealed class StorageSchemaMigrationTests
{
    [Fact]
    public async Task Load_V1_Project_Migrates_To_Current_Domain_Model()
    {
        const string projectId =
            "11111111-1111-1111-1111-111111111111";

        const string wallId =
            "22222222-2222-2222-2222-222222222222";

        const string doorId =
            "33333333-3333-3333-3333-333333333333";

        const string json =
            """
            {
              "schemaVersion": 1,
              "projectId": "11111111-1111-1111-1111-111111111111",
              "name": "Legacy V1 Project",
              "objects": [
                {
                  "id": "22222222-2222-2222-2222-222222222222",
                  "type": "Wall",
                  "data": {
                    "startX": 0,
                    "startY": 0,
                    "endX": 3000,
                    "endY": 0,
                    "thicknessMm": 120,
                    "heightMm": 2800
                  }
                },
                {
                  "id": "33333333-3333-3333-3333-333333333333",
                  "type": "Door",
                  "data": {
                    "hostWallId": "22222222-2222-2222-2222-222222222222",
                    "offsetMm": 500,
                    "widthMm": 900,
                    "heightMm": 2100
                  }
                }
              ]
            }
            """;

        var restored =
            await LoadAsync(json);

        Assert.Equal(
            Guid.Parse(projectId),
            restored.Id.Value);

        Assert.Equal(
            "Legacy V1 Project",
            restored.Name);

        Assert.Empty(
            restored.ProductDefinitions);

        Assert.Equal(
            2,
            restored.Objects.Count);

        var wall =
            restored.Get<Wall>(
                new EntityId(
                    Guid.Parse(wallId)));

        var door =
            restored.Get<Door>(
                new EntityId(
                    Guid.Parse(doorId)));

        Assert.Equal(
            3000,
            wall.LengthMm);

        Assert.Equal(
            Guid.Parse(wallId),
            door.HostWallId.Value);
    }

    [Fact]
    public async Task Load_V2_Project_Migrates_Room_To_Current_Boundary_Model()
    {
        const string projectId =
            "11111111-1111-1111-1111-111111111111";

        const string wallId =
            "22222222-2222-2222-2222-222222222222";

        const string roomId =
            "44444444-4444-4444-4444-444444444444";

        const string json =
            """
            {
              "schemaVersion": 2,
              "projectId": "11111111-1111-1111-1111-111111111111",
              "name": "Legacy V2 Project",
              "productDefinitions": [],
              "objects": [
                {
                  "id": "22222222-2222-2222-2222-222222222222",
                  "type": "Wall",
                  "data": {
                    "startX": 0,
                    "startY": 0,
                    "endX": 3000,
                    "endY": 0,
                    "thicknessMm": 120,
                    "heightMm": 2800
                  },
                  "metadata": null
                },
                {
                  "id": "44444444-4444-4444-4444-444444444444",
                  "type": "Room",
                  "data": {
                    "name": "Legacy Room",
                    "heightMm": 2800,
                    "wallIds": [
                      "22222222-2222-2222-2222-222222222222"
                    ],
                    "floorId": null
                  },
                  "metadata": null
                }
              ]
            }
            """;

        var restored =
            await LoadAsync(json);

        Assert.Equal(
            Guid.Parse(projectId),
            restored.Id.Value);

        var room =
            restored.Get<Room>(
                new EntityId(
                    Guid.Parse(roomId)));

        var edge =
            Assert.Single(
                room.Boundary.OuterLoop.Edges);

        Assert.Equal(
            Guid.Parse(wallId),
            edge.WallId.Value);

        Assert.False(
            edge.IsReversed);

        Assert.False(
            edge.Span.HasValue);
    }

    [Fact]
    public async Task Load_V3_Project_Migrates_Room_To_V4_Shape_And_Preserves_Reversal()
    {
        const string wallId =
            "22222222-2222-2222-2222-222222222222";

        const string roomId =
            "44444444-4444-4444-4444-444444444444";

        const string json =
            """
            {
              "schemaVersion": 3,
              "projectId": "11111111-1111-1111-1111-111111111111",
              "name": "Legacy V3 Project",
              "productDefinitions": [],
              "objects": [
                {
                  "id": "22222222-2222-2222-2222-222222222222",
                  "type": "Wall",
                  "data": {
                    "startX": 0,
                    "startY": 0,
                    "endX": 3000,
                    "endY": 0,
                    "thicknessMm": 120,
                    "heightMm": 2800
                  },
                  "metadata": null
                },
                {
                  "id": "44444444-4444-4444-4444-444444444444",
                  "type": "Room",
                  "data": {
                    "name": "Legacy V3 Room",
                    "heightMm": 2800,
                    "boundaryEdges": [
                      {
                        "wallId": "22222222-2222-2222-2222-222222222222",
                        "isReversed": true
                      }
                    ],
                    "floorId": null
                  },
                  "metadata": null
                }
              ]
            }
            """;

        var restored =
            await LoadAsync(json);

        var room =
            restored.Get<Room>(
                new EntityId(
                    Guid.Parse(roomId)));

        var edge =
            Assert.Single(
                room.Boundary.OuterLoop.Edges);

        Assert.Equal(
            Guid.Parse(wallId),
            edge.WallId.Value);

        Assert.True(
            edge.IsReversed);

        Assert.False(
            edge.Span.HasValue);
    }

    [Fact]
    public async Task Load_V4_Project_Preserves_Boundary_Span()
    {
        const string roomId =
            "44444444-4444-4444-4444-444444444444";

        const string json =
            """
            {
              "schemaVersion": 4,
              "projectId": "11111111-1111-1111-1111-111111111111",
              "name": "Current V4 Project",
              "productDefinitions": [],
              "objects": [
                {
                  "id": "22222222-2222-2222-2222-222222222222",
                  "type": "Wall",
                  "data": {
                    "startX": 0,
                    "startY": 0,
                    "endX": 3000,
                    "endY": 0,
                    "thicknessMm": 120,
                    "heightMm": 2800
                  },
                  "metadata": null
                },
                {
                  "id": "44444444-4444-4444-4444-444444444444",
                  "type": "Room",
                  "data": {
                    "name": "Current Room",
                    "heightMm": 2800,
                    "boundaryEdges": [
                      {
                        "wallId": "22222222-2222-2222-2222-222222222222",
                        "isReversed": true,
                        "startOffsetMm": 500,
                        "endOffsetMm": 2500
                      }
                    ],
                    "floorId": null
                  },
                  "metadata": null
                }
              ]
            }
            """;

        var restored =
            await LoadAsync(json);

        var room =
            restored.Get<Room>(
                new EntityId(
                    Guid.Parse(roomId)));

        var edge =
            Assert.Single(
                room.Boundary.OuterLoop.Edges);

        Assert.True(
            edge.IsReversed);

        Assert.True(
            edge.Span.HasValue);

        Assert.Equal(
            500,
            edge.Span.Value.StartOffset.Millimeters);

        Assert.Equal(
            2500,
            edge.Span.Value.EndOffset.Millimeters);
    }

    [Fact]
    public async Task Save_Writes_Current_Schema_V4()
    {
        var project =
            new ProjectModel(
                "Current Schema Project");

        project.Add(
            new Wall(
                new Point2D(0, 0),
                new Point2D(3000, 0),
                Length.FromMillimeters(120),
                Length.FromMillimeters(2800)));

        var serializer =
            new JsonProjectSerializer();

        await using var stream =
            new MemoryStream();

        await serializer.SaveAsync(
            project,
            stream);

        stream.Position = 0;

        using var json =
            JsonDocument.Parse(stream);

        Assert.Equal(
            4,
            json.RootElement
                .GetProperty("schemaVersion")
                .GetInt32());

        Assert.Equal(
            1,
            json.RootElement
                .GetProperty("objects")
                .GetArrayLength());
    }

    [Fact]
    public async Task Load_Unsupported_Schema_Version_Throws()
    {
        const string json =
            """
            {
              "schemaVersion": 999,
              "projectId": "11111111-1111-1111-1111-111111111111",
              "name": "Future Project",
              "objects": []
            }
            """;

        await Assert.ThrowsAsync<NotSupportedException>(
            () => LoadAsync(json));
    }

    [Fact]
    public async Task Load_Missing_Schema_Version_Throws_InvalidData()
    {
        const string json =
            """
            {
              "projectId": "11111111-1111-1111-1111-111111111111",
              "name": "Invalid Project",
              "objects": []
            }
            """;

        await Assert.ThrowsAsync<InvalidDataException>(
            () => LoadAsync(json));
    }

    private static async Task<ProjectModel> LoadAsync(
        string json)
    {
        var serializer =
            new JsonProjectSerializer();

        await using var stream =
            new MemoryStream(
                Encoding.UTF8.GetBytes(json));

        return await serializer.LoadAsync(stream);
    }
}
