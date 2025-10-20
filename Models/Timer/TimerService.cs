using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pomidoras.Models.Timer;

public interface ITimerService
{

    TimeSpan Remaining { get; }
    bool IsRunning { get; }
    void Start();
    void Stop();
    event EventHandler<TimeSpan>? RemainingChanged;
    event EventHandler? Completed;

}

public sealed class TimerService : ITimerService, IDisposable
{

    private readonly TimerMode _currentMode;
    private readonly Timer _timer;
    private readonly TimerConfigurationService _timerConfigurationService;

    private CancellationTokenSource? _cancellationTokenSource;

    public TimerService(TimerConfigurationService timerConfigurationService)
    {
        _timerConfigurationService = timerConfigurationService;
        var timerConfiguration = _timerConfigurationService.GetTimerConfiguration();
        _currentMode = timerConfiguration.DefaultMode;
        _timer = new Timer(timerConfiguration.GetDuration(_currentMode));
        _timer.RemainingChanged += (_, newValue) => RemainingChanged?.Invoke(this, newValue);
        _timer.Completed += (_, _) => Completed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
    }

    public event EventHandler<TimeSpan>? RemainingChanged;
    public event EventHandler? Completed;

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

    public void SwitchToNextMode()
    {
        // TODO: select next mode, create new timer, etc.
    }

    private async Task RunAsyncTimer(CancellationToken cancellationToken)
    {
        var interval = _timerConfigurationService.GetTimerConfiguration().Interval;
        using var periodicTimer = new PeriodicTimer(interval);
        try
        {
            while (await periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))  // TODO: what is ConfigureAwait?
                _timer.Tick(interval);
        }
        catch (OperationCanceledException)
        {
            // TODO: need some log here
        }
    }

}