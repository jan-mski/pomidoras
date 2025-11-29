using System;

namespace Pomidoras.Models.Timer;

public enum TimerMode
{

    Work,
    BreakShort,
    BreakLong

}

public record TimerState(TimerMode Mode, TimeSpan Duration, TimeSpan Interval)
{

    public TimeSpan Duration { get; } = Duration;
    public TimeSpan Interval { get; } = Interval;
    public TimerMode Mode { get; } = Mode;
    public TimeSpan Remaining { get; set; } = Duration;
    public bool IsRunning { get; set; }

}