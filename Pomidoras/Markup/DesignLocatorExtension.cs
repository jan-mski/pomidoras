using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Pomidoras.Markup;

/// <summary>
/// A markup extension that provides dependency injection for design-time data in Avalonia XAML.
/// Retrieves services from the application's service provider during design mode only.
/// </summary>
/// <param name="type">The type of the service to resolve from the dependency injection container.</param>
/// <exception cref="InvalidOperationException">
/// Thrown when used outside of design mode or when the service provider is not initialized.
/// </exception>
/// <example>
/// Usage in XAML:
/// <code>
/// &lt;Window xmlns:markup="using:Pomidoras.Markup"
///         DataContext="{markup:DesignLocator Type={x:Type vm:MainWindowViewModel}}" /&gt;
/// </code>
/// </example>
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