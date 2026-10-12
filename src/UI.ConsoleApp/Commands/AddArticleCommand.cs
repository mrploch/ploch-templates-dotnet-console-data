using System.ComponentModel;
using Ploch.Data.GenericRepository;
using Ploch.TemplateApp.Data;
using Ploch.TemplateApp.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.TemplateApp.UI.ConsoleApp.Commands;

/// <summary>
///     The <c>articles add</c> command: stores an article, and its author when one is named.
/// </summary>
internal sealed class AddArticleCommand(IAnsiConsole console, TemplateAppDbContext dbContext, IUnitOfWork unitOfWork)
    : DatabaseCommand<AddArticleCommand.Settings>(dbContext)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteWithDatabaseAsync(Settings settings, CancellationToken cancellationToken)
    {
        var article = new Article { Title = settings.Title.Trim() };

        if (!string.IsNullOrWhiteSpace(settings.Author))
        {
            var authorName = settings.Author.Trim();

            // Reuse an existing author with the same name rather than creating a duplicate. An existing author is
            // linked by its key, so adding the article never re-inserts an author that is already stored.
            var existing = await unitOfWork.Repository<Author, int>()
                                           .FindFirstAsync(author => author.Name == authorName, cancellationToken: cancellationToken);
            if (existing is not null)
            {
                article.AuthorId = existing.Id;
            }
            else
            {
                article.Author = new Author { Name = authorName };
            }
        }

        await unitOfWork.Repository<Article, int>().AddAsync(article, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        console.MarkupLineInterpolated($"[green]Added article {article.Id}:[/] {article.Title}");

        return 0;
    }

    /// <summary>
    ///     The arguments and options of the <c>articles add</c> command.
    /// </summary>
    internal sealed class Settings : CommandSettings
    {
        /// <summary>
        ///     Gets the title of the article.
        /// </summary>
        [CommandArgument(0, "<title>")]
        [Description("The title of the article.")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the name of the article's author, if any.
        /// </summary>
        [CommandOption("-a|--author <NAME>")]
        [Description("The name of the article's author.")]
        public string? Author { get; init; }

        /// <inheritdoc />
        public override ValidationResult Validate()
        {
            // The limits come from the model, so SQLite (which does not enforce them) and SQL Server agree.
            if (string.IsNullOrWhiteSpace(Title))
            {
                return ValidationResult.Error("The title must not be empty.");
            }

            if (Title.Trim().Length > Article.TitleMaxLength)
            {
                return ValidationResult.Error($"The title must not be longer than {Article.TitleMaxLength} characters.");
            }

            if (Author is not null && Author.Trim().Length > Model.Author.NameMaxLength)
            {
                return ValidationResult.Error($"The author name must not be longer than {Model.Author.NameMaxLength} characters.");
            }

            return ValidationResult.Success();
        }
    }
}
