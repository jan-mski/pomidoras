using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public partial class TimerViewModel : ViewModelBase
{

    private readonly ITimerService _timerService;
    [ObservableProperty] private TimerMode _currentTimerMode;
    [ObservableProperty] private bool _ending;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private TimeSpan _remaining;

    public TimerViewModel(ITimerService timerService)
    {
        _timerService = timerService;
        Remaining = _timerService.Remaining;
        _timerService.RemainingChanged += OnRemainingChanged;
        _timerService.IsRunningChanged += OnIsRunningChanged;
        _timerService.CurrentModeChanged += OnCurrentModeChanged;
    }

    [RelayCommand]
    private void SwitchTimerModeNext()
    {
        _timerService.SwitchModeNext();
    }

    [RelayCommand]
    private void SwitchTimerModePrevious()
    {
        _timerService.SwitchModePrevious();
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

    private void OnRemainingChanged(object? sender, TimeSpan newValue)
    {
        Remaining = newValue;
        if (Remaining.CompareTo(TimeSpan.FromSeconds(15)) <= 0) Ending = true;
    }

    private void OnIsRunningChanged(object? sender, bool newValue)
    {
        IsRunning = newValue;
        Ending = false;
    }

    private void OnCurrentModeChanged(object? sender, TimerMode newValue)
    {
        Remaining = _timerService.Remaining;
        CurrentTimerMode = newValue;
        OnPropertyChanged(nameof(CurrentTimerMode));
    }

}