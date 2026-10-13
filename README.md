# Ploch.TemplateApp

[![Build](https://github.com/mrploch/ploch-templates-dotnet-console-data/actions/workflows/build.yml/badge.svg)](https://github.com/mrploch/ploch-templates-dotnet-console-data/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mrploch_ploch-templates-dotnet-console-data&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mrploch_ploch-templates-dotnet-console-data)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrploch_ploch-templates-dotnet-console-data&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mrploch_ploch-templates-dotnet-console-data)
[![Licence](https://img.shields.io/github/license/mrploch/ploch-templates-dotnet-console-data)](LICENSE)

<!-- template-only:start -->
> [!NOTE]
> **This is a GitHub template repository** for a .NET command-line application with an EF Core data layer built
> on [Ploch.Data](https://github.com/mrploch/ploch-data). Everything between the `template-only` markers in this
> file is removed when a new repository is initialised; the rest becomes the new repository's README.

## Using this template

1. Create a repository from this template: **Use this template → Create a new repository** on GitHub, or
   `gh repo create mrploch/<name> --public --template mrploch/ploch-templates-dotnet-console-data --clone`.
2. Initialise it, then review, build and commit the result:

   ```bash
   pwsh ./scripts/Initialize-Repository.ps1 -Name Ploch.<Product> -WhatIf   # preview
   pwsh ./scripts/Initialize-Repository.ps1 -Name Ploch.<Product>
   dotnet build -c Release && dotnet test -c Release
   git add --all && git commit -m "chore: initialise repository from template"
   ```

   The script renames the solution, projects, namespaces, the DbContext and its migrations from
   `Ploch.TemplateApp` and `ploch-templates-dotnet-console-data`, removes these template-only sections, then deletes
   itself and the Template Bootstrap workflow that tests it.
3. Complete the [owner setup](#owner-setup) below, then replace the sample `Article`/`Author` model with the
   application's own and regenerate the migrations (see [Database migrations](#database-migrations)).

## What the template provides

| Area | What you get |
|---|---|
| Data | A `Model` project on Ploch.Data.Model, a `Data` project with the DbContext and entity configurations, and SQLite and SQL Server provider projects with committed migrations; Ploch.Data's generic repository and unit of work |
| Application | A [Spectre.Console.Cli](https://spectreconsole.net/cli/) host with dependency injection and `articles add` / `articles list` commands that migrate the database on start-up |
| Tests | Integration tests on Ploch.Data's in-memory SQLite harness, migration checks for both providers, an opt-in SQL Server test, and end-to-end command tests against a `TestConsole` |
| Build, CI, coverage, code quality | As in the other mrploch templates: pinned SDK, central package management, analysers with warnings as errors, `build.yml` with coverage, a changed-lines threshold and SonarCloud, and the PR Report workflow |
| Hygiene | Dependabot (NuGet, SDK, Actions), issue forms, PR template, CODEOWNERS, CONTRIBUTING, SECURITY, agent instructions |

Every package restores from nuget.org without credentials, so fork and Dependabot pull requests build like any
other. Ploch's command-line library (ploch-commandline) is not used yet because it is published only to GitHub
Packages, which needs a token.

## Keeping the template current

The shared files (workflows, build properties, analyser settings) are maintained canonically in
[mrploch-development](https://github.com/mrploch/mrploch-development) and synced into the template repositories.
Repositories created from the template are independent copies: to adopt a later template change, compare the
file with the template and copy the change.
<!-- template-only:end -->

## Getting started

```bash
dotnet tool restore
dotnet build -c Release
dotnet test -c Release
dotnet run --project src/UI.ConsoleApp -- articles add "Getting started" --author Ada
dotnet run --project src/UI.ConsoleApp -- articles list
```

The application stores its data in the SQLite database named by `ConnectionStrings:DefaultConnection` in
`src/Data.SQLite/appsettings.json`. That one file serves both the EF Core tools and the application: it is copied
next to the executable, and a relative path such as `template-app.db` resolves against the executable's folder, not
the current directory.

Every command brings the schema up to date (`Database.MigrateAsync`) before it runs. On SQLite that check costs a
few milliseconds and keeps a fresh checkout working with no setup; an application that must not change its schema
implicitly can move the call into a dedicated `database update` command.

`ConsoleApplication.Create` takes the database configuration from its caller, so `Program.cs` is the one place that
chooses the provider. To switch to SQL Server, replace the application's `src/Data.SQLite` reference with
`src/Data.SqlServer` (whose `appsettings.json` then becomes the application's configuration), call
`UseSqlServer` with that project's migrations assembly in `Program.cs`, and register
`DefaultDbContextCreationLifecycle` instead of `SqLiteDbContextCreationLifecycle` in `ConsoleApplication`.

## Project layout

| Project | Purpose |
|---|---|
| `src/Model` | Entities (POCOs implementing Ploch.Data.Model interfaces); no EF Core dependency |
| `src/Data` | `TemplateAppDbContext` and the `IEntityTypeConfiguration<>` classes |
| `src/Data.SQLite` | SQLite migrations, the design-time factory used by `dotnet ef`, and the connection string |
| `src/Data.SqlServer` | SQL Server migrations and design-time factory |
| `src/UI.ConsoleApp` | The executable: `ConsoleApplication` (services, data layer, commands) and `Commands/` |
| `tests/Data.IntegrationTests` | Repository and unit-of-work tests, and migration checks for both providers |
| `tests/UI.ConsoleApp.Tests` | End-to-end command tests against a temporary SQLite database |

## Database migrations

Both providers have their own migrations, and a test fails when either set no longer matches the model. After
changing the model, regenerate them:

```bash
pwsh src/Data.SQLite/recreate-migrations.ps1      # or: cd src/Data.SQLite && dotnet ef migrations add <Name>
pwsh src/Data.SqlServer/recreate-migrations.ps1
```

`recreate-migrations.ps1` replaces the migrations with a single fresh `Initial` migration, which suits a project
that has not shipped yet; once databases exist in the field, add incremental migrations with `dotnet ef migrations add`.

The SQL Server migration test runs only when the `SQLSERVER_TEST_CONNECTION_STRING` environment variable holds a
connection string; it creates and then deletes its database. CI does not set it, so the test is reported as skipped.

## Continuous integration

| Workflow | Runs on | Does |
|---|---|---|
| [Build](.github/workflows/build.yml) | push to `main`, pull requests, manual | Builds and tests in Release, measures coverage, runs SonarCloud |
| [PR Report](.github/workflows/pr-report.yml) | after every Build run | Publishes the "Test Results" check and the sticky coverage comment |

### Code coverage

Coverage is collected by coverlet in every test project and reported in three places that use the same
exclusions, so they agree:

- **The job summary of every Build run**: overall line and branch coverage, with a per-assembly and per-class table.
- **One comment on each pull request**, updated in place on every push: coverage of the lines the pull request
  changes, listing each file's uncovered lines, followed by the overall summary. Pull requests must cover at
  least **80%** of their changed lines; set the `DIFF_COVERAGE_THRESHOLD` repository variable to change that.
- **SonarCloud**, whose quality gate checks coverage of new code.

The full HTML report is attached to every Build run as the `coverage-report` artefact. Files excluded from
coverage (generated code and migrations) are listed once, in `COVERAGE_EXCLUDE` in `build.yml`.

## Owner setup

These steps need repository or organisation administrator access and cannot be automated from the repository.

### SonarCloud

1. In [SonarCloud](https://sonarcloud.io/projects/create), choose **Analyze new project**, select the `mrploch`
   organisation and this repository. The project key must be `mrploch_ploch-templates-dotnet-console-data`
   (or set the `SONAR_PROJECT_KEY` repository variable to the key you chose).
2. In the project's **Administration → Analysis Method**, turn **Automatic Analysis off**. CI-based analysis fails
   while it is on.
3. In **Administration → New Code**, select **Previous version**.
4. Provide a token as the `SONAR_TOKEN` secret: an organisation-level secret is inherited by every repository;
   otherwise generate one under **My Account → Security** and add it under **Settings → Secrets and variables →
   Actions**. The organisation defaults to the repository owner; set the `SONAR_ORGANIZATION` variable to override it.

Until `SONAR_TOKEN` exists and the SonarCloud project has been created, the Build workflow skips SonarCloud with
a warning rather than failing, so a new repository is green from its first push. A token that SonarCloud rejects as
invalid or expired (HTTP 401) also skips SonarCloud with a warning, so the build and tests still run; rotate the token
to restore analysis. A valid token whose account lacks Browse permission on the project (HTTP 403), or any other
unexpected SonarCloud response, fails the build.

### Security reporting

Under **Settings → Code security**, enable **Private vulnerability reporting**. `SECURITY.md` and the issue-form
contact link send reporters there; with it disabled, they have no private channel.

### Branch protection

Protect `main` and require the **Build, test and analyse** and **Test Results** checks. Do not require the
external **SonarCloud Code Analysis** check: fork and Dependabot pull requests skip SonarCloud, so they would never
report it. The SonarCloud quality gate is enforced inside **Build, test and analyse** instead, which waits for the
gate and fails when it fails.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Report security issues privately as described in [SECURITY.md](SECURITY.md).

## Licence

[Apache 2.0](LICENSE)
