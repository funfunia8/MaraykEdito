using System.Windows;

namespace DesignStudio.UI;

public enum RecoveryDialogResult
{
    None,
    Restore,
    Discard
}

public partial class RecoveryDialog : Window
{
    public RecoveryDialogResult Result { get; private set; } = RecoveryDialogResult.None;

    public RecoveryDialog(
        string title,
        string message,
        string path,
        string timestamp,
        string restoreText,
        string discardText,
        FlowDirection flowDirection)
    {
        InitializeComponent();

        Title = title;
        FlowDirection = flowDirection;

        MessageTextBlock.Text = message;
        PathTextBlock.Text = path;
        TimestampTextBlock.Text = timestamp;

        RestoreButton.Content = restoreText;
        DiscardButton.Content = discardText;
    }

    private void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
        Result = RecoveryDialogResult.Restore;
        DialogResult = true;
    }

    private void DiscardButton_Click(object sender, RoutedEventArgs e)
    {
        Result = RecoveryDialogResult.Discard;
        DialogResult = false;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (Result == RecoveryDialogResult.None)
        {
            e.Cancel = true;
            return;
        }

        base.OnClosing(e);
    }
}
