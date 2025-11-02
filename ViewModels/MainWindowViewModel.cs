using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty] private bool _alwaysOnTopEnabled;

    [ObservableProperty] private bool _splitViewPaneOpen;

    public MainWindowViewModel()
    {
        var timerConfigurationService = new TimerConfigurationService();
        var timerService = new TimerService(timerConfigurationService);
        TimerViewModel = new TimerViewModel(timerService);
    }

    public TimerViewModel TimerViewModel { get; }

    [RelayCommand]
    private void ToggleAlwaysOnTop()
    {
        AlwaysOnTopEnabled = !AlwaysOnTopEnabled;
    }

    [RelayCommand]
    private void ToggleSplitViewPane()
    {
        SplitViewPaneOpen = !SplitViewPaneOpen;
    }

}