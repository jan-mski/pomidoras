using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pomidoras.Models.Timer;
using Pomidoras.Models.Timer.Configuration;

namespace Pomidoras.ViewModels;

public partial class TimerViewModel : ViewModelBase
{
    private const long TimerStartedAnimationDurationSeconds = 1;
    private const long TimerEndingAnimationDurationSeconds = 3;

    [ObservableProperty] private bool _continuousModeEnabled;
    [ObservableProperty] private TimeSpan _remaining;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isStarted;
    [ObservableProperty] private string _currentModeName;
    [ObservableProperty] private bool _isEnding;

    private readonly TimerService _timerService;
    private bool _wasLastModeSwitchRequestedByUser;

    public TimerViewModel(TimerService timerService)
    {
        _timerService = timerService;

        ContinuousModeEnabled = _timerService.Configuration.ContinuousModeEnabled;
        Remaining = _timerService.Remaining;
        CurrentModeName = _timerService.CurrentMode.ToString();
        _timerService.RemainingChanged += OnRemainingChanged;
        _timerService.IsRunningChanged += OnIsRunningChanged;
        _timerService.ModeChanged += OnModeChanged;
    }

    [RelayCommand]
    private void ToggleContinuousMode()
    {
        ContinuousModeEnabled = !ContinuousModeEnabled;
    }

    [RelayCommand]
    private async Task StartTimer()
    {
        await DoStartTimer();
    }

    [RelayCommand]
    private void StopTimer()
    {
        _timerService.Stop();
    }

    [RelayCommand]
    private void SwitchTimerModeNext()
    {
        _wasLastModeSwitchRequestedByUser = true;
        _timerService.SwitchMode(true);
        _wasLastModeSwitchRequestedByUser = false;
    }

    [RelayCommand]
    private void SwitchTimerModePrevious()
    {
        _wasLastModeSwitchRequestedByUser = true;
        _timerService.SwitchMode(false);
        _wasLastModeSwitchRequestedByUser = false;
    }

    private async Task DoStartTimer()
    {
        _timerService.Start();

        IsStarted = true;
        await Task.Delay(TimeSpan.FromSeconds(TimerStartedAnimationDurationSeconds));
        IsStarted = false;
    }

    private void OnIsRunningChanged(object? sender, bool newIsRunning)
    {
        IsRunning = newIsRunning;
        IsEnding = false;
    }

    private void OnRemainingChanged(object? sender, TimeSpan newRemaining)
    {
        Remaining = newRemaining;
        if (Remaining.CompareTo(TimeSpan.FromSeconds(TimerEndingAnimationDurationSeconds)) <= 0)
        {
            IsEnding = true;
        }
    }

    private async void OnModeChanged(object? sender, TimerMode newMode)
    {
        CurrentModeName = newMode.ToString();

        if (ContinuousModeEnabled && !_wasLastModeSwitchRequestedByUser)
        {
            await DoStartTimer();
        }
    }
}