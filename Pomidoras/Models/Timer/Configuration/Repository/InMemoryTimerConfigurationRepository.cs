namespace Pomidoras.Models.Timer.Configuration.Repository;

public class InMemoryTimerConfigurationRepository : ITimerConfigurationRepository
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