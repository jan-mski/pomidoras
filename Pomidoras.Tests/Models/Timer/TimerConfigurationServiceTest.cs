using FluentAssertions;
using Pomidoras.Models.Timer;

namespace Pomidoras.Tests.Models.Timer;

public class TimerConfigurationServiceTest
{

    [Fact]
    public void GetTimerConfiguration_ReturnsDefaults()
    {
        var timerConfigurationService = new TimerConfigurationService();
        
        var expectedConfiguration = new TimerConfiguration(
            WorkDuration: TimeSpan.FromMinutes(25),
            BreakShortDuration: TimeSpan.FromMinutes(5),
            BreakLongDuration: TimeSpan.FromMinutes(20),
            Interval: TimeSpan.FromSeconds(1),
            DefaultMode: TimerMode.Work,
            WorkSessionsUntilBreakLong: 4);

        var actualConfiguration = timerConfigurationService.GetTimerConfiguration();
        
        actualConfiguration.Should().BeEquivalentTo(expectedConfiguration);
    }

}