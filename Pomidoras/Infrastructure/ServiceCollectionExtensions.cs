using Microsoft.Extensions.DependencyInjection;
using Pomidoras.Models.Timer;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.Models.Timer.Configuration.Repository;
using Pomidoras.ViewModels;

namespace Pomidoras.Infrastructure;

public static class ServiceCollectionExtensions
{

    public static void AddDesignTimeServices(this IServiceCollection serviceCollection)
    {
        // This is so that I don't forget how to do that when I actually define a non in-memory repository
        serviceCollection.AddServices();
    }

    public static void AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<TimerViewModel>();
        serviceCollection.AddTransient<MainWindowViewModel>();
        serviceCollection.AddSingleton<TimerConfigurationService>();
        serviceCollection.AddSingleton<ITimerConfigurationRepository, InMemoryTimerConfigurationRepository>(_ =>
            new InMemoryTimerConfigurationRepository());
        serviceCollection.AddSingleton<TimerService>();
    }

}