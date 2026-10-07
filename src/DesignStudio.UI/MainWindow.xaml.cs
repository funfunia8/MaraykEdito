using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using DesignStudio.Domain.Identity;
using DesignStudio.Localization;

namespace DesignStudio.UI;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _recoveryTimer;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.MainViewModel;
        _recoveryTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _recoveryTimer.Tick += async (_, _) => await AutoSaveRecoveryAsync();
        _recoveryTimer.Start();
        Loaded += MainWindow_Loaded;
        Closed += (_, _) => _recoveryTimer.Stop();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        if (!App.MainViewModel.HasRecoverySnapshot) return;

        var timestamp =
            App.MainViewModel.RecoverySnapshotTime?.ToLocalTime().ToString("g")
            ?? string.Empty;

        var dialog = new RecoveryDialog(
            App.MainViewModel.RecoveryDialogTitle,
            App.MainViewModel.RecoveryAvailableText,
            App.MainViewModel.RecoverySnapshotPathForDisplay(),
            timestamp,
            App.MainViewModel.RecoveryRestoreText,
            App.MainViewModel.RecoveryDiscardText,
            App.MainViewModel.CurrentLanguage == DesignStudio.Localization.Language.Arabic
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight)
        {
            Owner = this
        };

        dialog.ShowDialog();

        if (dialog.Result == RecoveryDialogResult.Restore)
        {
            try
            {
                await App.MainViewModel.RecoverProjectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    App.MainViewModel.RecoveryErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        else if (dialog.Result == RecoveryDialogResult.Discard)
        {
            App.MainViewModel.ClearRecoverySnapshot();
        }
    }

    private async Task AutoSaveRecoveryAsync()
    {
        try { await App.MainViewModel.AutoSaveRecoveryAsync(); }
        catch { /* Recovery must never crash the UI. */ }
    }


    private async void SaveProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Design Studio Project (*.design)|*.design|JSON Project (*.json)|*.json",
            DefaultExt = ".design",
            AddExtension = true,
            FileName = "Untitled.design"
        };
        if (dialog.ShowDialog() != true) return;
        try
        {
            await App.MainViewModel.SaveProjectAsync(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Design Studio", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OpenProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Design Studio Project (*.design;*.json)|*.design;*.json|All Files (*.*)|*.*",
            Multiselect = false
        };
        if (dialog.ShowDialog() != true) return;
        try
        {
            await App.MainViewModel.OpenProjectAsync(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Design Studio", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void NewProject_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.CreateNewProject();

    private void CreateRoom_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.CreateRoom();

    private void RoomsComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (RoomsComboBox.SelectedValue is EntityId id)
            App.MainViewModel.SelectRoom(id);
    }

    private void English_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.SetLanguage(DesignStudio.Localization.Language.English);

    private void Arabic_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.SetLanguage(DesignStudio.Localization.Language.Arabic);

    private void RoomCanvas_WallSelected(object? sender, int wallIndex)
        => App.MainViewModel.SelectWall(wallIndex);

    private void RoomCanvas_EndpointDragged(object? sender, EndpointDragEventArgs e)
        => App.MainViewModel.MoveSelectedWallEndpoint(e.MoveStart, e.ModelPoint);

    private void RoomCanvas_OpeningSelected(object? sender, EntityId openingId)
        => App.MainViewModel.SelectOpening(openingId);

    private void RoomCanvas_OpeningDragged(object? sender, OpeningDragEventArgs e)
        => App.MainViewModel.MoveSelectedOpening(e.ModelPoint);

    private void RoomCanvas_CabinetSelected(object? sender, EntityId cabinetId)
        => App.MainViewModel.SelectCabinet(cabinetId);

    private void RoomCanvas_CabinetDragged(object? sender, CabinetDragEventArgs e)
        => App.MainViewModel.MoveSelectedCabinet(e.ModelPoint);

    private void ExtendWall_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.ExtendSelectedWall();

    private void ShrinkWall_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.ShrinkSelectedWall();

    private void Undo_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.Undo();

    private void Redo_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.Redo();

    private void ApplyOpeningDimensions_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.ApplySelectedOpeningDimensions();

    private void AddDoor_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.AddDoor();

    private void AddWindow_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.AddWindow();

    private void AddCabinet_Click(object sender, RoutedEventArgs e)
        => App.MainViewModel.AddCabinet();
}
