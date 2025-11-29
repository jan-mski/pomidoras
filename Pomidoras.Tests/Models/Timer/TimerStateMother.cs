using Pomidoras.Models.Timer;

namespace Pomidoras.Tests.Models.Timer;

public static class TimerStateMother
{

    public static readonly TimeSpan DefaultDuration = TimeSpan.FromMinutes(25);
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(1);
    public const TimerMode DefaultMode = TimerMode.Work;

    public static TimerState Default()
    {
        return new TimerState(DefaultMode, DefaultDuration, DefaultInterval);
    }

}