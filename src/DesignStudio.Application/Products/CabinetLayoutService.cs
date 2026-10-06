using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Application.Products;

public sealed record CabinetShelfLayout(
    int Index,
    Length X,
    Length Y,
    Length Width,
    Length Depth,
    Length Thickness);

public sealed record CabinetDoorLayout(
    int Index,
    Length X,
    Length Y,
    Length Width,
    Length Height,
    Length Thickness);

public sealed record CabinetLayout(
    Length ClearWidth,
    Length ClearHeight,
    IReadOnlyList<CabinetShelfLayout> Shelves,
    IReadOnlyList<CabinetDoorLayout> Doors);

public sealed class CabinetLayoutService
{
    public CabinetLayout Build(Cabinet cabinet)
    {
        ArgumentNullException.ThrowIfNull(cabinet);

        var panel = cabinet.PanelThickness.Millimeters;
        var width = cabinet.Width.Millimeters;
        var height = cabinet.Height.Millimeters;
        var depth = cabinet.Depth.Millimeters;

        var clearWidth = Math.Max(0, width - (2 * panel));
        var clearHeight = Math.Max(0, height - (2 * panel));

        var shelves = BuildShelves(
            clearWidth,
            clearHeight,
            depth,
            panel,
            cabinet.ShelfCount);

        var doors = BuildDoors(
            clearWidth,
            clearHeight,
            panel,
            cabinet.DoorCount);

        return new CabinetLayout(
            Length.FromMillimeters(clearWidth),
            Length.FromMillimeters(clearHeight),
            shelves,
            doors);
    }

    private static IReadOnlyList<CabinetShelfLayout> BuildShelves(
        double clearWidth,
        double clearHeight,
        double depth,
        double panel,
        int shelfCount)
    {
        if (shelfCount <= 0)
            return Array.Empty<CabinetShelfLayout>();

        var availableClearHeight =
            Math.Max(0, clearHeight - shelfCount * panel);

        var openingHeight =
            availableClearHeight / (shelfCount + 1);

        var shelves = new List<CabinetShelfLayout>(shelfCount);

        for (var i = 0; i < shelfCount; i++)
        {
            var openingIndex = i + 1;

            var y =
     panel
    + openingHeight * openingIndex
    + panel * i;

           shelves.Add(
    new CabinetShelfLayout(
        i + 1,
        Length.FromMillimeters(panel),
        Length.FromMillimeters(y),
        Length.FromMillimeters(clearWidth),
        Length.FromMillimeters(depth),
        Length.FromMillimeters(panel)));
        }

        return shelves;
    }

    private static IReadOnlyList<CabinetDoorLayout> BuildDoors(
        double clearWidth,
        double clearHeight,
        double panel,
        int doorCount)
    {
        if (doorCount <= 0)
            return Array.Empty<CabinetDoorLayout>();

        var doorWidth = clearWidth / doorCount;
        var doors = new List<CabinetDoorLayout>(doorCount);

        for (var i = 0; i < doorCount; i++)
        {
            doors.Add(
                new CabinetDoorLayout(
                    i + 1,
                    Length.FromMillimeters(panel + doorWidth * i),
                    Length.FromMillimeters(panel),
                    Length.FromMillimeters(doorWidth),
                    Length.FromMillimeters(clearHeight),
                    Length.FromMillimeters(panel)));
        }

        return doors;
    }
}
