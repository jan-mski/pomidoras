using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pomidoras.Models.Timer.Configuration;

namespace Pomidoras.Models.Timer;

public sealed class TimerState(
    List<TimerMode> modes,
    int currentModeIndex,
    TimeSpan duration,
    TimeSpan interval)
{

    public List<TimerMode> Modes { get; } = modes;
    public int CurrentModeIndex { get; set; } = currentModeIndex;
    public TimeSpan Duration { get; set; } = duration;
    public TimeSpan Interval { get; } = interval;
    public TimeSpan Remaining { get; set; } = duration;
    public bool IsRunning { get; set; }

    public TimerMode GetCurrentMode()
    {
        return Modes[CurrentModeIndex];
    }

}

public sealed class TimerService : IDisposable, IAsyncDisposable
{

    private readonly TimerConfigurationService _timerConfigurationService;
    private readonly TimerState _state;
    private readonly TimerConfiguration _timerConfiguration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _runningTimerTask;

    public event EventHandler<TimeSpan>? RemainingChanged;
    public event EventHandler<bool>? IsRunningChanged;
    public event EventHandler<TimerMode>? ModeChanged;

    public TimeSpan Remaining => _state.Remaining;

    public bool IsRunning => _state.IsRunning;

    public TimerService(TimerConfigurationService timerConfigurationService)
    {
        _timerConfigurationService = timerConfigurationService;
        _timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var timerModes = CreateModes(_timerConfiguration);
        var initialModeIndex = _timerConfiguration.InitialModeIndex;
        var duration = _timerConfiguration.GetDuration(timerModes[initialModeIndex]);
        _state = new TimerState(timerModes, initialModeIndex, duration, _timerConfiguration.Interval);
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }

        if (_runningTimerTask is not null)
        {
            try
            {
                await _runningTimerTask.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        _cancellationTokenSource?.Dispose();
    }

    public void Start()
    {
        if (_state.IsRunning)
        {
            return;
        }

        UpdateIsRunning(true);

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _runningTimerTask = RunAsyncTimer(_cancellationTokenSource.Token);
    }

    public void Stop()
    {
        if (!_state.IsRunning)
        {
            return;
        }

        SetCompleted();
        UpdateRemaining(_state.Duration);
    }

    public void SwitchModeNext()
    {
        if (_state.IsRunning)
        {
            SetCompleted();
        }

        var nextModeIndex = _state.CurrentModeIndex + 1 == _state.Modes.Count ? 0 : _state.CurrentModeIndex + 1;
        SwitchMode(nextModeIndex);
    }

    public void SwitchModePrevious()
    {
        if (_state.IsRunning)
        {
            SetCompleted();
        }

        var previousModeIndex = _state.CurrentModeIndex - 1 < 0 ? _state.Modes.Count - 1 : _state.CurrentModeIndex - 1;
        SwitchMode(previousModeIndex);
    }

    private void SwitchMode(int newModeIndex)
    {
        _state.CurrentModeIndex = newModeIndex;
        var newMode = _state.GetCurrentMode();

        _state.Duration = _timerConfiguration.GetDuration(newMode);

        UpdateRemaining(_state.Duration);

        ModeChanged?.Invoke(this, newMode);
    }

    private async Task RunAsyncTimer(CancellationToken cancellationToken)
    {
        using var periodicTimer = new PeriodicTimer(_state.Interval);
        while (await periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            Tick();
        }
    }

    private void Tick()
    {
        if (_state.Remaining <= _state.Interval)
        {
            UpdateRemaining(TimeSpan.Zero);
            SetCompleted();
        }
        else
        {
            UpdateRemaining(_state.Remaining - _state.Interval);
        }
    }

    private void SetCompleted()
    {
        _cancellationTokenSource?.Cancel();
        UpdateIsRunning(false);
    }

    private void UpdateIsRunning(bool newIsRunning)
    {
        _state.IsRunning = newIsRunning;
        IsRunningChanged?.Invoke(this, newIsRunning);
    }

    private void UpdateRemaining(TimeSpan newRemaining)
    {
        _state.Remaining = newRemaining;
        RemainingChanged?.Invoke(this, _state.Remaining);
    }

    private List<TimerMode> CreateModes(TimerConfiguration timerConfiguration)
    {
        List<TimerMode> workAndBreakShort = new([TimerMode.Work, TimerMode.BreakShort]);
        List<TimerMode> workAndBreakLong = new([TimerMode.Work, TimerMode.BreakLong]);

        return Enumerable.Repeat(workAndBreakShort, timerConfiguration.WorkSessionsUntilBreakLong - 1)
            .SelectMany(x => x)
            .Concat(workAndBreakLong)
            .ToList();
    }

}