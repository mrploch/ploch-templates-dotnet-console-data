using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ploch.Data.EFCore;
using Ploch.Data.EFCore.SqLite;
using Ploch.TemplateApp.Data;
using Ploch.TemplateApp.Model;
using SqLiteFactory = Ploch.TemplateApp.Data.SQLite.TemplateAppDbContextFactory;
using SqlServerFactory = Ploch.TemplateApp.Data.SqlServer.TemplateAppDbContextFactory;

namespace Ploch.TemplateApp.Data.IntegrationTests;

/// <summary>
///     Guards the committed migrations: they must match the model and apply cleanly.
/// </summary>
public class MigrationTests
{
    /// <summary>
    ///     The environment variable holding a SQL Server connection string. SQL Server tests run only when it is set.
    /// </summary>
    private const string SqlServerConnectionStringVariable = "SQLSERVER_TEST_CONNECTION_STRING";

    [Fact]
    public void SQLite_migrations_should_match_the_model()
    {
        using var dbContext = CreateSqLiteContext("DataSource=:memory:");

        dbContext.Database.HasPendingModelChanges().Should().BeFalse("run src/Data.SQLite/recreate-migrations.ps1 after changing the model");
    }

    [Fact]
    public void SqlServer_migrations_should_match_the_model()
    {
        // Comparing the model with the migrations needs no server: nothing connects to this placeholder address.
        using var dbContext = CreateSqlServerContext("Server=localhost;Database=ModelCheck;Integrated Security=True;TrustServerCertificate=True");

        dbContext.Database.HasPendingModelChanges().Should().BeFalse("run src/Data.SqlServer/recreate-migrations.ps1 after changing the model");
    }

    [Fact]
    public async Task SQLite_migrations_should_create_a_working_database()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using (var dbContext = CreateSqLiteContext($"DataSource={path};Pooling=False"))
            {
                await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
                dbContext.Articles.Add(new Article { Title = "Migrated", Author = new Author { Name = "Ada" } });
                await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            await using var verifyContext = CreateSqLiteContext($"DataSource={path};Pooling=False");
            var saved = await verifyContext.Articles.Include(article => article.Author).SingleAsync(TestContext.Current.CancellationToken);
            saved.Author!.Name.Should().Be("Ada");
        }
        finally
        {
            foreach (var file in new[] { path, $"{path}-journal", $"{path}-wal", $"{path}-shm" })
            {
                File.Delete(file);
            }
        }
    }

    [Fact]
    public async Task SqlServer_migrations_should_create_a_working_database()
    {
        var connectionString = Environment.GetEnvironmentVariable(SqlServerConnectionStringVariable);
        Assert.SkipWhen(string.IsNullOrWhiteSpace(connectionString), $"Set {SqlServerConnectionStringVariable} to run the SQL Server tests.");

        // The test owns a uniquely named database, so it never touches the database named in the variable and
        // concurrent runs cannot interfere with each other.
        var isolated = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = $"MigrationTest_{Guid.NewGuid():N}" };

        await using var dbContext = CreateSqlServerContext(isolated.ConnectionString);
        try
        {
            await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
            dbContext.Articles.Add(new Article { Title = "Migrated" });
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            (await dbContext.Articles.CountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
        }
        finally
        {
            // Cleanup must run even when the test was cancelled, so it does not use the test's token.
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await dbContext.Database.EnsureDeletedAsync(cleanup.Token);
        }
    }

    private static TemplateAppDbContext CreateSqLiteContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<TemplateAppDbContext>()
                      .UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(typeof(SqLiteFactory).Assembly.GetName().Name))
                      .Options;

        return new TemplateAppDbContext(options, new SqLiteDbContextCreationLifecycle());
    }

    private static TemplateAppDbContext CreateSqlServerContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<TemplateAppDbContext>()
                      .UseSqlServer(connectionString, sqlServer => sqlServer.MigrationsAssembly(typeof(SqlServerFactory).Assembly.GetName().Name))
                      .Options;

        return new TemplateAppDbContext(options, new DefaultDbContextCreationLifecycle());
    }
}
