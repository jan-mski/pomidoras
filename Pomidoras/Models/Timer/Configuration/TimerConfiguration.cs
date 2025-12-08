using System;

namespace Pomidoras.Models.Timer.Configuration;

public enum TimerMode
{

    Work,
    BreakShort,
    BreakLong

}

public record TimerConfiguration(
    TimeSpan WorkDuration,
    TimeSpan BreakShortDuration,
    TimeSpan BreakLongDuration,
    TimeSpan Interval,
    int InitialModeIndex,
    int WorkSessionsUntilBreakLong)
{

    public TimeSpan GetDuration(TimerMode timerMode)
    {
        return timerMode switch
        {
            TimerMode.Work => WorkDuration,
            TimerMode.BreakShort => BreakShortDuration,
            TimerMode.BreakLong => BreakLongDuration,
            _ => throw new ArgumentException("Invalid timer type", nameof(timerMode))
        };
    }

}