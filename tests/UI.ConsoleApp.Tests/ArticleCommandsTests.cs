using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.TemplateApp.Data.SQLite;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace Ploch.TemplateApp.UI.ConsoleApp.Tests;

/// <summary>
///     End-to-end tests: each test runs the real command application against a fresh SQLite database file.
/// </summary>
public sealed class ArticleCommandsTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

    private string ConnectionString => $"DataSource={_databasePath};Pooling=False";

    [Fact]
    public async Task List_should_report_an_empty_database()
    {
        var (exitCode, output) = await RunAsync("articles", "list");

        exitCode.Should().Be(0);
        output.Should().Contain("No articles yet.");
    }

    [Fact]
    public async Task Add_then_list_should_show_the_article_and_its_author()
    {
        (await RunAsync("articles", "add", "Getting started", "--author", "Ada")).ExitCode.Should().Be(0);
        (await RunAsync("articles", "add", "Second steps", "--author", "Ada")).ExitCode.Should().Be(0);

        var (exitCode, output) = await RunAsync("articles", "list");

        exitCode.Should().Be(0);
        output.Should().Contain("Getting started").And.Contain("Second steps").And.Contain("Ada");
    }

    [Fact]
    public async Task Add_should_report_the_new_article_id()
    {
        var (exitCode, output) = await RunAsync("articles", "add", "Without an author");

        exitCode.Should().Be(0);
        output.Should().Contain("Added article 1:").And.Contain("Without an author");
    }

    [Fact]
    public async Task Add_should_fail_when_the_title_is_blank()
    {
        // These tests propagate exceptions (see RunAsync), so a validation failure surfaces as the exception that the
        // application would otherwise render as an error message with a non-zero exit code.
        var act = () => RunAsync("articles", "add", " ");

        await act.Should().ThrowAsync<CommandRuntimeException>().WithMessage("*The title must not be empty.*");
    }

    public void Dispose()
    {
        foreach (var path in new[] { _databasePath, $"{_databasePath}-journal", $"{_databasePath}-wal", $"{_databasePath}-shm" })
        {
            File.Delete(path);
        }
    }

    private async Task<(int ExitCode, string Output)> RunAsync(params string[] args)
    {
        using var console = new TestConsole();
        console.Profile.Width = 200;
        var services = new ServiceCollection().AddSingleton<IAnsiConsole>(console);
        var migrationsAssembly = typeof(TemplateAppDbContextFactory).Assembly.GetName().Name;
        var app = ConsoleApplication.Create(
            services,
            options => options.UseSqlite(ConnectionString, sqlite => sqlite.MigrationsAssembly(migrationsAssembly)));
        app.Configure(config =>
        {
            config.Settings.Console = console;

            // Surface unexpected exceptions as test failures instead of rendered error text.
            config.PropagateExceptions();
        });

        var exitCode = await app.RunAsync(args, TestContext.Current.CancellationToken);

        return (exitCode, console.Output);
    }
}
