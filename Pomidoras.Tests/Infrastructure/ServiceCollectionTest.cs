using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pomidoras.Infrastructure;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.ViewModels;

namespace Pomidoras.Tests.Infrastructure;

public class ServiceCollectionTest
{

    private static readonly Dictionary<Type, ServiceLifetime> RegisteredServices = new()
    {
        { typeof(MainWindowViewModel), ServiceLifetime.Transient },
        { typeof(TimerConfigurationService), ServiceLifetime.Singleton },
        { typeof(ITimerConfigurationRepository), ServiceLifetime.Singleton }
    };

    [Fact]
    public void AddServices_AddsExpectedServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddServices();

        serviceCollection.Should().HaveCount(RegisteredServices.Count);

        serviceCollection.Should().SatisfyRespectively(descriptor =>
            {
                descriptor.ServiceType.Should().Be(typeof(MainWindowViewModel));
                descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
            },
            descriptor =>
            {
                descriptor.ServiceType.Should().Be(typeof(TimerConfigurationService));
                descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
            },
            descriptor =>
            {
                descriptor.ServiceType.Should().Be(typeof(ITimerConfigurationRepository));
                descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
            }
        );
    }

}