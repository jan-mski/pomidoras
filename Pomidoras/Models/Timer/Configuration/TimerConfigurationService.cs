using System;

namespace Pomidoras.Models.Timer.Configuration;

public interface ITimerConfigurationRepository
{
    TimerConfiguration? GetConfiguration();
    TimerConfiguration SaveConfiguration(TimerConfiguration configuration);
}

public class TimerConfigurationService(ITimerConfigurationRepository repository)
{
    private static readonly TimerConfiguration Default = new(
        TimeSpan.FromMinutes(25),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(20),
        TimeSpan.FromSeconds(1),
        0,
        4);

    public TimerConfiguration GetTimerConfiguration()
    {
        return repository.GetConfiguration() ?? repository.SaveConfiguration(Default);
    }
}