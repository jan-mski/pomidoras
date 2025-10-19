using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pomidoras.Models;
using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty] private string _buttonText;

    public MainWindowViewModel()
    {
        var timerConfigurationService = new TimerConfigurationService();
        Timer = new TimerViewModel(timerConfigurationService);
        ButtonText = "Start";
    }

    public ITimer Timer { get; }

    [RelayCommand]
    private void StartStop()
    {
        if (Timer.IsRunning)
        {
            Timer.Stop();
            ButtonText = "Start";
        }
        else
        {
            Timer.Start();
            ButtonText = "Stop";
        }
    }

}