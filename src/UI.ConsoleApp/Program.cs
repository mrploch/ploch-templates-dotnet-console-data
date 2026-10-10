using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore;
using Ploch.TemplateApp.UI.ConsoleApp;
using Spectre.Console;

// The connection string comes from appsettings.json next to the executable (ConnectionStrings:DefaultConnection).
var connectionString = ConnectionString.FromJsonFile()()
                       ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing from appsettings.json.");

var services = new ServiceCollection().AddSingleton(AnsiConsole.Console);

return await ConsoleApplication.Create(services, connectionString).RunAsync(args);
