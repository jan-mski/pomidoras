using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Pomidoras.Models;

namespace Pomidoras.ViewModels;

public partial class TimerViewModel : ViewModelBase, ITimer
{

    private readonly TimerMode _currentMode;
    private readonly DispatcherTimer _dispatcherTimer;
    private readonly TimerConfigurationService _timerConfigurationService;

    [ObservableProperty] private TimeSpan _duration;
    [ObservableProperty] private TimeSpan _remaining;

    public TimerViewModel(TimerConfigurationService timerConfigurationService)
    {
        _timerConfigurationService = timerConfigurationService;
        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();

        _currentMode = timerConfiguration.DefaultMode;
        Remaining = timerConfiguration.GetDuration(_currentMode);
        _dispatcherTimer = CreateDispatcherTimer(timerConfiguration.Interval);
    }

    public bool IsRunning => _dispatcherTimer.IsEnabled;

    public void Start()
    {
        _dispatcherTimer.Start();
    }

    public void Stop()
    {
        _dispatcherTimer.Stop();
        Remaining = TimeSpan.FromMinutes(25);
    }

    private DispatcherTimer CreateDispatcherTimer(TimeSpan timerInterval)
    {
        var dispatcherTimer = new DispatcherTimer { Interval = timerInterval };
        dispatcherTimer.Tick += (_, _) =>
        {
            Remaining -= dispatcherTimer.Interval;
            if (Remaining.Equals(TimeSpan.Zero)) dispatcherTimer.Stop();
        };
        return dispatcherTimer;
    }

}