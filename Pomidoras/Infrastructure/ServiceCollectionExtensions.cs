using Microsoft.Extensions.DependencyInjection;
using Pomidoras.Models.Timer;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.Models.Timer.Configuration.Repository;
using Pomidoras.ViewModels;

namespace Pomidoras.Infrastructure;

public static class ServiceCollectionExtensions
{

    public static void AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<MainWindowViewModel>();
        serviceCollection.AddSingleton<TimerConfigurationService>();
        serviceCollection.AddSingleton<ITimerConfigurationRepository, InMemoryTimerConfigurationRepository>(_ =>
            new InMemoryTimerConfigurationRepository());
        serviceCollection.AddSingleton<TimerService>();
    }

}