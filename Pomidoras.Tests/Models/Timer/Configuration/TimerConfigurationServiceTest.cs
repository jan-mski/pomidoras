using FluentAssertions;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.Tests.Models.Timer.Configuration.Repository;

namespace Pomidoras.Tests.Models.Timer.Configuration;

public class TimerConfigurationServiceTest
{

    private readonly TimerConfigurationRepositoryStub _timerConfigurationRepositoryStub = new();

    [Fact]
    public void GetTimerConfiguration_WhenConfigurationNotExists_ReturnsAndSavesDefault()
    {
        var expectedConfiguration = TimerConfigurationMother.Default();

        var timerConfigurationService = new TimerConfigurationService(_timerConfigurationRepositoryStub);

        var actualConfiguration = timerConfigurationService.GetTimerConfiguration();

        actualConfiguration.Should().BeEquivalentTo(expectedConfiguration);
    }

    [Fact]
    public void GetTimerConfiguration_WhenConfigurationExists_ReturnsIt()
    {
        var expectedConfiguration =
            TimerConfigurationMother.With_WorkDuration_Interval(TimeSpan.MaxValue, TimeSpan.MinValue);
        _timerConfigurationRepositoryStub.SaveConfiguration(expectedConfiguration);

        var timerConfigurationService = new TimerConfigurationService(_timerConfigurationRepositoryStub);

        var actualConfiguration = timerConfigurationService.GetTimerConfiguration();

        actualConfiguration.Should().BeEquivalentTo(expectedConfiguration);
    }

}