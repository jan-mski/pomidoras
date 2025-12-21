using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Pomidoras.Markup;

public class DesignLocatorExtension(Type type) : MarkupExtension
{

    private Type Type { get; } = type;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("Should only be used in design mode.");
        }

        return App.ServiceProvider is null
            ? throw new InvalidOperationException("Service provider is null.")
            : App.ServiceProvider.GetRequiredService(Type);
    }

}