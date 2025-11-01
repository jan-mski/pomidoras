using System;
using System.Collections.Generic;

namespace Pomidoras.Models.Timer;

public class TimerConfiguration(
    TimeSpan workDuration,
    TimeSpan breakShortDuration,
    TimeSpan breakLongDuration,
    TimeSpan interval,
    TimerMode defaultMode)
{

    public TimeSpan Interval { get; } = interval;
    public TimerMode DefaultMode { get; } = defaultMode;

    // TODO: should be passed to constructor, not hardcoded -- but maybe should pass a dictionary of modes to times?
    public LinkedList<TimerMode> Modes { get; } = new([TimerMode.Work, TimerMode.BreakShort, TimerMode.BreakLong]);

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