using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore.SqLite;
using Ploch.Data.GenericRepository.EFCore;
using Ploch.TemplateApp.Data;
using Ploch.TemplateApp.Data.SQLite;
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
    /// <param name="connectionString">The SQLite connection string of the application database.</param>
    /// <returns>The configured <see cref="CommandApp" />.</returns>
    public static CommandApp Create(IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Registers the DbContext with Ploch.Data's generic repositories and unit of work. Migrations live in the
        // Data.SQLite project, so the migrations assembly is named explicitly.
        var migrationsAssembly = typeof(TemplateAppDbContextFactory).Assembly.GetName().Name;
        services.AddDbContextWithRepositories<TemplateAppDbContext, SqLiteDbContextCreationLifecycle>(
            options => options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(migrationsAssembly)));

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
