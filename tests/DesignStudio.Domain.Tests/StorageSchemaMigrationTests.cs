using System.Text;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public class StorageSchemaMigrationTests
{
    [Fact]
    public async Task Load_V1_Project_Migrates_To_Current_Domain_Model()
    {
        const string projectId = "11111111-1111-1111-1111-111111111111";
        const string wallId = "22222222-2222-2222-2222-222222222222";
        const string doorId = "33333333-3333-3333-3333-333333333333";

        const string json = """
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

        var serializer = new JsonProjectSerializer();

        await using var stream =
            new MemoryStream(Encoding.UTF8.GetBytes(json));

        var restored = await serializer.LoadAsync(stream);

        Assert.Equal(
            Guid.Parse(projectId),
            restored.Id.Value);

        Assert.Equal(
            "Legacy V1 Project",
            restored.Name);

        Assert.Empty(restored.ProductDefinitions);

        Assert.Equal(
            2,
            restored.Objects.Count);

        var wall = restored.Get<Wall>(
            new EntityId(Guid.Parse(wallId)));

        var door = restored.Get<Door>(
            new EntityId(Guid.Parse(doorId)));

        Assert.Equal(
            3000,
            wall.LengthMm);

        Assert.Equal(
            Guid.Parse(wallId),
            door.HostWallId.Value);
    }

    [Fact]
    public async Task Load_Unsupported_Schema_Version_Throws()
    {
        const string json = """
        {
          "schemaVersion": 4,
          "projectId": "11111111-1111-1111-1111-111111111111",
          "name": "Future Project",
          "objects": []
        }
        """;

        var serializer = new JsonProjectSerializer();

        await using var stream =
            new MemoryStream(Encoding.UTF8.GetBytes(json));

        await Assert.ThrowsAsync<NotSupportedException>(
            () => serializer.LoadAsync(stream));
    }
}
