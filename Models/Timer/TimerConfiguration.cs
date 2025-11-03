using System;

namespace Pomidoras.Models.Timer;

public class TimerConfiguration(
    TimeSpan workDuration,
    TimeSpan breakShortDuration,
    TimeSpan breakLongDuration,
    TimeSpan interval,
    TimerMode defaultMode,
    int workSessionsUntilBreakLong)
{

    public TimeSpan Interval { get; } = interval;
    public TimerMode DefaultMode { get; } = defaultMode;
    public int WorkSessionsUntilBreakLong { get; } = workSessionsUntilBreakLong;

    public TimeSpan GetDuration(TimerMode timerMode)
    {
        return timerMode switch
        {
            TimerMode.Work => workDuration,
            TimerMode.BreakShort => breakShortDuration,
            TimerMode.BreakLong => breakLongDuration,
            _ => throw new ArgumentException("Invalid timer type", nameof(timerMode))
        };
    }

}