using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pomidoras.Models.Timer;
using Pomidoras.Models.Timer.Configuration;

namespace Pomidoras.ViewModels;

public partial class TimerViewModel : ViewModelBase
{

    [ObservableProperty] private TimeSpan _remaining;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _currentModeName;
    [ObservableProperty] private bool _ending;

    private readonly TimerService _timerService;

    public TimerViewModel(TimerService timerService)
    {
        _timerService = timerService;

        Remaining = _timerService.Remaining;
        CurrentModeName = _timerService.CurrentMode.ToString();
        _timerService.RemainingChanged += OnRemainingChanged;
        _timerService.IsRunningChanged += OnIsRunningChanged;
        _timerService.ModeChanged += OnModeChanged;
    }

    [RelayCommand]
    private void StartTimer()
    {
        _timerService.Start();
    }

    [RelayCommand]
    private void StopTimer()
    {
        _timerService.Stop();
    }

    [RelayCommand]
    private void SwitchTimerModeNext()
    {
        _timerService.SwitchMode(true);
    }

    [RelayCommand]
    private void SwitchTimerModePrevious()
    {
        _timerService.SwitchMode(false);
    }

    private void OnIsRunningChanged(object? sender, bool newIsRunning)
    {
        IsRunning = newIsRunning;
    }

    private void OnRemainingChanged(object? sender, TimeSpan newRemaining)
    {
        Remaining = newRemaining;
    }

    private void OnModeChanged(object? sender, TimerMode newMode)
    {
        CurrentModeName = newMode.ToString();
    }

}