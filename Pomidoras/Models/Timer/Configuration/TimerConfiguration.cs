using System;
using System.Collections.Generic;
using System.Linq;

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
    int WorkSessionsUntilBreakLong,
    bool ContinuousModeEnabled)
{
    public List<TimerMode> Modes { get; } = CreateModes(WorkSessionsUntilBreakLong);

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

    private static List<TimerMode> CreateModes(int workSessionsUntilBreakLong)
    {
        List<TimerMode> workAndBreakShort = new([TimerMode.Work, TimerMode.BreakShort]);
        List<TimerMode> workAndBreakLong = new([TimerMode.Work, TimerMode.BreakLong]);

        return Enumerable.Repeat(workAndBreakShort, workSessionsUntilBreakLong - 1)
            .SelectMany(x => x)
            .Concat(workAndBreakLong)
            .ToList();
    }
}