using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Pomidoras.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private bool _settingsPaneOpen;
    [ObservableProperty] private bool _alwaysOnTop;

    public TimerViewModel TimerViewModel { get; }

    public MainWindowViewModel(TimerViewModel timerViewModel)
    {
        TimerViewModel = timerViewModel;
    }

    [RelayCommand]
    private void ToggleSettingsPane()
    {
        SettingsPaneOpen = !SettingsPaneOpen;
    }

    [RelayCommand]
    private void ToggleAlwaysOnTop()
    {
        AlwaysOnTop = !AlwaysOnTop;
    }
}