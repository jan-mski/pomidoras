using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pomidoras.Models.Timer;

public interface ITimerService
{

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

    private readonly LinkedList<TimerMode> _modes;
    private readonly TimerConfigurationService _timerConfigurationService;
    private CancellationTokenSource? _cancellationTokenSource;
    private LinkedListNode<TimerMode> _currentMode;
    private Timer _timer;

    public TimerService(TimerConfigurationService timerConfigurationService)
    {
        _timerConfigurationService = timerConfigurationService;
        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var timerMode = timerConfiguration.DefaultMode;
        var duration = timerConfiguration.GetDuration(timerMode);
        _modes = CreateModes(timerConfiguration);
        _currentMode = _modes.First!;
        _timer = CreateTimer(timerMode, duration, timerConfiguration.Interval);
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
    }

    public event EventHandler<TimeSpan>? RemainingChanged;

    public event EventHandler<bool>? IsRunningChanged;

    public event EventHandler<TimerMode>? CurrentModeChanged;

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
        SetNextMode();

        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var duration = timerConfiguration.GetDuration(_currentMode.ValueRef);
        _timer = CreateTimer(_currentMode.ValueRef, duration, timerConfiguration.Interval);
        SignalCurrentModeChanged(_currentMode.ValueRef);
    }

    public void SwitchModePrevious()
    {
        Stop();
        SetPreviousMode();

        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var duration = timerConfiguration.GetDuration(_currentMode.ValueRef);
        _timer = CreateTimer(_currentMode.ValueRef, duration, timerConfiguration.Interval);
        SignalCurrentModeChanged(_currentMode.ValueRef);
    }

    private LinkedList<TimerMode> CreateModes(TimerConfiguration timerConfiguration)
    {
        LinkedList<TimerMode> workAndBreakShort = new([TimerMode.Work, TimerMode.BreakShort]);
        LinkedList<TimerMode> workAndBreakLong = new([TimerMode.Work, TimerMode.BreakLong]);

        return new LinkedList<TimerMode>(
            Enumerable.Repeat(workAndBreakShort, timerConfiguration.WorkSessionsUntilBreakLong - 1)
                .SelectMany(x => x)
                .Concat(workAndBreakLong));
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

    private void SetNextMode()
    {
        _currentMode = _currentMode.Next ?? _modes.First!;
    }

    private void SetPreviousMode()
    {
        _currentMode = _currentMode.Previous ?? _modes.Last!;
    }

}