using System;
using System.Threading;
using System.Threading.Tasks;
using Pomidoras.Models.Timer.Configuration;

namespace Pomidoras.Models.Timer;

// TODO:
//  - [x] Fix TimerServiceTest by using a timer factory
//  - [x] Reconsider testing approach for periodic timer so that I can avoid writing shitty interfaces for no reason
//  - [x] Instead of the dumb timer factory, just configure an interval of 50ms or less and only assert events
//  - [ ] Add remaining methods from TimerService_Old
//  - [ ] Add tests for the remaining methods
//  - [ ] Add this service to dependency injection and update those tests
public sealed class TimerService : IDisposable
{

    private readonly TimerConfigurationService _timerConfigurationService;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly TimerState _state;

    public event EventHandler<TimeSpan>? RemainingChanged;
    public event EventHandler<bool>? IsRunningChanged;

    public TimeSpan Remaining => _state.Remaining;

    public bool IsRunning => _state.IsRunning;

    public TimerService(TimerConfigurationService timerConfigurationService)
    {
        _timerConfigurationService = timerConfigurationService;
        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        var timerMode = timerConfiguration.DefaultMode;
        var duration = timerConfiguration.GetDuration(timerMode);
        _state = new TimerState(timerMode, duration, timerConfiguration.Interval);
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
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
        _ = RunAsyncTimer(_cancellationTokenSource.Token);
    }

    public void Stop()
    {
        if (!_state.IsRunning)
        {
            return;
        }

        Finish();

        _state.Remaining = _state.Duration;
        SignalRemainingChanged();
    }

    private void Finish()
    {
        _cancellationTokenSource?.Cancel();
        _state.IsRunning = false;
        SignalIsRunningChanged(false);
    }

    private void Tick()
    {
        if (_state.Remaining <= _state.Interval)
        {
            _state.Remaining = TimeSpan.Zero;
            SignalRemainingChanged();
            Finish();
        }
        else
        {
            _state.Remaining -= _state.Interval;
            SignalRemainingChanged();
        }
    }

    private void SignalRemainingChanged()
    {
        RemainingChanged?.Invoke(this, _state.Remaining);
    }

    private void SignalIsRunningChanged(bool newValue)
    {
        IsRunningChanged?.Invoke(this, newValue);
    }

    private async Task RunAsyncTimer(CancellationToken cancellationToken)
    {
        using var periodicTimer = new PeriodicTimer(_state.Interval);
        while (await periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            Tick();
        }
    }

}