using System;

namespace Pomidoras.Models.Timer;

public class Timer(TimeSpan duration, TimeSpan interval)
{

    private readonly TimeSpan _duration = duration;
    public TimeSpan Remaining { get; private set; } = duration;
    public bool IsRunning { get; private set; }

    public TimeSpan Interval { get; } = interval;
    public event EventHandler<TimeSpan>? RemainingChanged;
    public event EventHandler<bool>? IsRunningChanged;

    public void Start()
    {
        if (IsRunning) return;
        IsRunning = true;
        SignalIsRunningChanged(true);
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        Remaining = _duration;
        SignalRemainingChanged();
        SignalIsRunningChanged(false);
    }

    public void Tick()
    {
        if (Remaining <= Interval)
        {
            Remaining = TimeSpan.Zero;
            SignalRemainingChanged();
        }
        else
        {
            Remaining -= Interval;
            SignalRemainingChanged();
        }
    }

    private void SignalRemainingChanged()
    {
        RemainingChanged?.Invoke(this, Remaining);
    }

    private void SignalIsRunningChanged(bool newValue)
    {
        IsRunningChanged?.Invoke(this, newValue);
    }

}