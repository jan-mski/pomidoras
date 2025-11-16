using FluentAssertions;
using Pomidoras.Models.Timer;

namespace Pomidoras.Tests.Models.Timer;

public class TimerConfigurationTest
{

    private static TimerConfiguration CreateDefaultConfiguration()
    {
        return new TimerConfiguration(
            WorkDuration: TimeSpan.FromMinutes(25),
            BreakShortDuration: TimeSpan.FromMinutes(5),
            BreakLongDuration: TimeSpan.FromMinutes(15),
            Interval: TimeSpan.FromMilliseconds(100),
            DefaultMode: TimerMode.Work,
            WorkSessionsUntilBreakLong: 4
        );
    }

    public static TheoryData<TimerMode, TimeSpan> GetTimerModeAndExpectedDurations()
    {
        var theoryData = new TheoryData<TimerMode, TimeSpan>();
        theoryData.Add(TimerMode.Work, TimeSpan.FromMinutes(25));
        theoryData.Add(TimerMode.BreakShort, TimeSpan.FromMinutes(5));
        theoryData.Add(TimerMode.BreakLong, TimeSpan.FromMinutes(15));
        return theoryData;
    }

    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        var expectedInterval = TimeSpan.FromMilliseconds(100);
        var expectedDefaultMode = TimerMode.Work;
        var expectedWorkSessionsUntilBreakLong = 4;

        var configuration = CreateDefaultConfiguration();

        configuration.Interval.Should().Be(expectedInterval);
        configuration.DefaultMode.Should().Be(expectedDefaultMode);
        configuration.WorkSessionsUntilBreakLong.Should().Be(expectedWorkSessionsUntilBreakLong);
    }

    [Theory]
    [MemberData(nameof(GetTimerModeAndExpectedDurations))]
    public void GetDuration_ReturnsCorrectDuration_ForValidTimerMode(TimerMode timerMode, TimeSpan expectedDuration)
    {
        var configuration = CreateDefaultConfiguration();

        var actualDuration = configuration.GetDuration(timerMode);

        actualDuration.Should().Be(expectedDuration);
    }

    [Fact]
    public void GetDuration_ThrowsArgumentException_ForInvalidTimerMode()
    {
        var configuration = CreateDefaultConfiguration();
        var invalidTimerMode = (TimerMode)999;

        var act = () => configuration.GetDuration(invalidTimerMode);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("timerMode");
    }

}