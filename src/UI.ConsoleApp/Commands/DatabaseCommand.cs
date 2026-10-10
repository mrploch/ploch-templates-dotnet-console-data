using Microsoft.EntityFrameworkCore;
using Ploch.TemplateApp.Data;
using Spectre.Console.Cli;

namespace Ploch.TemplateApp.UI.ConsoleApp.Commands;

/// <summary>
///     A command that uses the database. Brings the database schema up to date before the command runs.
/// </summary>
/// <typeparam name="TSettings">The command's settings.</typeparam>
/// <param name="dbContext">The application database.</param>
internal abstract class DatabaseCommand<TSettings>(TemplateAppDbContext dbContext) : AsyncCommand<TSettings>
    where TSettings : CommandSettings
{
    /// <summary>
    ///     Gets the application database.
    /// </summary>
    protected TemplateAppDbContext DbContext { get; } = dbContext;

    /// <inheritdoc />
    public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        await DbContext.Database.MigrateAsync(cancellationToken);

        return await ExecuteWithDatabaseAsync(settings, cancellationToken);
    }

    /// <summary>
    ///     Runs the command once the database is up to date.
    /// </summary>
    /// <param name="settings">The command's settings.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The process exit code.</returns>
    protected abstract Task<int> ExecuteWithDatabaseAsync(TSettings settings, CancellationToken cancellationToken);
}
