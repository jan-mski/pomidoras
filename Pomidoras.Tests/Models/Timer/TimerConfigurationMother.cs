using Pomidoras.Models.Timer;

namespace Pomidoras.Tests.Models.Timer;

public static class TimerConfigurationMother
{

    public static readonly TimeSpan DefaultWorkDuration = TimeSpan.FromMinutes(25);
    public static readonly TimeSpan DefaultBreakShortDuration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan DefaultBreakLongDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(1);
    public const TimerMode DefaultDefaultMode = TimerMode.Work;
    public const int DefaultWorkSessionsUntilBreakLong = 4;

    public static TimerConfiguration Default()
    {
        return new TimerConfiguration(
            DefaultWorkDuration,
            DefaultBreakShortDuration,
            DefaultBreakLongDuration,
            DefaultInterval,
            DefaultDefaultMode,
            DefaultWorkSessionsUntilBreakLong
        );
    }

    public static TimerConfiguration With_WorkDuration_Interval(TimeSpan workDuration, TimeSpan interval)
    {
        return new TimerConfiguration(
            workDuration,
            DefaultBreakShortDuration,
            DefaultBreakLongDuration,
            interval,
            DefaultDefaultMode,
            DefaultWorkSessionsUntilBreakLong);
    }

}