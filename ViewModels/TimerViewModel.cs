using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public partial class TimerViewModel : ViewModelBase
{

    private readonly ITimerService _timerService;

    [ObservableProperty] private TimeSpan _remaining;

    public TimerViewModel(ITimerService timerService)
    {
        _timerService = timerService;
        Remaining = _timerService.Remaining;
        _timerService.RemainingChanged += OnRemainingChanged;
    }

    public bool IsRunning => _timerService.IsRunning;

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

}