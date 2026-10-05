using DesignStudio.Domain.Project;
using DesignStudio.Domain.Units;

namespace DesignStudio.Application.Commands;

public sealed class ResizeCabinetCommand : ICommand
{
    private readonly Cabinet _cabinet;
    private readonly Length _width;
    private readonly Length _height;
    private readonly Length _depth;
    private readonly Length _backThickness;
    private readonly Length _panelThickness;
    private readonly int _shelfCount;
    private readonly int _doorCount;
    private readonly CabinetConstructionMethod _constructionMethod;

    private Length _oldWidth;
    private Length _oldHeight;
    private Length _oldDepth;
    private Length _oldBackThickness;
    private Length _oldPanelThickness;
    private int _oldShelfCount;
    private int _oldDoorCount;
    private CabinetConstructionMethod _oldConstructionMethod;

    public ResizeCabinetCommand(
        Cabinet cabinet,
        double widthMm,
        double heightMm,
        double depthMm,
        double panelThicknessMm,
        double backThicknessMm,
        int shelfCount,
        int doorCount,
        CabinetConstructionMethod constructionMethod)
    {
        _cabinet = cabinet ?? throw new ArgumentNullException(nameof(cabinet));
        _width = Length.FromMillimeters(widthMm);
        _height = Length.FromMillimeters(heightMm);
        _depth = Length.FromMillimeters(depthMm);
        _panelThickness = Length.FromMillimeters(panelThicknessMm);
        _backThickness = Length.FromMillimeters(backThicknessMm);
        _shelfCount = shelfCount;
        _doorCount = doorCount;
        _constructionMethod = constructionMethod;
    }

    public string Name => "Resize Cabinet";

    public void Execute()
    {
        CaptureOldState();
        Apply(_width, _height, _depth, _panelThickness, _backThickness, _shelfCount, _doorCount, _constructionMethod);
    }

    public void Undo()
        => Apply(_oldWidth, _oldHeight, _oldDepth, _oldPanelThickness, _oldBackThickness, _oldShelfCount, _oldDoorCount, _oldConstructionMethod);

    private void CaptureOldState()
    {
        _oldWidth = _cabinet.Width;
        _oldHeight = _cabinet.Height;
        _oldDepth = _cabinet.Depth;
        _oldPanelThickness = _cabinet.PanelThickness;
        _oldBackThickness = _cabinet.BackThickness;
        _oldShelfCount = _cabinet.ShelfCount;
        _oldDoorCount = _cabinet.DoorCount;
        _oldConstructionMethod = _cabinet.ConstructionMethod;
    }

    private void Apply(Length width, Length height, Length depth, Length panelThickness, Length backThickness,
        int shelfCount, int doorCount, CabinetConstructionMethod constructionMethod)
    {
        _cabinet.SetDimensions(width, height, depth);
        _cabinet.SetPanelThickness(panelThickness);
        _cabinet.SetBackThickness(backThickness);
        _cabinet.SetShelfCount(shelfCount);
        _cabinet.SetDoorCount(doorCount);
        _cabinet.SetConstructionMethod(constructionMethod);
    }
}
