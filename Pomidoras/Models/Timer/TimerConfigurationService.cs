using System;

namespace Pomidoras.Models.Timer;

public class TimerConfigurationService
{

    public TimerConfiguration GetTimerConfiguration()
    {
        return new TimerConfiguration(
            WorkDuration: TimeSpan.FromMinutes(25),
            BreakShortDuration: TimeSpan.FromMinutes(5),
            BreakLongDuration: TimeSpan.FromMinutes(20),
            Interval: TimeSpan.FromSeconds(1),
            DefaultMode: TimerMode.Work,
            WorkSessionsUntilBreakLong: 4);
    }

}