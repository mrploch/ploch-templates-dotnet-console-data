using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore;
using Ploch.TemplateApp.Data.SQLite;
using Ploch.TemplateApp.UI.ConsoleApp;
using Ploch.TemplateApp.UI.ConsoleApp.Infrastructure;
using Spectre.Console;

// ConnectionStrings:DefaultConnection comes from appsettings.json, which src/Data.SQLite supplies.
var configured = ConnectionString.FromJsonFile()()
                 ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing from appsettings.json.");

// A relative SQLite path would otherwise resolve against the current directory, so the application would use a
// different database depending on where it was started. Anchor it to the executable's folder instead.
var connectionString = SqliteConnectionStrings.AnchorRelativePath(configured, AppContext.BaseDirectory);

var migrationsAssembly = typeof(TemplateAppDbContextFactory).Assembly.GetName().Name;

var services = new ServiceCollection().AddSingleton(AnsiConsole.Console);

return await ConsoleApplication.Create(
                                   services,
                                   options => options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(migrationsAssembly)))
                               .RunAsync(args);
