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

    event EventHandler<TimeSpan> RemainingChanged;

}

public sealed class TimerService : ITimerService, IDisposable
{

    private readonly Timer _timer;

    private CancellationTokenSource? _cancellationTokenSource;

    public TimerService(TimerConfigurationService timerConfigurationService)
    {
        var timerConfiguration = timerConfigurationService.GetTimerConfiguration();
        var duration = timerConfiguration.GetDuration(timerConfiguration.DefaultMode);

        _timer = new Timer(duration, timerConfiguration.Interval);
        _timer.RemainingChanged += (_, newValue) => RemainingChanged?.Invoke(this, newValue);
        _timer.Completed += OnCompleted;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
    }

    public event EventHandler<TimeSpan>? RemainingChanged;

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

    private void OnCompleted(object? sender, EventArgs e)
    {
        // TODO: switch mode if "continuous" mode is enabled (it's a UI-side switch though)
        throw new NotImplementedException();
    }

    private async Task RunAsyncTimer(CancellationToken cancellationToken)
    {
        using var periodicTimer = new PeriodicTimer(_timer.Interval);
        while (await periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false)) _timer.Tick();
    }

}