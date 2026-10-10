using Ploch.Data.EFCore;
using Ploch.Data.EFCore.SqlServer;
using Ploch.TemplateApp.Data;

namespace Ploch.TemplateApp.Data.SqlServer;

/// <summary>
/// Design-time factory for <see cref="TemplateAppDbContext"/> backed by SQL Server.
/// Used by the EF Core tools (<c>dotnet ef migrations add</c> / <c>database update</c>)
/// and by the helper scripts in this folder.
/// </summary>
public class TemplateAppDbContextFactory()
    : SqlServerDbContextFactory<TemplateAppDbContext, TemplateAppDbContextFactory>(
        static options => new(options, new DefaultDbContextCreationLifecycle()))
{
}
