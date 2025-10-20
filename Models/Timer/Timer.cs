using System;

namespace Pomidoras.Models.Timer;

public class Timer(TimeSpan duration)
{

    private readonly TimeSpan _duration = duration;
    public TimeSpan Remaining { get; private set; } = duration;
    public bool IsRunning { get; private set; }

    public event EventHandler<TimeSpan>? RemainingChanged;
    public event EventHandler? Completed;

    public void Start()
    {
        if (IsRunning) return;
        IsRunning = true;
        SignalRemainingChanged();
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        Remaining = _duration;
        SignalRemainingChanged();
    }

    public void Tick(TimeSpan interval)
    {
        if (!IsRunning || interval <= TimeSpan.Zero) return;

        if (Remaining <= interval)
        {
            Remaining = TimeSpan.Zero;
            IsRunning = false;
            SignalRemainingChanged();
            SignalCompleted();
        }

        Remaining -= interval;
        SignalRemainingChanged();
    }

    private void SignalRemainingChanged()
    {
        RemainingChanged?.Invoke(this, Remaining);
    }

    private void SignalCompleted()
    {
        Completed?.Invoke(this, EventArgs.Empty);
    }

}