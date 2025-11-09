using Microsoft.Extensions.DependencyInjection;
using Pomidoras.ViewModels;

namespace Pomidoras.Infrastructure;

public static class ServiceCollectionExtensions
{

    public static void AddServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
    }    

}