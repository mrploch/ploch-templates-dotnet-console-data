using Ploch.Data.EFCore.SqLite;
using Ploch.TemplateApp.Data;

namespace Ploch.TemplateApp.Data.SQLite;

/// <summary>
/// Design-time factory for <see cref="TemplateAppDbContext"/> backed by SQLite.
/// Used by the EF Core tools (<c>dotnet ef migrations add</c> / <c>database update</c>)
/// and by the helper scripts in this folder.
/// </summary>
public class TemplateAppDbContextFactory()
    : SqLiteDbContextFactory<TemplateAppDbContext, TemplateAppDbContextFactory>(static options => new(options, CreationLifecycle))
{
}
