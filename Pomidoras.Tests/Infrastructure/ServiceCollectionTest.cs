using Microsoft.Extensions.DependencyInjection;
using Pomidoras.Infrastructure;
using Pomidoras.ViewModels;

namespace Pomidoras.Tests.Infrastructure;

public class ServiceCollectionTest
{

    public static TheoryData<Type, ServiceLifetime> GetRegisteredServiceTypes()
    {
        var theoryData = new TheoryData<Type, ServiceLifetime>();
        theoryData.Add(typeof(MainWindowViewModel), ServiceLifetime.Transient);
        return theoryData;
    }
    
    [Fact]
    public void ServiceProvider_CanBeBuilt_WithoutErrors()
    {
        var services = new ServiceCollection();
        services.AddServices();
        
        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider);
    }

    [Theory]
    [MemberData(nameof(GetRegisteredServiceTypes))]
    public void MainWindowViewModel_CanBeResolved(Type registeredServiceType, ServiceLifetime registeredServiceLifetime)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddServices();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var actualService = serviceProvider.GetService(registeredServiceType);
        
        Assert.NotNull(actualService);
        Assert.IsType(registeredServiceType, actualService);
        Assert.Equal(registeredServiceLifetime, registeredServiceLifetime);
    }

}