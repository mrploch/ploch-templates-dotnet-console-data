using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Ploch.TemplateApp.UI.ConsoleApp.Infrastructure;

/// <summary>
///     Resolves Spectre.Console.Cli types from one dependency-injection scope per application run, and disposes the
///     scope and the provider with the application.
/// </summary>
/// <remarks>
///     Resolving from a scope rather than the root provider gives scoped services (a DbContext, a unit of work) a
///     proper lifetime, and lets the provider's scope validation catch a scoped service captured by a singleton.
/// </remarks>
/// <param name="provider">The service provider built from the application's services.</param>
internal sealed class TypeResolver(ServiceProvider provider) : ITypeResolver, IDisposable
{
    private readonly IServiceScope _scope = provider.CreateScope();

    /// <inheritdoc />
    public object? Resolve(Type? type) => type is null ? null : _scope.ServiceProvider.GetService(type);

    /// <inheritdoc />
    public void Dispose()
    {
        _scope.Dispose();
        provider.Dispose();
    }
}
