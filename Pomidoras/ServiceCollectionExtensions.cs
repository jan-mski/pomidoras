using Microsoft.Extensions.DependencyInjection;
using Pomidoras.ViewModels;

namespace Pomidoras;

public static class ServiceCollectionExtensions
{

    public static void AddServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
    }    

}