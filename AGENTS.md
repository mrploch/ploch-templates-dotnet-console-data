# Agent instructions

Instructions for AI coding agents working in this repository. Human contributors: see
[CONTRIBUTING.md](CONTRIBUTING.md).

## Repository

- .NET command-line application with an EF Core data layer, `Ploch.TemplateApp`, targeting the SDK in `global.json`.
- `src/Model` (entities on Ploch.Data.Model), `src/Data` (DbContext and configurations), `src/Data.SQLite` and
  `src/Data.SqlServer` (migrations and design-time factories), `src/UI.ConsoleApp` (Spectre.Console.Cli host).
- Data access goes through Ploch.Data's generic repositories and `IUnitOfWork`.
- Package versions are pinned centrally in `Directory.Packages.props`; never put a version in a `.csproj`.

## Commands

```bash
dotnet tool restore
dotnet build Ploch.TemplateApp.slnx -c Release
dotnet test Ploch.TemplateApp.slnx -c Release
dotnet run --project src/UI.ConsoleApp -- articles list
```

## Rules

- After changing the model, regenerate the migrations for **both** providers (`recreate-migrations.ps1` in each
  provider folder, or `dotnet ef migrations add`); `MigrationTests` fails when they drift from the model.
- Entities stay in `src/Model` with no EF Core dependency; mapping belongs in `src/Data/Configurations`.
- Register new commands in `ConsoleApplication.Create`; inject `IAnsiConsole` instead of using the static `AnsiConsole`.
- Release builds treat warnings as errors. Fix analyser warnings; do not suppress them without a written reason.
- Test methods are named `<Member>_should_<expected_behaviour>`; pass `TestContext.Current.CancellationToken` to async calls.
- New and changed code needs tests: CI requires 80% coverage of changed lines, and SonarCloud gates new code.
- Commits follow Conventional Commits. Add a `change-log/` entry for every user-visible change.
- Never commit credentials or connection strings with passwords. Secrets reach CI only through GitHub secrets.
