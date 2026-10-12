using Microsoft.Data.Sqlite;
using Ploch.TemplateApp.UI.ConsoleApp.Infrastructure;

namespace Ploch.TemplateApp.UI.ConsoleApp.Tests;

public class SqliteConnectionStringsTests
{
    private static readonly string Directory = Path.Combine(Path.GetTempPath(), "app");

    [Theory]
    [InlineData("Data Source=template-app.db", "template-app.db")]
    [InlineData("DataSource=data/app.db;Cache=Shared", "data/app.db")]
    [InlineData("Data Source=\"my app.db\"", "my app.db")]
    public void AnchorRelativePath_should_resolve_a_relative_file_against_the_directory(string connectionString, string relativePath)
    {
        var result = SqliteConnectionStrings.AnchorRelativePath(connectionString, Directory);

        new SqliteConnectionStringBuilder(result).DataSource.Should().Be(Path.Combine(Directory, relativePath));
    }

    [Fact]
    public void AnchorRelativePath_should_keep_the_other_connection_settings()
    {
        var result = SqliteConnectionStrings.AnchorRelativePath("Data Source=app.db;Cache=Shared;Pooling=False", Directory);

        var connection = new SqliteConnectionStringBuilder(result);
        connection.Cache.Should().Be(SqliteCacheMode.Shared);
        connection.Pooling.Should().BeFalse();
    }

    [Theory]
    [InlineData("Data Source=")]
    [InlineData("Data Source=:memory:")]
    [InlineData("Data Source=shared;Mode=Memory")]
    [InlineData("Data Source=file:app.db?mode=ro")]
    [InlineData("Data Source=FILE:app.db")]
    [InlineData("Data Source=|DataDirectory|app.db")]
    [InlineData("Data Source=|datadirectory|app.db")]
    public void AnchorRelativePath_should_leave_special_data_sources_unchanged(string connectionString)
    {
        SqliteConnectionStrings.AnchorRelativePath(connectionString, Directory).Should().Be(connectionString);
    }

    [Fact]
    public void AnchorRelativePath_should_leave_a_rooted_path_unchanged()
    {
        var connectionString = $"Data Source={Path.Combine(Path.GetTempPath(), "rooted.db")}";

        SqliteConnectionStrings.AnchorRelativePath(connectionString, Directory).Should().Be(connectionString);
    }
}
