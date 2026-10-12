using Microsoft.Data.Sqlite;

namespace Ploch.TemplateApp.UI.ConsoleApp.Infrastructure;

/// <summary>
///     Prepares the configured SQLite connection string for the running application.
/// </summary>
internal static class SqliteConnectionStrings
{
    /// <summary>
    ///     Anchors a relative database file path to <paramref name="directory" />, so the application uses the same
    ///     database whichever folder it is started from.
    /// </summary>
    /// <remarks>
    ///     Microsoft.Data.Sqlite gives some data sources a meaning of their own, and those are returned unchanged: an
    ///     empty value (a temporary database), <c>:memory:</c> or <c>Mode=Memory</c> (in-memory databases), a
    ///     <c>file:</c> URI and a path starting with <c>|DataDirectory|</c>. Rooted paths are returned unchanged too.
    /// </remarks>
    /// <param name="connectionString">The configured connection string.</param>
    /// <param name="directory">The folder relative database paths are resolved against.</param>
    /// <returns>The connection string to open.</returns>
    public static string AnchorRelativePath(string connectionString, string directory)
    {
        var connection = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = connection.DataSource;

        if (dataSource.Length == 0
            || dataSource == ":memory:"
            || connection.Mode == SqliteOpenMode.Memory
            || dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase)
            || Path.IsPathRooted(dataSource))
        {
            return connectionString;
        }

        connection.DataSource = Path.Combine(directory, dataSource);

        return connection.ConnectionString;
    }
}
