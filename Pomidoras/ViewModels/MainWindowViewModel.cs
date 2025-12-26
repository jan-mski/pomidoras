using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pomidoras.Models.Timer;

namespace Pomidoras.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty] private TimeSpan _remaining;
    [ObservableProperty] private bool _isRunning;
    private readonly TimerService _timerService;

    public MainWindowViewModel(TimerService timerService)
    {
        _timerService = timerService;

        Remaining = _timerService.Remaining;
        _timerService.RemainingChanged += OnRemainingChanged;
        _timerService.IsRunningChanged += OnIsRunningChanged;
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

}