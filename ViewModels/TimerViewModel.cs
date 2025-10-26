using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public partial class TimerViewModel : ViewModelBase
{

    private readonly ITimerService _timerService;

    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private TimeSpan _remaining;

    public TimerViewModel(ITimerService timerService)
    {
        _timerService = timerService;
        Remaining = _timerService.Remaining;
        _timerService.RemainingChanged += OnRemainingChanged;
        _timerService.IsRunningChanged += OnIsRunningChanged;
    }


    public void Start()
    {
        _timerService.Start();
    }

    public void Stop()
    {
        _timerService.Stop();
    }

    private void OnRemainingChanged(object? sender, TimeSpan newValue)
    {
        Remaining = newValue;
        OnPropertyChanged(nameof(Remaining));
    }

    private void OnIsRunningChanged(object? sender, bool newValue)
    {
        IsRunning = newValue;
        OnPropertyChanged(nameof(IsRunning));
    }

}