using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pomidoras.Infrastructure;
using Pomidoras.Models.Timer;
using Pomidoras.Models.Timer.Configuration;
using Pomidoras.ViewModels;

namespace Pomidoras.Tests.Infrastructure;

public class ServiceCollectionTest
{

    private static readonly List<(Type ServiceType, ServiceLifetime ServiceLifetime)> RegisteredServices =
    [
        (typeof(TimerViewModel), ServiceLifetime.Transient),
        (typeof(MainWindowViewModel), ServiceLifetime.Transient),
        (typeof(TimerConfigurationService), ServiceLifetime.Singleton),
        (typeof(ITimerConfigurationRepository), ServiceLifetime.Singleton),
        (typeof(TimerService), ServiceLifetime.Singleton)
    ];

    [Fact]
    public void AddServices_AddsExpectedServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddServices();

        serviceCollection.Should().SatisfyRespectively(descriptor =>
            {
                descriptor.ServiceType.Should().Be(RegisteredServices[0].ServiceType);
                descriptor.Lifetime.Should().Be(RegisteredServices[0].ServiceLifetime);
            },
            descriptor =>
            {
                descriptor.ServiceType.Should().Be(RegisteredServices[1].ServiceType);
                descriptor.Lifetime.Should().Be(RegisteredServices[1].ServiceLifetime);
            },
            descriptor =>
            {
                descriptor.ServiceType.Should().Be(RegisteredServices[2].ServiceType);
                descriptor.Lifetime.Should().Be(RegisteredServices[2].ServiceLifetime);
            },
            descriptor =>
            {
                descriptor.ServiceType.Should().Be(RegisteredServices[3].ServiceType);
                descriptor.Lifetime.Should().Be(RegisteredServices[3].ServiceLifetime);
            },
            descriptor =>
            {
                descriptor.ServiceType.Should().Be(RegisteredServices[4].ServiceType);
                descriptor.Lifetime.Should().Be(RegisteredServices[4].ServiceLifetime);
            }
        );
    }

}