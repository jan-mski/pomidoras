using CommunityToolkit.Mvvm.Input;
using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    public MainWindowViewModel()
    {
        // TODO: replace with dependency injection
        var timerConfigurationService = new TimerConfigurationService();
        var timerService = new TimerService(timerConfigurationService);
        TimerViewModel = new TimerViewModel(timerService);
    }

    public TimerViewModel TimerViewModel { get; }

    // TODO: seems like it is unnecessarily convoluting the button text and button action? but maybe its ok?
    //  or maybe it should be a separate control type!
    [RelayCommand]
    private void StartStop()
    {
        if (TimerViewModel.IsRunning)
        {
            TimerViewModel.Stop();
        }
        else
        {
            TimerViewModel.Start();
        }
    }

}