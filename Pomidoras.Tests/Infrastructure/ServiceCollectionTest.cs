using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pomidoras.Infrastructure;
using Pomidoras.Models.Timer;
using Pomidoras.ViewModels;
using Pomidoras.Views;

namespace Pomidoras.Tests.Infrastructure;

public class ServiceCollectionTest
{

    private static readonly Dictionary<Type, ServiceLifetime> RegisteredServices = new() {
        { typeof(MainWindowViewModel), ServiceLifetime.Transient },
        { typeof(TimerConfigurationService), ServiceLifetime.Singleton },
    };

    public static TheoryData<Type> GetRegisteredServiceTypes()
    {
        return new TheoryData<Type>(RegisteredServices.Keys);
    }
    
    [Fact]
    public void ServiceProvider_CanBeBuilt_WithoutErrors()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddServices();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        serviceProvider.Should().NotBeNull();
    }
    
    [Fact]
    public void AddServices_AddsExpectedServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddServices();
        
        serviceCollection.Should().HaveCount(RegisteredServices.Count);
        
        foreach (var (serviceType, lifetime) in RegisteredServices)
        {
            var descriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == serviceType);

            descriptor.Should().NotBeNull(because: $"{serviceType.Name} should be registered");
            descriptor.Lifetime.Should().Be(lifetime, because: $"{serviceType.Name} should have lifetime: {lifetime}");
        }
    }

    [Theory]
    [MemberData(nameof(GetRegisteredServiceTypes))]
    public void RegisteredServiceTypes_CanBeResolved(Type registeredServiceType)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddServices();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var actualService = serviceProvider.GetService(registeredServiceType);
        
        actualService.Should().NotBeNull();
        actualService.Should().BeOfType(registeredServiceType);
    }

}