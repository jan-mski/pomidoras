using FluentAssertions;

namespace Pomidoras.Tests.Models.Timer;

public class TimerStateTest
{

    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        var timer = TimerStateMother.Default();

        timer.Mode.Should().Be(TimerStateMother.DefaultMode);
        timer.Remaining.Should().Be(TimerStateMother.DefaultDuration);
        timer.Interval.Should().Be(TimerStateMother.DefaultInterval);
        timer.IsRunning.Should().BeFalse();
    }

}