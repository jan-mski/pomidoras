using Pomidoras.Models.Timer;
using TimerClass = Pomidoras.Models.Timer.Timer;

namespace Pomidoras.Tests.Models.Timer;

public static class TimerMother
{

    public static readonly TimeSpan DefaultDuration = TimeSpan.FromMinutes(25);
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(1);
    public const TimerMode DefaultMode = TimerMode.Work;

    public static TimerClass Default()
    {
        return new TimerClass(DefaultMode, DefaultDuration, DefaultInterval);
    }

    public static TimerClass WithDuration(TimeSpan duration)
    {
        return new TimerClass(DefaultMode, duration, DefaultInterval);
    }

}
