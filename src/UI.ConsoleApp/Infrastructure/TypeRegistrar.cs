using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Ploch.TemplateApp.UI.ConsoleApp.Infrastructure;

/// <summary>
///     Lets Spectre.Console.Cli create commands through Microsoft.Extensions.DependencyInjection.
/// </summary>
/// <param name="services">The application's service collection.</param>
internal sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    /// <inheritdoc />
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());

    /// <inheritdoc />
    public void Register(Type service, Type implementation) => services.AddSingleton(service, implementation);

    /// <inheritdoc />
    public void RegisterInstance(Type service, object implementation) => services.AddSingleton(service, implementation);

    /// <inheritdoc />
    public void RegisterLazy(Type service, Func<object> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        services.AddSingleton(service, _ => factory());
    }
}
