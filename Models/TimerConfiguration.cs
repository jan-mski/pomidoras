using System;

namespace Pomidoras.Models;

public class TimerConfiguration(
    TimeSpan workDuration,
    TimeSpan breakLongDuration,
    TimeSpan breakShortDuration,
    TimeSpan interval,
    TimerMode defaultMode)
{

    public TimeSpan Interval { get; } = interval;
    public TimerMode DefaultMode { get; } = defaultMode;

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