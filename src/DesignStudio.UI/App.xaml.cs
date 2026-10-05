using System.Windows;
using DesignStudio.Application.Shell;
using DesignStudio.Localization;
using DesignStudio.Storage;
using DesignStudio.Storage.Recovery;
using DesignStudio.UI.ViewModels;

namespace DesignStudio.UI;

public partial class App : System.Windows.Application
{
    public static MainWindowViewModel MainViewModel { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var localization = new LocalizationService();
        var state = new WorkspaceState();
        var workspace = new WorkspaceController(state);

        var serializer = new JsonProjectSerializer();
        var recoveryStore = new ProjectRecoveryStore(serializer);
        MainViewModel = new MainWindowViewModel(workspace, localization, serializer, recoveryStore);
    }
}
