using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pomidoras.Models.Timer;

public interface ITimerService
{

    TimerMode CurrentMode { get; }

    TimeSpan Remaining { get; }

    bool IsRunning { get; }

    void Start();

    void Stop();

    void SwitchModeNext();
    void SwitchModePrevious();

    event EventHandler<TimeSpan> RemainingChanged;
    event EventHandler<bool> IsRunningChanged;
    event EventHandler<TimerMode> CurrentModeChanged;

}

public sealed class TimerService : ITimerService, IDisposable
{

    private readonly TimerConfigurationService _timerConfigurationService;
    private CancellationTokenSource? _cancellationTokenSource;
    private Timer _timer;

    public TimerService(TimerConfigurationService timerConfigurationService)
    {
        _timerConfigurationService = timerConfigurationService;
        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var timerMode = timerConfiguration.DefaultMode;
        var duration = timerConfiguration.GetDuration(timerMode);

        _timer = CreateTimer(timerMode, duration, timerConfiguration.Interval);
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
    }

    public event EventHandler<TimeSpan>? RemainingChanged;

    public event EventHandler<bool>? IsRunningChanged;

    public event EventHandler<TimerMode>? CurrentModeChanged;

    public TimerMode CurrentMode => _timer.Mode;

    public TimeSpan Remaining => _timer.Remaining;

    public bool IsRunning => _timer.IsRunning;

    public void Start()
    {
        if (IsRunning) return;

        _timer.Start();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _ = RunAsyncTimer(_cancellationTokenSource.Token);
    }

    public void Stop()
    {
        if (!IsRunning) return;

        _cancellationTokenSource?.Cancel();
        _timer.Stop();
    }

    public void SwitchModeNext()
    {
        Stop();

        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var nextMode = GetNextMode(CurrentMode, timerConfiguration.Modes);
        var duration = timerConfiguration.GetDuration(nextMode);
        _timer = CreateTimer(nextMode, duration, timerConfiguration.Interval);
        SignalCurrentModeChanged(nextMode);
    }

    public void SwitchModePrevious()
    {
        Stop();

        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var previousMode = GetPreviousMode(CurrentMode, timerConfiguration.Modes);
        var duration = timerConfiguration.GetDuration(previousMode);
        _timer = CreateTimer(previousMode, duration, timerConfiguration.Interval);
        SignalCurrentModeChanged(previousMode);
    }

    private Timer CreateTimer(TimerMode mode, TimeSpan duration, TimeSpan interval)
    {
        var timer = new Timer(mode, duration, interval);
        timer.RemainingChanged += (_, newValue) => RemainingChanged?.Invoke(this, newValue);
        timer.IsRunningChanged += (_, newValue) => IsRunningChanged?.Invoke(this, newValue);
        timer.Completed += OnCompleted;

        return timer;
    }

    private void OnCompleted(object? sender, EventArgs e)
    {
        SwitchModeNext();
    }

    private async Task RunAsyncTimer(CancellationToken cancellationToken)
    {
        using var periodicTimer = new PeriodicTimer(_timer.Interval);
        while (await periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false)) _timer.Tick();
    }

    private void SignalCurrentModeChanged(TimerMode nextMode)
    {
        CurrentModeChanged?.Invoke(this, nextMode);
    }

    private static TimerMode GetNextMode(TimerMode currentMode, LinkedList<TimerMode> allModes)
    {
        var currentModeNode = allModes.Find(currentMode);
        return currentModeNode?.Next?.ValueRef ?? allModes.First!.ValueRef;
    }

    private static TimerMode GetPreviousMode(TimerMode currentMode, LinkedList<TimerMode> allModes)
    {
        var currentModeNode = allModes.Find(currentMode);
        return currentModeNode?.Previous?.ValueRef ?? allModes.Last!.ValueRef;
    }

}