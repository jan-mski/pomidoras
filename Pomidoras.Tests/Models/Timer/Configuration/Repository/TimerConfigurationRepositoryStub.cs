using Pomidoras.Models.Timer.Configuration;

namespace Pomidoras.Tests.Models.Timer.Configuration.Repository;

public class TimerConfigurationRepositoryStub : ITimerConfigurationRepository
{

    private TimerConfiguration? _configuration;

    public TimerConfiguration? GetConfiguration()
    {
        return _configuration;
    }

    public TimerConfiguration SaveConfiguration(TimerConfiguration configuration)
    {
        _configuration = configuration;
        return _configuration;
    }

}