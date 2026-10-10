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
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true }));

    // Commands and their settings are created per run, so they are transient: a command never outlives the scope
    // that supplies its dependencies (such as a scoped DbContext).

    /// <inheritdoc />
    public void Register(Type service, Type implementation) => services.AddTransient(service, implementation);

    /// <inheritdoc />
    public void RegisterInstance(Type service, object implementation) => services.AddSingleton(service, implementation);

    /// <inheritdoc />
    public void RegisterLazy(Type service, Func<object> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        services.AddSingleton(service, _ => factory());
    }
}
