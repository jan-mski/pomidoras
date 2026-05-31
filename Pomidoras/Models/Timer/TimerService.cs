using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pomidoras.Models.Timer.Configuration;

namespace Pomidoras.Models.Timer;

public sealed class TimerService : IDisposable, IAsyncDisposable
{
    private sealed class TimerState(TimerConfiguration configuration)
    {
        public List<TimerMode> Modes { get; } = configuration.Modes;
        public int CurrentModeIndex { get; set; } = configuration.InitialModeIndex;
        public TimeSpan Duration { get; set; } = GetInitialModeDuration(configuration);
        public TimeSpan Interval { get; } = configuration.Interval;
        public TimeSpan Remaining { get; set; } = GetInitialModeDuration(configuration);
        public bool IsRunning { get; set; }
        public TimerMode CurrentMode => Modes[CurrentModeIndex];

        private static TimeSpan GetInitialModeDuration(TimerConfiguration configuration)
        {
            return configuration.GetDuration(configuration.Modes[configuration.InitialModeIndex]);
        }
    }

    private readonly TimerConfigurationService _timerConfigurationService;
    private readonly TimerState _state;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _runningTimerTask;

    public event EventHandler<TimeSpan>? RemainingChanged;
    public event EventHandler<bool>? IsRunningChanged;
    public event EventHandler<TimerMode>? ModeChanged;

    public TimerConfiguration Configuration { get; }

    public TimeSpan Remaining => _state.Remaining;

    public bool IsRunning => _state.IsRunning;

    public TimerMode CurrentMode => _state.CurrentMode;

    public TimerService(TimerConfigurationService timerConfigurationService)
    {
        _timerConfigurationService = timerConfigurationService;
        Configuration = _timerConfigurationService.GetTimerConfiguration();
        _state = new TimerState(Configuration);
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

    public void SwitchMode(bool isForward)
    {
        if (_state.IsRunning)
        {
            SetCompleted();
        }

        var newModeIndex = isForward ? GetNextModeIndex() : GetPreviousModeIndex();

        SwitchMode(newModeIndex);
    }

    private void SwitchMode(int newModeIndex)
    {
        _state.CurrentModeIndex = newModeIndex;
        var newMode = _state.CurrentMode;

        _state.Duration = Configuration.GetDuration(newMode);

        UpdateRemaining(_state.Duration);

        ModeChanged?.Invoke(this, newMode);
    }

    private int GetNextModeIndex()
    {
        return _state.CurrentModeIndex + 1 == _state.Modes.Count
            ? 0
            : _state.CurrentModeIndex + 1;
    }

    private int GetPreviousModeIndex()
    {
        return _state.CurrentModeIndex - 1 == -1
            ? _state.Modes.Count - 1
            : _state.CurrentModeIndex - 1;
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
            SwitchMode(true);
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
        RemainingChanged?.Invoke(this, newRemaining);
    }
}