using System.Windows;
using System.Windows.Input;

namespace DesignStudio.UI;

public partial class MainWindow
{
    private void MainWindow_ProductionPreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.D)
            return;

        if (Keyboard.Modifiers != (ModifierKeys.Control | ModifierKeys.Shift))
            return;

        e.Handled = true;

        var window = new ProductionDrawingWindow
        {
            Owner = this
        };

        window.ShowDialog();
    }
}
