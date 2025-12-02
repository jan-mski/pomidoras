using FluentAssertions;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.Models.Timer.Configuration.Repository;

namespace Pomidoras.Tests.Models.Timer;

public class TimerConfigurationServiceTest
{

    private readonly InMemoryTimerConfigurationRepository _timerConfigurationRepository = new();

    [Fact]
    public void GetTimerConfiguration_WhenNoConfigurationExists_ReturnsAndSavesDefault()
    {
        var expectedConfiguration = TimerConfigurationMother.Default();
        _timerConfigurationRepository.SaveConfiguration(expectedConfiguration);

        var timerConfigurationService = new TimerConfigurationService(_timerConfigurationRepository);

        var actualConfiguration = timerConfigurationService.GetTimerConfiguration();

        actualConfiguration.Should().BeEquivalentTo(expectedConfiguration);
    }

    [Fact]
    public void GetTimerConfiguration_WhenConfigurationExists_ReturnsIt()
    {
        var expectedConfiguration =
            TimerConfigurationMother.With_WorkDuration_Interval(TimeSpan.MaxValue, TimeSpan.MinValue);
        _timerConfigurationRepository.SaveConfiguration(expectedConfiguration);

        var timerConfigurationService = new TimerConfigurationService(_timerConfigurationRepository);

        var actualConfiguration = timerConfigurationService.GetTimerConfiguration();

        actualConfiguration.Should().BeEquivalentTo(expectedConfiguration);
    }

}