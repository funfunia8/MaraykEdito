using DesignStudio.Application.Rooms;
using DesignStudio.Application.Commands;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Shell;

public sealed class WorkspaceController
{
    private readonly IntelligentRoomService _rooms = new();
    private readonly RoomRegenerationService _regeneration = new();
    private readonly Products.CabinetService _cabinets = new();
    private readonly Products.CabinetPlacementValidationService _cabinetPlacement = new();
    private readonly CommandHistory _history = new();

    public WorkspaceController(WorkspaceState state)
    {
        State = state;
    }

    public WorkspaceState State { get; }
    public bool CanUndo => _history.CanUndo;
    public bool CanRedo => _history.CanRedo;

    public Project CreateProject(string name)
    {
        _history.Clear();
        var project = new Project(name);
        State.SetProject(project);
        return project;
    }

    public void OpenProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);
        _history.Clear();
        State.SetProject(project);

        var room = project.Objects.OfType<Room>().FirstOrDefault();
        if (room is not null)
        {
            var derived = _regeneration.Regenerate(project, room);
            State.SetActiveRoom(room, derived);
        }
    }

    public Room CreateDefaultRoom()
    {
        var project = State.ActiveProject ?? CreateProject("Untitled Project");
        var roomIndex = project.Objects.OfType<Room>().Count();
        var roomName = roomIndex == 0 ? "Living Room" : $"Room {roomIndex + 1}";
        var originX = roomIndex * 6500.0;
        var room = _rooms.CreateRectangularRoom(project, roomName, 5000, 4000, originXmm: originX);
        var derived = _regeneration.Regenerate(project, room);
        State.SetActiveRoom(room, derived);
        State.MarkDirty();
        return room;
    }

    public IReadOnlyList<Room> GetRooms()
        => (IReadOnlyList<Room>?)State.ActiveProject?.Objects.OfType<Room>().OrderBy(x => x.Id.Value).ToList()
           ?? Array.Empty<Room>();

    public EntityId CreateCabinetInActiveRoom(
        string name = "Base Cabinet",
        double widthMm = 600,
        double heightMm = 720,
        double depthMm = 560)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = State.ActiveRoom ?? throw new InvalidOperationException("No active room.");

        var created = _cabinets.Create(project, name, widthMm, heightMm, depthMm);
        var center = GetRoomCenter(project, room);
        created.Cabinet.SetPlacement(room.Id, center);
        var placementValidation = _cabinetPlacement.Validate(project, room);
        if (!placementValidation.IsValid)
        {
            project.Remove(created.Cabinet.Id);
            throw new InvalidOperationException(string.Join(Environment.NewLine, placementValidation.Issues.Select(x => x.Message)));
        }
        State.SetDerived(_regeneration.Regenerate(project, room));
        State.MarkDirty();
        return created.Cabinet.Id;
    }

    public void MoveActiveCabinet(EntityId cabinetId, Point2D newPosition)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = State.ActiveRoom ?? throw new InvalidOperationException("No active room.");
        var cabinet = project.Get<Cabinet>(cabinetId);
        if (cabinet.HostRoomId != room.Id)
            throw new InvalidOperationException("Selected cabinet does not belong to the active room.");

        _history.Execute(new MoveCabinetCommand(project, cabinetId, newPosition));
        var placementValidation = _cabinetPlacement.Validate(project, room);
        if (!placementValidation.IsValid)
        {
            _history.Undo();
            throw new InvalidOperationException(string.Join(Environment.NewLine, placementValidation.Issues.Select(x => x.Message)));
        }
        State.SetDerived(_regeneration.Regenerate(project, room));
        State.MarkDirty();
    }

    private static Point2D GetRoomCenter(Project project, Room room)
    {
        var walls = room.WallIds.Select(project.Get<Wall>).ToList();
        var points = walls.SelectMany(w => new[] { w.Start, w.End }).ToList();
        if (points.Count == 0) return new Point2D(0, 0);
        return new Point2D(points.Average(p => p.X), points.Average(p => p.Y));
    }


    public void SelectRoom(EntityId roomId)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = project.Get<Room>(roomId);
        var derived = _regeneration.Regenerate(project, room);
        _history.Clear();
        State.SetActiveRoom(room, derived);
    }

    public void ResizeActiveWall(int wallIndex, Point2D newEnd)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = State.ActiveRoom ?? throw new InvalidOperationException("No active room.");
        var wall = project.Get<DesignStudio.Domain.Project.Wall>(room.WallIds[wallIndex]);
        _history.Execute(new ResizeWallCommand(wall, newEnd));
        var validation = _rooms.Validate(project, room);
        if (!validation.IsValid)
        {
            _history.Undo();
            throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Issues.Select(x => x.Message)));
        }
        State.SetDerived(_regeneration.Regenerate(project, room));
        State.MarkDirty();
    }

    public void MoveActiveWallEndpoint(int wallIndex, bool moveStart, Point2D newPoint)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = State.ActiveRoom ?? throw new InvalidOperationException("No active room.");
        var wall = project.Get<Wall>(room.WallIds[wallIndex]);
        var command = new MoveWallEndpointCommand(project, room, wall.Id, moveStart, newPoint);
        _history.Execute(command);
        var validation = _rooms.Validate(project, room);
        if (!validation.IsValid)
        {
            _history.Undo();
            throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Issues.Select(x => x.Message)));
        }
        State.SetDerived(_regeneration.Regenerate(project, room));
        State.MarkDirty();
    }


    public EntityId AddDoorToActiveWall(int wallIndex, double? offsetMm = null, double widthMm = 900, double heightMm = 2100)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = State.ActiveRoom ?? throw new InvalidOperationException("No active room.");
        if (wallIndex < 0 || wallIndex >= room.WallIds.Count)
            throw new ArgumentOutOfRangeException(nameof(wallIndex));

        var wall = project.Get<Wall>(room.WallIds[wallIndex]);
        var offset = offsetMm ?? Math.Max(0, (wall.LengthMm - widthMm) / 2);
        var opening = new Door(wall.Id, DesignStudio.Domain.Units.Length.FromMillimeters(offset),
            DesignStudio.Domain.Units.Length.FromMillimeters(widthMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(heightMm));

        _history.Execute(new AddOpeningCommand(project, room, opening));
        State.SetDerived(_regeneration.Regenerate(project, room));
        State.MarkDirty();
        return opening.Id;
    }

    public EntityId AddWindowToActiveWall(int wallIndex, double? offsetMm = null, double widthMm = 1200, double heightMm = 1200, double sillMm = 900)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = State.ActiveRoom ?? throw new InvalidOperationException("No active room.");
        if (wallIndex < 0 || wallIndex >= room.WallIds.Count)
            throw new ArgumentOutOfRangeException(nameof(wallIndex));

        var wall = project.Get<Wall>(room.WallIds[wallIndex]);
        var offset = offsetMm ?? Math.Max(0, (wall.LengthMm - widthMm) / 2);
        var opening = new Window(wall.Id, DesignStudio.Domain.Units.Length.FromMillimeters(offset),
            DesignStudio.Domain.Units.Length.FromMillimeters(widthMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(heightMm),
            DesignStudio.Domain.Units.Length.FromMillimeters(sillMm));

        _history.Execute(new AddOpeningCommand(project, room, opening));
        State.SetDerived(_regeneration.Regenerate(project, room));
        State.MarkDirty();
        return opening.Id;
    }

    public void ResizeActiveOpening(EntityId openingId, double widthMm, double heightMm, double? sillHeightMm)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = State.ActiveRoom ?? throw new InvalidOperationException("No active room.");

        var command = new ResizeOpeningCommand(project, openingId, widthMm, heightMm, sillHeightMm);
        _history.Execute(command);
        var validation = _rooms.Validate(project, room);
        if (!validation.IsValid)
        {
            _history.Undo();
            throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Issues.Select(x => x.Message)));
        }

        State.SetDerived(_regeneration.Regenerate(project, room));
        State.MarkDirty();
    }

    public void MoveActiveOpening(EntityId openingId, Point2D newPoint)
    {
        var project = State.ActiveProject ?? throw new InvalidOperationException("No active project.");
        var room = State.ActiveRoom ?? throw new InvalidOperationException("No active room.");
        var opening = project.TryGet(openingId, out var obj) ? obj : null;
        var hostWallId = opening switch
        {
            Door door => door.HostWallId,
            Window window => window.HostWallId,
            _ => throw new InvalidOperationException("Selected object is not an opening.")
        };
        var wall = project.Get<Wall>(hostWallId);
        var widthMm = opening switch
        {
            Door door => door.Width.Millimeters,
            Window window => window.Width.Millimeters,
            _ => 0
        };

        var dx = wall.End.X - wall.Start.X;
        var dy = wall.End.Y - wall.Start.Y;
        var lenSq = dx * dx + dy * dy;
        if (lenSq <= 0.000001) throw new InvalidOperationException("Host wall is invalid.");

        var t = ((newPoint.X - wall.Start.X) * dx + (newPoint.Y - wall.Start.Y) * dy) / lenSq;
        var offsetMm = Math.Clamp(t * Math.Sqrt(lenSq), 0, Math.Max(0, wall.LengthMm - widthMm));

        var command = new MoveOpeningCommand(project, openingId, offsetMm);
        _history.Execute(command);
        var validation = _rooms.Validate(project, room);
        if (!validation.IsValid)
        {
            _history.Undo();
            throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Issues.Select(x => x.Message)));
        }

        State.SetDerived(_regeneration.Regenerate(project, room));
        State.MarkDirty();
    }

    public void Undo()
    {
        if (!_history.CanUndo || State.ActiveProject is null || State.ActiveRoom is null) return;
        _history.Undo();
        State.SetDerived(_regeneration.Regenerate(State.ActiveProject, State.ActiveRoom));
        State.MarkDirty();
    }

    public void Redo()
    {
        if (!_history.CanRedo || State.ActiveProject is null || State.ActiveRoom is null) return;
        _history.Redo();
        State.SetDerived(_regeneration.Regenerate(State.ActiveProject, State.ActiveRoom));
        State.MarkDirty();
    }
}
