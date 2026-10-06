using System.Text;
using System.Text.Json;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;
using DesignStudio.Storage;

namespace DesignStudio.Domain.Tests;

public sealed class DirectedBoundaryPersistenceTests
{
    [Fact]
    public async Task SaveAndLoadPreservesReversedBoundaryEdges()
    {
        var project = new ProjectModel("Directed Boundary");
        var room = new Room("Room");

        var first = AddWall(
            project,
            new Point2D(5000, 0),
            new Point2D(0, 0));

        var second = AddWall(
            project,
            new Point2D(5000, 4000),
            new Point2D(5000, 0));

        var third = AddWall(
            project,
            new Point2D(0, 4000),
            new Point2D(5000, 4000));

        var fourth = AddWall(
            project,
            new Point2D(0, 0),
            new Point2D(0, 4000));

        room.AddBoundaryEdge(
            new BoundaryEdge(first.Id, IsReversed: true));

        room.AddBoundaryEdge(
            new BoundaryEdge(second.Id, IsReversed: true));

        room.AddBoundaryEdge(
            new BoundaryEdge(third.Id, IsReversed: true));

        room.AddBoundaryEdge(
            new BoundaryEdge(fourth.Id, IsReversed: true));

        project.Add(room);

        var serializer = new JsonProjectSerializer();

        await using var destination = new MemoryStream();

        await serializer.SaveAsync(
            project,
            destination);

        destination.Position = 0;

        using (var document = await JsonDocument.ParseAsync(destination))
        {
            Assert.Equal(
                3,
                document.RootElement
                    .GetProperty("schemaVersion")
                    .GetInt32());

            var roomRecord = document.RootElement
                .GetProperty("objects")
                .EnumerateArray()
                .Single(record =>
                    record.GetProperty("type").GetString() == "Room");

            var edges = roomRecord
                .GetProperty("data")
                .GetProperty("boundaryEdges")
                .EnumerateArray()
                .ToArray();

            Assert.Equal(4, edges.Length);

            Assert.All(
                edges,
                edge => Assert.True(
                    edge.GetProperty("isReversed").GetBoolean()));
        }

        destination.Position = 0;

        var restored = await serializer.LoadAsync(destination);
        var restoredRoom = restored.Objects
            .OfType<Room>()
            .Single();

        Assert.Equal(4, restoredRoom.Boundary.OuterLoop.Edges.Count);
        Assert.All(
            restoredRoom.Boundary.OuterLoop.Edges,
            edge => Assert.True(edge.IsReversed));

        Assert.Equal(
            room.WallIds,
            restoredRoom.WallIds);
    }

    [Fact]
    public async Task LoadSchema2RoomMigratesWallIdsToForwardBoundaryEdges()
    {
        const string wall1 = "11111111-1111-1111-1111-111111111111";
        const string wall2 = "22222222-2222-2222-2222-222222222222";
        const string wall3 = "33333333-3333-3333-3333-333333333333";
        const string wall4 = "44444444-4444-4444-4444-444444444444";
        const string roomId = "55555555-5555-5555-5555-555555555555";

        const string json = $$"""
        {
          "schemaVersion": 2,
          "projectId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "name": "Schema 2",
          "productDefinitions": [],
          "objects": [
            {
              "id": "{{wall1}}",
              "type": "Wall",
              "data": {
                "startX": 5000,
                "startY": 0,
                "endX": 0,
                "endY": 0,
                "thicknessMm": 120,
                "heightMm": 2700
              }
            },
            {
              "id": "{{wall2}}",
              "type": "Wall",
              "data": {
                "startX": 5000,
                "startY": 4000,
                "endX": 5000,
                "endY": 0,
                "thicknessMm": 120,
                "heightMm": 2700
              }
            },
            {
              "id": "{{wall3}}",
              "type": "Wall",
              "data": {
                "startX": 0,
                "startY": 4000,
                "endX": 5000,
                "endY": 4000,
                "thicknessMm": 120,
                "heightMm": 2700
              }
            },
            {
              "id": "{{wall4}}",
              "type": "Wall",
              "data": {
                "startX": 0,
                "startY": 0,
                "endX": 0,
                "endY": 4000,
                "thicknessMm": 120,
                "heightMm": 2700
              }
            },
            {
              "id": "{{roomId}}",
              "type": "Room",
              "data": {
                "name": "Room",
                "heightMm": 2700,
                "wallIds": [
                  "{{wall1}}",
                  "{{wall2}}",
                  "{{wall3}}",
                  "{{wall4}}"
                ]
              }
            }
          ]
        }
        """;

        var serializer = new JsonProjectSerializer();

        await using var source =
            new MemoryStream(Encoding.UTF8.GetBytes(json));

        var restored = await serializer.LoadAsync(source);
        var room = restored.Get<Room>(
            new DesignStudio.Domain.Identity.EntityId(
                Guid.Parse(roomId)));

        Assert.Equal(
            4,
            room.Boundary.OuterLoop.Edges.Count);

        Assert.All(
            room.Boundary.OuterLoop.Edges,
            edge => Assert.False(edge.IsReversed));
    }

    [Fact]
    public async Task LoadSchema1RoomMigratesThroughSchema2ToForwardBoundaryEdges()
    {
        const string wall1 = "11111111-1111-1111-1111-111111111111";
        const string wall2 = "22222222-2222-2222-2222-222222222222";
        const string wall3 = "33333333-3333-3333-3333-333333333333";
        const string wall4 = "44444444-4444-4444-4444-444444444444";
        const string roomId = "55555555-5555-5555-5555-555555555555";

        const string json = $$"""
        {
          "schemaVersion": 1,
          "projectId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "name": "Schema 1",
          "objects": [
            {
              "id": "{{wall1}}",
              "type": "Wall",
              "data": {
                "startX": 5000,
                "startY": 0,
                "endX": 0,
                "endY": 0,
                "thicknessMm": 120,
                "heightMm": 2700
              }
            },
            {
              "id": "{{wall2}}",
              "type": "Wall",
              "data": {
                "startX": 5000,
                "startY": 4000,
                "endX": 5000,
                "endY": 0,
                "thicknessMm": 120,
                "heightMm": 2700
              }
            },
            {
              "id": "{{wall3}}",
              "type": "Wall",
              "data": {
                "startX": 0,
                "startY": 4000,
                "endX": 5000,
                "endY": 4000,
                "thicknessMm": 120,
                "heightMm": 2700
              }
            },
            {
              "id": "{{wall4}}",
              "type": "Wall",
              "data": {
                "startX": 0,
                "startY": 0,
                "endX": 0,
                "endY": 4000,
                "thicknessMm": 120,
                "heightMm": 2700
              }
            },
            {
              "id": "{{roomId}}",
              "type": "Room",
              "data": {
                "name": "Room",
                "heightMm": 2700,
                "wallIds": [
                  "{{wall1}}",
                  "{{wall2}}",
                  "{{wall3}}",
                  "{{wall4}}"
                ]
              }
            }
          ]
        }
        """;

        var serializer = new JsonProjectSerializer();

        await using var source =
            new MemoryStream(Encoding.UTF8.GetBytes(json));

        var restored = await serializer.LoadAsync(source);
        var room = restored.Get<Room>(
            new DesignStudio.Domain.Identity.EntityId(
                Guid.Parse(roomId)));

        Assert.Equal(
            4,
            room.Boundary.OuterLoop.Edges.Count);

        Assert.All(
            room.Boundary.OuterLoop.Edges,
            edge => Assert.False(edge.IsReversed));
    }

    private static Wall AddWall(
        ProjectModel project,
        Point2D start,
        Point2D end)
    {
        var wall = new Wall(
            start,
            end,
            Length.FromMillimeters(120),
            Length.FromMillimeters(2700));

        project.Add(wall);
        return wall;
    }
}
