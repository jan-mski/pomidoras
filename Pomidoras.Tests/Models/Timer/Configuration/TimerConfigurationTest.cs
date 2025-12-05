using FluentAssertions;
using Pomidoras.Models.Timer.Configuration;

namespace Pomidoras.Tests.Models.Timer.Configuration;

public class TimerConfigurationTest
{

    public static TheoryData<TimerMode, TimeSpan> GetTimerModeAndExpectedDurations()
    {
        return new TheoryData<TimerMode, TimeSpan>
        {
            { TimerMode.Work, TimerConfigurationMother.DefaultWorkDuration },
            { TimerMode.BreakShort, TimerConfigurationMother.DefaultBreakShortDuration },
            { TimerMode.BreakLong, TimerConfigurationMother.DefaultBreakLongDuration }
        };
    }

    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        var configuration = TimerConfigurationMother.Default();

        var expectedInterval = TimerConfigurationMother.DefaultInterval;
        var expectedInitialModeIndex = TimerConfigurationMother.DefaultInitialModeIndex;
        var expectedWorkSessionsUntilBreakLong = TimerConfigurationMother.DefaultWorkSessionsUntilBreakLong;

        configuration.Interval.Should().Be(expectedInterval);
        configuration.InitialModeIndex.Should().Be(expectedInitialModeIndex);
        configuration.WorkSessionsUntilBreakLong.Should().Be(expectedWorkSessionsUntilBreakLong);
    }

    [Theory]
    [MemberData(nameof(GetTimerModeAndExpectedDurations))]
    public void GetDuration_ReturnsCorrectDuration_ForValidTimerMode(TimerMode timerMode, TimeSpan expectedDuration)
    {
        var configuration = TimerConfigurationMother.Default();

        var actualDuration = configuration.GetDuration(timerMode);

        actualDuration.Should().Be(expectedDuration);
    }

    [Fact]
    public void GetDuration_ThrowsArgumentException_ForInvalidTimerMode()
    {
        var configuration = TimerConfigurationMother.Default();
        var invalidTimerMode = (TimerMode)999;

        var act = () => configuration.GetDuration(invalidTimerMode);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("timerMode");
    }

}