using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore.SqLite;
using Ploch.Data.GenericRepository.EFCore;
using Ploch.TemplateApp.Data;
using Ploch.TemplateApp.UI.ConsoleApp.Commands;
using Ploch.TemplateApp.UI.ConsoleApp.Infrastructure;
using Spectre.Console.Cli;

namespace Ploch.TemplateApp.UI.ConsoleApp;

/// <summary>
///     Composes the command-line application: its services, its data layer and its commands.
/// </summary>
/// <remarks>
///     Program.cs supplies the real console and the configured database; tests supply a <c>TestConsole</c> and a
///     temporary SQLite database, so every command can be exercised end to end.
/// </remarks>
internal static class ConsoleApplication
{
    /// <summary>
    ///     Creates the command application.
    /// </summary>
    /// <param name="services">
    ///     The service collection, which must already contain the <see cref="Spectre.Console.IAnsiConsole" /> to write to.
    /// </param>
    /// <param name="configureDatabase">
    ///     Configures the database provider, connection and migrations assembly, for example
    ///     <c>options =&gt; options.UseSqlite(connectionString, sqlite =&gt; sqlite.MigrationsAssembly(name))</c>.
    /// </param>
    /// <returns>The configured <see cref="CommandApp" />.</returns>
    public static CommandApp Create(IServiceCollection services, Action<DbContextOptionsBuilder> configureDatabase)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDatabase);

        // Registers the DbContext with Ploch.Data's generic repositories and unit of work. The SQLite creation
        // lifecycle adds the value conversions SQLite needs (for example DateTimeOffset); use
        // DefaultDbContextCreationLifecycle when switching to SQL Server.
        services.AddDbContextWithRepositories<TemplateAppDbContext, SqLiteDbContextCreationLifecycle>(configureDatabase);

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config =>
        {
            config.SetApplicationName("template-app");
            config.AddBranch("articles", articles =>
            {
                articles.SetDescription("Manages articles.");
                articles.AddCommand<AddArticleCommand>("add")
                        .WithDescription("Adds an article, optionally with its author.")
                        .WithExample("articles", "add", "\"Getting started\"", "--author", "Ada");
                articles.AddCommand<ListArticlesCommand>("list")
                        .WithDescription("Lists every article.");
            });
        });

        return app;
    }
}
