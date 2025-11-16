using FluentAssertions;
using Pomidoras.Models.Timer;

namespace Pomidoras.Tests.Models.Timer;

public class TimerConfigurationServiceTest
{

    [Fact]
    public void GetTimerConfiguration_ReturnsDefaults()
    {
        var timerConfigurationService = new TimerConfigurationService();

        var expectedConfiguration = TimerConfigurationMother.Default();

        var actualConfiguration = timerConfigurationService.GetTimerConfiguration();
        
        actualConfiguration.Should().BeEquivalentTo(expectedConfiguration);
    }

}