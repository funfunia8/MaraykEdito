using DesignStudio.Application.Rooms;
using DesignStudio.Domain.Project;

namespace DesignStudio.Application.Shell;

public sealed class WorkspaceState
{
    public Project? ActiveProject { get; private set; }
    public Room? ActiveRoom { get; private set; }
    public RoomDerivedState? ActiveRoomDerived { get; private set; }
    public bool IsDirty { get; private set; }
    public string? FilePath { get; private set; }

    public void SetProject(Project project)
    {
        ActiveProject = project;
        ActiveRoom = null;
        ActiveRoomDerived = null;
        IsDirty = false;
        FilePath = null;
    }

    public void SetActiveRoom(Room room, RoomDerivedState derived)
    {
        ActiveRoom = room;
        ActiveRoomDerived = derived;
    }

    public void SetDerived(RoomDerivedState derived) => ActiveRoomDerived = derived;
    public void MarkDirty() => IsDirty = true;
    public void MarkSaved() => IsDirty = false;
    public void SetFilePath(string? filePath) => FilePath = filePath;
}
