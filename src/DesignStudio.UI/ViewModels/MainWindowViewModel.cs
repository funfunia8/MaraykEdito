using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using DesignStudio.Domain.Identity;
using DesignStudio.Domain.Project;
using DesignStudio.Application.Shell;
using DesignStudio.Geometry.Derived;
using DesignStudio.Localization;
using DesignStudio.Storage;
using DesignStudio.Application.Persistence;


namespace DesignStudio.UI.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly WorkspaceController _workspace;
    private readonly ILocalizationService _localization;
    private readonly IProjectSerializer _serializer;
    private readonly ProjectRecoveryCoordinator _recoveryCoordinator;
    private readonly ObservableCollection<RoomListItem> _rooms = new();
    private string _statusOverride = string.Empty;

    public MainWindowViewModel(WorkspaceController workspace, ILocalizationService localization, IProjectSerializer serializer, ProjectRecoveryCoordinator recoveryCoordinator)
    {
        _workspace = workspace;
        _localization = localization;
        _serializer = serializer;
        _recoveryCoordinator = recoveryCoordinator;
        Rooms = new ReadOnlyObservableCollection<RoomListItem>(_rooms);
        _localization.LanguageChanged += (_, _) => RefreshTexts();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Language CurrentLanguage => _localization.CurrentLanguage;
    public string Title => _localization.Get("app.title");
    public string Subtitle => _localization.Get("app.subtitle");
    public string NewProjectText => _localization.Get("project.new");
    public string OpenProjectText => _localization.Get("project.open");
    public string SaveProjectText => _localization.Get("project.save");
    public string CreateRoomText => _localization.Get("room.create");
    public string RoomsText => _localization.Get("workspace.rooms");
    public string WallsText => _localization.Get("workspace.walls");
    public string DoorsText => _localization.Get("workspace.doors");
    public string WindowsText => _localization.Get("workspace.windows");
    public string DesignText => _localization.Get("workspace.design");
    public string ModelText => _localization.Get("workspace.model");
    public string DocumentsText => _localization.Get("workspace.documents");
    public string PropertiesText => _localization.Get("workspace.properties");
    public string RelationshipsText => _localization.Get("workspace.relationships");
    public string ValidationText => _localization.Get("workspace.validation");
    public string DesignViewText => _localization.Get("workspace.designView");
    public string ExtendText => _localization.Get("edit.extend");
    public string ShrinkText => _localization.Get("edit.shrink");
    public string UndoText => _localization.Get("edit.undo");
    public string RedoText => _localization.Get("edit.redo");
    public string SelectedWallText => _localization.Get("workspace.selectedWall");
    public string LengthText => _localization.Get("workspace.length");
    public string SelectedOpeningText => _localization.Get("workspace.selectedOpening");
    public string OpeningKindText => _localization.Get("workspace.openingKind");
    public string OffsetText => _localization.Get("workspace.offset");
    public string WidthText => _localization.Get("workspace.width");
    public string HeightText => _localization.Get("workspace.height");
    public string SillText => _localization.Get("workspace.sill");
    public string ApplyOpeningText => _localization.Get("edit.applyOpening");
    public string AddDoorText => _localization.Get("edit.addDoor");
    public string AddWindowText => _localization.Get("edit.addWindow");
    public string AddCabinetText => _localization.Get("edit.addCabinet");
    public string SelectedCabinetText => _localization.Get("workspace.selectedCabinet");
    public string CabinetPositionText => _localization.Get("workspace.cabinetPosition");
    public string CabinetWidthText => _localization.Get("workspace.cabinetWidth");
    public string CabinetDepthText => _localization.Get("workspace.cabinetDepth");
    public string CabinetRotationText => _localization.Get("workspace.cabinetRotation");

    private string _openingWidthInput = string.Empty;
    private string _openingHeightInput = string.Empty;
    private string _openingSillInput = string.Empty;

    public string OpeningWidthInput { get => _openingWidthInput; set { _openingWidthInput = value; OnPropertyChanged(); } }
    public string OpeningHeightInput { get => _openingHeightInput; set { _openingHeightInput = value; OnPropertyChanged(); } }
    public string OpeningSillInput { get => _openingSillInput; set { _openingSillInput = value; OnPropertyChanged(); } }

    public bool IsOpeningSelected => SelectedOpeningId is not null;
    public bool IsDirty => _workspace.State.IsDirty;
    public bool HasRecoverySnapshot => _recoveryCoordinator.HasRecoverySnapshot;
    public DateTimeOffset? RecoverySnapshotTime => _recoveryCoordinator.RecoverySnapshotTime;
    public string RecoverySnapshotPathForDisplay() => _recoveryCoordinator.RecoveryPath;
    public string RecoveryDialogTitle => _localization.Get("recovery.title");
    public string RecoveryAvailableText => _localization.Get("recovery.available");
    public string RecoveryRestoreText => _localization.Get("recovery.restore");
    public string RecoveryDiscardText => _localization.Get("recovery.discard");
    public string RecoveryErrorTitle => _localization.Get("recovery.errorTitle");
    public string? FilePath => _workspace.State.FilePath;
    public int SelectedWallIndex { get; private set; }
    public EntityId? SelectedOpeningId { get; private set; }
    public EntityId? SelectedCabinetId { get; private set; }
    public ReadOnlyObservableCollection<RoomListItem> Rooms { get; } = null!;
    public EntityId? SelectedRoomId => _workspace.State.ActiveRoom?.Id;

    public string? SelectedOpeningKind
    {
        get
        {
            if (SelectedOpeningId is not EntityId id) return null;
            var project = _workspace.State.ActiveProject;
            if (project is null || !project.TryGet(id, out var obj)) return null;
            return obj switch
            {
                Door => _localization.Get("opening.door"),
                Window => _localization.Get("opening.window"),
                _ => null
            };
        }
    }

    public double? SelectedOpeningOffsetMm => GetSelectedOpening()?.OffsetMm;
    public double? SelectedOpeningWidthMm => GetSelectedOpening()?.WidthMm;
    public double? SelectedWallLengthMm
    {
        get
        {
            var room = _workspace.State.ActiveRoom;
            var project = _workspace.State.ActiveProject;
            if (room is null || project is null || SelectedWallIndex < 0 || SelectedWallIndex >= room.WallIds.Count) return null;
            return project.Get<Wall>(room.WallIds[SelectedWallIndex]).LengthMm;
        }
    }

    public bool IsCabinetSelected => SelectedCabinetId is not null;
    public double? SelectedCabinetWidthMm => GetSelectedCabinet()?.Width.Millimeters;
    public double? SelectedCabinetDepthMm => GetSelectedCabinet()?.Depth.Millimeters;
    public double? SelectedCabinetRotationDegrees => GetSelectedCabinet()?.RotationDegrees;
    public string SelectedCabinetPositionText
    {
        get
        {
            var cabinet = GetSelectedCabinet();
            return cabinet is null ? string.Empty : $"{cabinet.Position.X:0} , {cabinet.Position.Y:0} mm";
        }
    }

    public string StatusText => !string.IsNullOrWhiteSpace(_statusOverride)
        ? _statusOverride
        : (_workspace.State.ActiveRoom is null ? _localization.Get("status.ready") : _workspace.State.ActiveRoom.Name);
    public RoomPlan2D? ActivePlan => _workspace.State.ActiveRoomDerived?.Plan2D;

    public void CreateNewProject()
    {
        _workspace.CreateProject("Untitled Project");
        SyncRooms();
        ResetSelection();
        _statusOverride = string.Empty;
        RefreshState();
    }

    public async Task SaveProjectAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var project = _workspace.State.ActiveProject;
        if (project is null)
        {
            SetStatusOverride(_localization.Get("status.noProject"));
            RefreshState();
            return;
        }

        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException("A valid file path is required.");

        Directory.CreateDirectory(directory);
        var tempPath = Path.Combine(directory, $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
            {
                await _serializer.SaveAsync(project, stream, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(tempPath, filePath, overwrite: true);
            _workspace.State.SetFilePath(filePath);
            _workspace.State.MarkSaved();
            _recoveryCoordinator.ClearAfterSuccessfulSaveOrOpen();
            _statusOverride = _localization.Get("status.saved");
            RefreshState();
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    public async Task OpenProjectAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.SequentialScan);
        var project = await _serializer.LoadAsync(stream, cancellationToken);
        _workspace.OpenProject(project);
        _workspace.State.SetFilePath(filePath);
        _workspace.State.MarkSaved();
        _recoveryCoordinator.ClearAfterSuccessfulSaveOrOpen();
        SyncRooms();
        ResetSelection();
        _statusOverride = _localization.Get("status.opened");
        RefreshState();
    }

    public void CreateRoom()
    {
        _workspace.CreateDefaultRoom();
        SyncRooms();
        ResetSelection();
        _statusOverride = string.Empty;
        RefreshState();
    }

    public void SelectRoom(EntityId roomId)
    {
        try
        {
            _workspace.SelectRoom(roomId);
            ResetSelection();
            _statusOverride = string.Empty;
            RefreshState();
        }
        catch (InvalidOperationException)
        {
            SetStatusOverride(_localization.Get("status.editRejected"));
            RefreshState();
        }
    }

    public void SetLanguage(Language language) => _localization.SetLanguage(language);

    public async Task AutoSaveRecoveryAsync(CancellationToken cancellationToken = default)
    {
        var saved = await _recoveryCoordinator.AutoSaveAsync(cancellationToken);
        if (!saved) return;

        _statusOverride = _localization.Get("recovery.saved");
        RefreshState();
    }

    public async Task<bool> RecoverProjectAsync(CancellationToken cancellationToken = default)
    {
        var recovered = await _recoveryCoordinator.RecoverAsync(cancellationToken);
        if (!recovered) return false;

        SyncRooms();
        ResetSelection();
        _statusOverride = _localization.Get("recovery.restored");
        RefreshState();
        return true;
    }

    public void ClearRecoverySnapshot()
    {
        _recoveryCoordinator.ClearRecoverySnapshot();
        _statusOverride = _localization.Get("recovery.cleared");
        RefreshState();
    }

    public void SelectWall(int index)
    {
        if (_workspace.State.ActiveRoom is null) return;
        if (index < 0 || index >= _workspace.State.ActiveRoom.WallIds.Count) return;
        SelectedWallIndex = index;
        SelectedOpeningId = null;
        SelectedCabinetId = null;
        OnPropertyChanged(nameof(SelectedWallIndex));
        OnPropertyChanged(nameof(SelectedOpeningId));
        OnPropertyChanged(nameof(SelectedWallLengthMm));
        OnPropertyChanged(nameof(StatusText));
    }

    public void ExtendSelectedWall() => ResizeSelectedWall(500);
    public void ShrinkSelectedWall() => ResizeSelectedWall(-500);

    public void AddDoor()
    {
        try { SelectOpening(_workspace.AddDoorToActiveWall(SelectedWallIndex)); _statusOverride = string.Empty; RefreshState(); }
        catch (InvalidOperationException) { SetStatusOverride(_localization.Get("status.editRejected")); RefreshState(); }
    }

    public void AddWindow()
    {
        try { SelectOpening(_workspace.AddWindowToActiveWall(SelectedWallIndex)); _statusOverride = string.Empty; RefreshState(); }
        catch (InvalidOperationException) { SetStatusOverride(_localization.Get("status.editRejected")); RefreshState(); }
    }

    public void AddCabinet()
    {
        try
        {
            SelectCabinet(_workspace.CreateCabinetInActiveRoom());
            _statusOverride = string.Empty;
            RefreshState();
        }
        catch (InvalidOperationException)
        {
            SetStatusOverride(_localization.Get("status.editRejected"));
            RefreshState();
        }
    }

    public void MoveSelectedWallEndpoint(bool moveStart, DesignStudio.Domain.Geometry.Point2D modelPoint)
    {
        try { _workspace.MoveActiveWallEndpoint(SelectedWallIndex, moveStart, modelPoint); _statusOverride = string.Empty; RefreshState(); }
        catch (InvalidOperationException) { SetStatusOverride(_localization.Get("status.editRejected")); RefreshState(); }
    }

    public void SelectCabinet(EntityId cabinetId)
    {
        var project = _workspace.State.ActiveProject;
        if (project is null || !project.TryGet(cabinetId, out var obj) || obj is not Cabinet) return;
        SelectedCabinetId = cabinetId;
        SelectedOpeningId = null;
        SelectedWallIndex = -1;
        _statusOverride = string.Empty;
        OnPropertyChanged(nameof(SelectedCabinetId));
        OnPropertyChanged(nameof(SelectedOpeningId));
        OnPropertyChanged(nameof(SelectedWallIndex));
        RefreshState();
    }

    public void MoveSelectedCabinet(DesignStudio.Domain.Geometry.Point2D modelPoint)
    {
        if (SelectedCabinetId is not EntityId id) return;
        try
        {
            _workspace.MoveActiveCabinet(id, modelPoint);
            _statusOverride = string.Empty;
            RefreshState();
        }
        catch (InvalidOperationException)
        {
            SetStatusOverride(_localization.Get("status.editRejected"));
            RefreshState();
        }
    }

    public void SelectOpening(EntityId openingId)
    {
        var project = _workspace.State.ActiveProject;
        if (project is null || !project.TryGet(openingId, out var obj) || obj is not Door && obj is not Window) return;
        SelectedOpeningId = openingId;
        SelectedCabinetId = null;
        _statusOverride = string.Empty;
        OnPropertyChanged(nameof(SelectedOpeningId));
        OnPropertyChanged(nameof(SelectedCabinetId));
        OnPropertyChanged(nameof(SelectedOpeningKind));
        OnPropertyChanged(nameof(SelectedOpeningOffsetMm));
        OnPropertyChanged(nameof(SelectedOpeningWidthMm));
        OnPropertyChanged(nameof(StatusText));
        RefreshState();
    }

    public void MoveSelectedOpening(DesignStudio.Domain.Geometry.Point2D modelPoint)
    {
        if (SelectedOpeningId is not EntityId id) return;
        try { _workspace.MoveActiveOpening(id, modelPoint); _statusOverride = string.Empty; RefreshState(); }
        catch (InvalidOperationException) { SetStatusOverride(_localization.Get("status.editRejected")); RefreshState(); }
    }

    public void ApplySelectedOpeningDimensions()
    {
        if (SelectedOpeningId is not EntityId id) return;
        if (!TryParseMillimeters(OpeningWidthInput, out var widthMm) || !TryParseMillimeters(OpeningHeightInput, out var heightMm))
        {
            SetStatusOverride(_localization.Get("status.invalidDimension")); RefreshState(); return;
        }

        double? sillMm = null;
        var project = _workspace.State.ActiveProject;
        var isWindow = project is not null && project.TryGet(id, out var selectedObject) && selectedObject is Window;
        if (isWindow)
        {
            if (!TryParseMillimeters(OpeningSillInput, out var parsedSill))
            {
                SetStatusOverride(_localization.Get("status.invalidDimension")); RefreshState(); return;
            }
            sillMm = parsedSill;
        }

        try { _workspace.ResizeActiveOpening(id, widthMm, heightMm, sillMm); _statusOverride = string.Empty; RefreshState(); }
        catch (InvalidOperationException) { SetStatusOverride(_localization.Get("status.editRejected")); RefreshState(); }
    }

    public void Undo() { _workspace.Undo(); _statusOverride = string.Empty; RefreshState(); }
    public void Redo() { _workspace.Redo(); _statusOverride = string.Empty; RefreshState(); }

    private void ResizeSelectedWall(double deltaMm)
    {
        var room = _workspace.State.ActiveRoom;
        var project = _workspace.State.ActiveProject;
        if (room is null || project is null || SelectedWallIndex < 0 || SelectedWallIndex >= room.WallIds.Count) return;
        var wall = project.Get<Wall>(room.WallIds[SelectedWallIndex]);
        var dx = wall.End.X - wall.Start.X;
        var dy = wall.End.Y - wall.Start.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);
        if (length < 1) return;
        var end = new DesignStudio.Domain.Geometry.Point2D(wall.End.X + dx / length * deltaMm, wall.End.Y + dy / length * deltaMm);
        try { _workspace.ResizeActiveWall(SelectedWallIndex, end); _statusOverride = string.Empty; RefreshState(); }
        catch (InvalidOperationException) { SetStatusOverride(_localization.Get("status.editRejected")); RefreshState(); }
    }

    private Cabinet? GetSelectedCabinet()
    {
        if (SelectedCabinetId is not EntityId id) return null;
        var project = _workspace.State.ActiveProject;
        if (project is null || !project.TryGet(id, out var obj)) return null;
        return obj as Cabinet;
    }

    private (double OffsetMm, double WidthMm)? GetSelectedOpening()
    {
        if (SelectedOpeningId is not EntityId id) return null;
        var project = _workspace.State.ActiveProject;
        if (project is null || !project.TryGet(id, out var obj)) return null;
        return obj switch { Door door => (door.OffsetFromWallStart.Millimeters, door.Width.Millimeters), Window window => (window.OffsetFromWallStart.Millimeters, window.Width.Millimeters), _ => null };
    }

    private (double WidthMm, double HeightMm, double? SillMm)? GetSelectedOpeningDetails()
    {
        if (SelectedOpeningId is not EntityId id) return null;
        var project = _workspace.State.ActiveProject;
        if (project is null || !project.TryGet(id, out var obj)) return null;
        return obj switch { Door door => (door.Width.Millimeters, door.Height.Millimeters, null), Window window => (window.Width.Millimeters, window.Height.Millimeters, window.SillHeight.Millimeters), _ => null };
    }

    private void ResetSelection()
    {
        SelectedWallIndex = 0;
        SelectedOpeningId = null;
        SelectedCabinetId = null;
        OnPropertyChanged(nameof(SelectedWallIndex));
        OnPropertyChanged(nameof(SelectedOpeningId));
        OnPropertyChanged(nameof(SelectedCabinetId));
    }

    private void SetStatusOverride(string message) => _statusOverride = message;

    private static bool TryParseMillimeters(string text, out double value)
        => double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private void SyncRooms()
    {
        var current = _workspace.GetRooms().Select(x => new RoomListItem(x.Id, x.Name)).ToList();
        if (_rooms.SequenceEqual(current)) return;
        _rooms.Clear();
        foreach (var room in current) _rooms.Add(room);
    }

    private void RefreshState()
    {
        OnPropertyChanged(nameof(ActivePlan));
        OnPropertyChanged(nameof(SelectedRoomId));
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(FilePath));
        OnPropertyChanged(nameof(SelectedWallLengthMm));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(SelectedOpeningKind));
        OnPropertyChanged(nameof(SelectedOpeningOffsetMm));
        OnPropertyChanged(nameof(SelectedOpeningWidthMm));
        OnPropertyChanged(nameof(IsOpeningSelected));
        OnPropertyChanged(nameof(SelectedCabinetWidthMm));
        OnPropertyChanged(nameof(SelectedCabinetDepthMm));
        OnPropertyChanged(nameof(SelectedCabinetRotationDegrees));
        OnPropertyChanged(nameof(SelectedCabinetPositionText));
        OnPropertyChanged(nameof(IsCabinetSelected));

        var selected = GetSelectedOpeningDetails();
        OpeningWidthInput = selected?.WidthMm.ToString("0.###", CultureInfo.InvariantCulture) ?? string.Empty;
        OpeningHeightInput = selected?.HeightMm.ToString("0.###", CultureInfo.InvariantCulture) ?? string.Empty;
        OpeningSillInput = selected?.SillMm?.ToString("0.###", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private void RefreshTexts()
    {
        foreach (var property in new[]
        {
            nameof(CurrentLanguage), nameof(Title), nameof(Subtitle), nameof(NewProjectText), nameof(OpenProjectText), nameof(SaveProjectText), nameof(CreateRoomText), nameof(RoomsText), nameof(WallsText), nameof(DoorsText), nameof(WindowsText), nameof(DesignText), nameof(ModelText), nameof(DocumentsText), nameof(PropertiesText), nameof(RelationshipsText), nameof(ValidationText), nameof(DesignViewText), nameof(ExtendText), nameof(ShrinkText), nameof(AddDoorText), nameof(AddWindowText), nameof(UndoText), nameof(RedoText), nameof(SelectedWallText), nameof(LengthText), nameof(StatusText), nameof(SelectedOpeningText), nameof(OpeningKindText), nameof(OffsetText), nameof(WidthText), nameof(SelectedOpeningKind), nameof(HeightText), nameof(SillText), nameof(ApplyOpeningText), nameof(IsOpeningSelected), nameof(AddCabinetText), nameof(SelectedCabinetText), nameof(CabinetPositionText), nameof(CabinetWidthText), nameof(CabinetDepthText), nameof(CabinetRotationText), nameof(IsCabinetSelected), nameof(HasRecoverySnapshot), nameof(RecoverySnapshotTime), nameof(RecoveryDialogTitle), nameof(RecoveryAvailableText), nameof(RecoveryRestoreText), nameof(RecoveryDiscardText), nameof(RecoveryErrorTitle)
        }) OnPropertyChanged(property);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


public sealed record RoomListItem(EntityId Id, string Name);
