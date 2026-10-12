using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository;
using Ploch.TemplateApp.Data;
using Ploch.TemplateApp.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.TemplateApp.UI.ConsoleApp.Commands;

/// <summary>
///     The <c>articles list</c> command: prints every article as a table.
/// </summary>
internal sealed class ListArticlesCommand(IAnsiConsole console, TemplateAppDbContext dbContext, IUnitOfWork unitOfWork)
    : DatabaseCommand<ListArticlesCommand.Settings>(dbContext)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteWithDatabaseAsync(Settings settings, CancellationToken cancellationToken)
    {
        var articles = await unitOfWork.Repository<Article, int>()
                                       .GetAllAsync(onDbSet: query => query.Include(article => article.Author).OrderBy(article => article.Id),
                                                    cancellationToken: cancellationToken);

        if (articles.Count == 0)
        {
            console.MarkupLine("[yellow]No articles yet.[/] Add one with [bold]articles add <title>[/].");

            return 0;
        }

        var table = new Table().AddColumns("Id", "Title", "Author", "Created (UTC)");
        foreach (var article in articles)
        {
            table.AddRow(
                article.Id.ToString(CultureInfo.InvariantCulture),
                Markup.Escape(article.Title),
                Markup.Escape(article.Author?.Name ?? "-"),
                article.CreatedTime?.ToString("u", CultureInfo.InvariantCulture) ?? "-");
        }

        console.Write(table);

        return 0;
    }

    /// <summary>
    ///     The <c>articles list</c> command has no arguments or options.
    /// </summary>
    internal sealed class Settings : CommandSettings
    {
    }
}
