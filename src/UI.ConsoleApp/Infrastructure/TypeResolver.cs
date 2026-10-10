using Spectre.Console.Cli;

namespace Ploch.TemplateApp.UI.ConsoleApp.Infrastructure;

/// <summary>
///     Resolves Spectre.Console.Cli types from the built service provider, and disposes it with the application.
/// </summary>
/// <param name="provider">The service provider built from the application's services.</param>
internal sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    /// <inheritdoc />
    public object? Resolve(Type? type) => type is null ? null : provider.GetService(type);

    /// <inheritdoc />
    public void Dispose() => (provider as IDisposable)?.Dispose();
}
