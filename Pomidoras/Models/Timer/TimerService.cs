using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pomidoras.Models.Timer.Configuration;

namespace Pomidoras.Models.Timer;

public record TimerState(
    List<TimerMode> Modes,
    int CurrentModeIndex,
    TimeSpan Duration,
    TimeSpan Interval)
{

    public int CurrentModeIndex { get; set; } = CurrentModeIndex;
    public TimeSpan Duration { get; set; } = Duration;
    public TimeSpan Interval { get; } = Interval;
    public TimeSpan Remaining { get; set; } = Duration;
    public bool IsRunning { get; set; }

    public TimerMode GetCurrentMode()
    {
        return Modes[CurrentModeIndex];
    }

}

// TODO:
//  - [x] Fix TimerServiceTest by using a timer factory
//  - [x] Reconsider testing approach for periodic timer so that I can avoid writing shitty interfaces for no reason
//  - [x] Instead of the dumb timer factory, just configure an interval of 50ms or less and only assert events
//  - [X] Add remaining methods from TimerService_Old
//  - [x] Add tests for the remaining methods
//  - [x] Add tests for Dispose and DisposeAsync
//  - [ ] Try to add some test for mode index corner cases
//  - [ ] Add this service to dependency injection and update those tests
public sealed class TimerService : IDisposable, IAsyncDisposable
{

    private readonly TimerConfigurationService _timerConfigurationService;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly TimerState _state;
    private Task? _runningTimerTask;

    public event EventHandler<TimeSpan>? RemainingChanged;
    public event EventHandler<bool>? IsRunningChanged;
    public event EventHandler<TimerMode>? ModeChanged;

    public TimeSpan Remaining => _state.Remaining;

    public bool IsRunning => _state.IsRunning;

    public TimerService(TimerConfigurationService timerConfigurationService)
    {
        _timerConfigurationService = timerConfigurationService;
        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var timerModes = CreateModes(timerConfiguration);
        var timerModeIndex = timerConfiguration.InitialModeIndex;
        var duration = timerConfiguration.GetDuration(timerModes[timerModeIndex]);
        _state = new TimerState(timerModes, timerModeIndex, duration, timerConfiguration.Interval);
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

        _state.IsRunning = true;
        SignalIsRunningChanged(true);

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

        _state.Remaining = _state.Duration;
        SignalRemainingChanged();
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
        var currentMode = _state.GetCurrentMode();
        
        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        _state.Duration = timerConfiguration.GetDuration(currentMode);
        _state.Remaining = _state.Duration;
        
        SignalRemainingChanged();
        SignalModeChanged(currentMode);
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
            _state.Remaining = TimeSpan.Zero;
            SignalRemainingChanged();
            SetCompleted();
        }
        else
        {
            _state.Remaining -= _state.Interval;
            SignalRemainingChanged();
        }
    }

    private void SetCompleted()
    {
        _cancellationTokenSource?.Cancel();
        _state.IsRunning = false;
        SignalIsRunningChanged(false);
    }

    private void SignalIsRunningChanged(bool newValue)
    {
        IsRunningChanged?.Invoke(this, newValue);
    }

    private void SignalRemainingChanged()
    {
        RemainingChanged?.Invoke(this, _state.Remaining);
    }

    private void SignalModeChanged(TimerMode nextMode)
    {
        ModeChanged?.Invoke(this, nextMode);
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