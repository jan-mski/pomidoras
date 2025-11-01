using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public class MainWindowViewModel : ViewModelBase
{

    public MainWindowViewModel()
    {
        // TODO: replace with dependency injection
        var timerConfigurationService = new TimerConfigurationService();
        var timerService = new TimerService(timerConfigurationService);
        TimerViewModel = new TimerViewModel(timerService);
    }

    public TimerViewModel TimerViewModel { get; }

}