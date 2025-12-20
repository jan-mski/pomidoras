using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty] private TimeSpan _remaining;

    public MainWindowViewModel(TimerService timerService)
    {
        Remaining = timerService.Remaining;
        timerService.RemainingChanged += OnRemainingChanged;
    }

    private void OnRemainingChanged(object? sender, TimeSpan newRemaining)
    {
        Remaining = newRemaining;
    }

}