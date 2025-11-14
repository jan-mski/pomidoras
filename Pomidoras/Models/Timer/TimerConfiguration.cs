using System;

namespace Pomidoras.Models.Timer;

public record TimerConfiguration(
    TimeSpan WorkDuration,
    TimeSpan BreakShortDuration,
    TimeSpan BreakLongDuration,
    TimeSpan Interval,
    TimerMode DefaultMode,
    int WorkSessionsUntilBreakLong)
{

    private TimeSpan WorkDuration { get; } = WorkDuration;
    private TimeSpan BreakShortDuration { get; } = BreakShortDuration;
    private TimeSpan BreakLongDuration { get; } = BreakLongDuration;

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