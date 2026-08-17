# Dad-A-Base Project Map

> **Status:** Living project document. Update this file whenever the repository's architecture, behavior, configuration, testing, automation, or Copilot customization changes.
>
> **Purpose:** Give Copilot a fast, evidence-based map of the repository so routine work can start at the owning project instead of scanning the whole tree.
>
> **Last reviewed:** 2026-08-17

## 1. Project Identity

Dad-A-Base is a .NET 10 demonstration and working application for storing, browsing, searching, rating, exporting, and administering dad jokes. It also demonstrates Azure hosting, infrastructure as code, SQL schema delivery, CI/CD, Playwright testing, AI integrations, Azure Functions, command-line tooling, and MCP servers.

The public-facing product is the Blazor Server web application. The other projects provide alternate API, data-processing, automation, and agent-integration surfaces around the same joke domain.

Authoritative sources when this document and older prose disagree:

1. Current project files (`*.csproj`, `Program.cs`, source code, and workflow YAML).
2. `README.md`, `PRODUCT.md`, `CONTRIBUTING.md`, and the relevant focused documentation.
3. `MAP.md` (keep this updated after the sources above change).
4. Older generated architecture exports or historical notes, which may contain stale versions or paths.

## 2. Fast Routing Guide

| Task | Start here | Then inspect |
| --- | --- | --- |
| Change a web page or Blazor behavior | `src/web/Website/Pages/`, `Components/`, or `Shared/` | `src/web/Website/Program.cs`, related `.razor.cs`, and scoped `.razor.css` |
| Change joke persistence or query behavior | `src/web/Data/Repositories/` and `src/web/Data/` | `IJokeRepository.cs`, `JokeSQLRepository.cs`, `JokeJsonRepository.cs`, `DadABaseDbContext.cs` |
| Change web REST endpoints | `src/web/Website/API/` | controller base classes, repository interface, Swagger configuration |
| Change the serverless API | `src/function/Function/` | `src/function/DataLayer/`, `Entities/`, and function tests |
| Change the database schema | `src/sql.database/` | SQL object files, pre/post deployment scripts, `.github/instructions/sql-database-dacpac-instructions.md` |
| Change Azure resources | `infra/Bicep/main.bicep` and `infra/Bicep/modules/` | `main.bicepparam`, `azure.yaml`, pipeline variable templates |
| Change GitHub deployment | `.github/workflows/` | reusable `template-*.yml` workflows and `.github/workflows-readme.md` |
| Change Azure DevOps deployment | `.azdo/pipelines/` | `stages/`, `jobs/`, `steps/`, and `vars/` templates |
| Add or repair automated browser coverage | `playwright/` | root Playwright config variants and `playwright/fixtures/` |
| Change AI behavior | `src/web/Website/Helpers/`, services, and analyzer code | `applicationSettings.json`, AI package references, related tests |
| Change Copilot behavior | `.github/copilot-instructions.md`, `.github/instructions/`, `.github/skills/`, `.github/agents/`, `.github/prompts/` | this file, plus any referenced external skills workspace |

## 3. Architecture and Data Flow

### Web application

`src/web/Website/` is an ASP.NET Core / Blazor Server application targeting `net10.0`. `Program.cs` composes configuration and dependency injection, selects the joke data source, registers authentication and UI services, maps controllers, and maps the Blazor hub and fallback host page.

The normal request path is:

1. Browser connects to the Blazor Server application.
2. Blazor pages and components call application services or `IJokeRepository`.
3. REST controllers in `src/web/Website/API/` expose HTTP API operations over the same application.
4. The repository implementation reads or writes either JSON or SQL data.
5. Optional AI helpers call Azure OpenAI / Microsoft Agent or Copilot SDK integrations and can use Blob Storage for generated images.
6. Optional OpenTelemetry/Azure Monitor configuration emits application telemetry.

The web app supports anonymous access by default. Entra ID authentication is configured only when `AzureAD:TenantId` is present; individual pages/endpoints can then apply authorization. API key handling is implemented in the web API support code and must be preserved when changing controllers.

### Data layer

`src/web/Data/` is a shared .NET library containing domain models, `DadABaseDbContext`, repository abstractions, and repository implementations.

`IJokeRepository` is the principal boundary for joke operations. It includes reads (all, recent, one, random, categories, search), writes (add, update, delete, ratings/category updates), import/export operations, and image-description updates.

There are two application data modes:

- `DataSource=JSON`: `JokeJsonRepository` reads the deployed `Data/Jokes.json` file. This is the default template mode and is convenient for local demos without SQL.
- `DataSource=SQL`, `SQLDB`, or `DATABASE` in the web startup code: `JokeSQLRepository` uses EF Core SQL Server through `DadABaseDbContext` and the configured connection string.

If SQL is selected without a usable `DefaultConnection`, the web application deliberately falls back to JSON. An unknown data source also falls back to JSON. Do not assume that local development is database-backed.

The SQL domain is represented under the `Dad` schema and includes jokes, categories, joke/category associations, and ratings. The SQL database project is the schema authority for deployment; do not treat EF migrations or JSON seed data as a substitute for DACPAC changes.

### Other hosts and tools

- `src/function/Function/`: .NET 10 Azure Functions isolated worker. HTTP-triggered API and health-check behavior live here. Its startup configures worker middleware, Application Insights, the function repository, and Swagger.
- `src/function/DataLayer/` and `src/function/Entities/`: function-specific data access and DTO/entity projects; these are separate from the web shared data library.
- `src/console/`: .NET console application using `Spectre.Console` for terminal interaction and a `DadJokeService` for joke API access.
- `src/analyzer/`: .NET console/batch analyzer for processing jokes with AI and updating data. `Program.cs`, `RecordProcessor.cs`, and `JokeDbContext.cs` are the main entry points.
- `src/mcp/`: MCP shared models/tools plus Stdio and SSE server hosts. Use the local MCP readme and project files for current readiness and transport details.

## 4. Repository Layout

```text
src/
  web/
    Website/       Blazor Server web host, pages, components, APIs, services, static/data files
    Data/          Shared joke domain, EF Core context, repository interface and implementations
    Tests/         Web/data xUnit tests and coverage.runsettings
    dadabase.net10.web.sln
  function/
    Function/      Azure Functions isolated worker host and HTTP triggers
    DataLayer/     Function data access
    Entities/      Function entities/DTOs
    Tests/         Function xUnit tests and test JSON
    TestHarness/   HTTP request assets where present
    DadABase.Net10.Function.sln
  mcp/             Shared, Stdio, and SSE MCP projects; DadJokeMCP.sln
  console/         CLI host; DadJoke.console.sln
  analyzer/        AI/batch analyzer; DadJokeAnalyzer.sln
  sql.database/    SQL Server Database Project/DACPAC source; sql.database.sln
  Directory.Build.props

infra/
  Bicep/           Main Bicep composition, parameter file, resource modules, lookup data, scripts
  azd-main.bicep   Azure Developer CLI entry point
  azd-main.parameters.json

playwright/        TypeScript Playwright smoke, basic, UI, and API suites; fixtures and page objects
.github/
  copilot-instructions.md  Repository-wide Copilot rules
  instructions/             Focused authoring rules
  skills/                   Repository skills, including Dadabase Playwright guidance
  agents/                   Custom agent modes
  prompts/                  Task prompts
  workflows/                GitHub Actions entry workflows and reusable templates
  actions/, config/, hooks/, scripts/  Workflow support assets
.azdo/pipelines/             Azure DevOps entry pipelines and reusable stages/jobs/steps/vars
Docs/                         Product, architecture, SQL, deployment, SBOM, and feature documentation
TestHarness/                  HTTP test files
azure.yaml                    Azure Developer CLI configuration
package.json                  Node development dependencies and Husky setup
PRODUCT.md, DESIGN.md         Product constraints and visual/design guidance
CONTRIBUTING.md               Local setup, hooks, test and contribution guidance
```

Build output such as `bin/`, `obj/`, publish folders, and test result folders may exist under source projects. They are generated artifacts, not source locations.

## 5. Languages and Frameworks

| Area | Language / framework | Evidence and notes |
| --- | --- | --- |
| Web | C#, ASP.NET Core, Blazor Server, .NET 10 | `src/web/Website/DadABase.Web.csproj`, `Program.cs` |
| UI | Razor/HTML/CSS, MudBlazor, Blazored LocalStorage, SweetAlert2 integration | Web project package references and `.razor`/`.razor.css` files |
| Shared data | C#, EF Core 10, SQL Server provider, Newtonsoft.Json | `src/web/Data/DadABase.Data.csproj` |
| Function API | C#, Azure Functions isolated worker, .NET 10 | `src/function/Function/DadABase.Function.csproj` |
| Console/analyzer | C#, .NET 10; console uses Spectre.Console; analyzer uses its AI/data packages | `src/console/`, `src/analyzer/` project files |
| MCP | C#, .NET 10, Model Context Protocol SDK projects | `src/mcp/` project files |
| Database | T-SQL / SQL Server Database Project / DACPAC | `src/sql.database/` |
| Infrastructure | Azure Bicep | `infra/Bicep/` |
| Browser tests | TypeScript, Playwright Test | `package.json`, `playwright.config*.ts`, `playwright/` |
| Automation | YAML, PowerShell, shell/JavaScript support scripts | `.github/workflows/`, `.azdo/pipelines/`, `infra/Bicep/scripts/` |

Notable web packages include MudBlazor, Azure Identity, Azure OpenAI, Microsoft Identity Web, Microsoft Agent/OpenAI packages, Blob Storage, OpenTelemetry/Azure Monitor, EF Core SQL Server, Swashbuckle, AutoMapper, and the GitHub Copilot SDK. Always read the current project file for exact versions; older architecture exports list historical versions.

## 6. Configuration and Secrets

### Web configuration

`src/web/Website/applicationSettings.json` is a checked-in template/configuration file copied to output. It contains `AppSettings` including `DataSource`, `DefaultConnection`, `ApiKey`, `AdminUserList`, `EnableSwagger`, AI settings, and Blob Storage settings, plus `AzureAD`, `APPLICATIONINSIGHTS_CONNECTION_STRING`, and `VisualStudioTenantId` settings.

Web startup loads, in this general order:

1. The deployed `applicationSettings.json` path.
2. Environment variables.
3. User Secrets in local development.
4. Azure Key Vault when `KeyVaultName` is set.

Use environment variables, User Secrets, or Key Vault for real credentials. Do not put connection strings, API keys, tenant credentials, or passwords in committed configuration. `DefaultAzureCredential` is the managed identity/default credential boundary. `VisualStudioTenantId` is a local-development override and must not be used as an Azure deployment substitute.

The function host uses `appsettings.json` if present, environment variables, and User Secrets. Its startup is in `src/function/Function/Program.cs`.

### Infrastructure and deployment configuration

- `infra/Bicep/main.bicep` is the main resource composition. It supports `webapp`, `containerapp`, `functionapp`, and `all` deployment types, optional website-only deployments, JSON or SQL app data source, existing-resource reuse, managed identity choices, Key Vault, storage, Application Insights/Log Analytics, Azure SQL, and AI parameters.
- `infra/Bicep/main.bicepparam` is the shared parameter file used by Azure DevOps workflows; pipeline substitution is expected for environment-specific values.
- `azure.yaml` declares Bicep as the azd infrastructure provider and the web service at `src/web/Website/` hosted on App Service.
- `.azdo/pipelines/vars/` contains common, environment, source-location, and service-connection variable templates.
- GitHub deployment values are documented in `.github/workflows-readme.md` and `.github/CreateGitHubSecrets.md`. Prefer OIDC/federated credentials and environment-scoped secrets.

## 7. Testing

### C# unit and component-facing tests

- Web/data tests: `src/web/Tests/` in `DadABase.Tests.csproj`.
- Function tests: `src/function/Tests/` in `DadABase.Function.Tests.csproj`.
- Framework: xUnit v3, xunit runner, Microsoft.NET.Test.Sdk, Moq, EF Core InMemory/TestHost where needed.
- Shared setup/data: `BaseTest.cs`, `BaseWebTest.cs`, and `SampleData/` under web tests; function test data includes `Jokes.json`.
- Coverage: both test projects point to a local `coverage.runsettings`; the web project includes coverage exclusions.
- Test naming is grouped by repository, model, API, and export behavior. Preserve existing fixtures and mock boundaries when adding tests.

Useful commands:

```text
dotnet test src/web/Tests/DadABase.Tests.csproj
dotnet test src/function/Tests/DadABase.Function.Tests.csproj
dotnet test src/web/dadabase.net10.web.sln
dotnet test src/function/DadABase.Net10.Function.sln
```

`CONTRIBUTING.md` also documents `dotnet test` from `src/web` and full Playwright setup. The exact solution/project command is preferable when narrowing a change.

### Playwright end-to-end tests

- `playwright/smoke-tests/`: fast homepage, search, navigation, theme, and API checks.
- `playwright/basic-tests/`: basic UI flows.
- `playwright/ui-tests/`: broader UI scenarios.
- `playwright/api-tests/`: API-focused browser tests.
- `playwright/page-objects/`: reusable Home, Search, About, Layout models.
- `playwright/fixtures/test-fixtures.ts`: typed fixture that injects those page objects.
- Config variants at the root include `playwright.config.ts`, `playwright.config.local.ts`, `playwright.config.cicd.ts`, `playwright.config.test-service.ts`, and `playwright.config.workspace.ts`.

The local config currently targets the deployed Dadabase URL by default; inspect the selected config before assuming a local server is used. `package.json` currently declares Playwright dependencies but does not define named `test:*` scripts, so use `npx playwright test` with the appropriate config or the repository's pipeline commands. Install browsers with `npx playwright install` when needed.

`dadabase-playwright-testing` under `.github/skills/` contains project-specific testing guidance and should be consulted for anonymous homepage, category, and search smoke work.

## 8. GitHub Actions

The numbered workflows are the user-facing entry points; `template-*.yml` files are reusable workflow building blocks. The current entry workflows are:

- `1-deploy-bicep.yml`: infrastructure deployment.
- `2.1-bicep-build-deploy-webapp.yml`: Bicep plus web app build/deployment.
- `2.2-bicep-build-deploy-containerapp.yml`: Bicep plus container app build/deployment.
- `3-bicep-build-deploy-function.yml`: Bicep plus function build/deployment.
- `4-build-deploy-dacpac.yml`: SQL DACPAC build/deployment.
- `5-run-sql-script.yml`: run a SQL patch/script.
- `6-pr-scan-build.yml`: pull-request scan/build.
- `7-scan-code.yml`: code/security scan.
- `8-smoke-test-webapp.yml`: deployed web app smoke tests.
- `azure-dev.yml`: Azure Developer CLI workflow.

Reusable templates cover configuration loading, Bicep, web app, container app, function, DACPAC, SQL, SBOM, scan, and smoke-test work. Read the entry workflow and the called template together before changing inputs/outputs, permissions, environments, or secrets.

The workflow authoring contract is in `.github/instructions/github-actions-instructions.md`. Security-sensitive changes should also review `.github/CreateGitHubSecrets.md` and the scanning workflows. Do not casually replace OIDC with long-lived service principal credentials.

## 9. Azure DevOps Pipelines

Azure DevOps mirrors much of the GitHub deployment capability but uses composed YAML under `.azdo/pipelines/`:

- Entry pipelines include `1-deploy-bicep.yml`, `2.1-bicep-build-deploy-webapp.yml`, `2.2-bicep-build-deploy-containerapp.yml`, `3-bicep-build-deploy-function.yml`, `4-build-deploy-dacpac.yml`, `5-run-sql-script.yml`, `6-pr-scan-build.yml`, `6.1-pr-review-scan-build.yml`, `7-scan-code.yml`, `7.1-scan-github-repo.yml`, `8-smoke-test-webapp.yml`, `9-dependabot.yml`, `10-deploy-webapp-only-pipeline.yml`, and `11-auto-test-pipeline.yml`.
- `stages/` composes promotion and environment flow.
- `jobs/` contains reusable build, deploy, scan, Playwright, DACPAC, SQL, function, container, and SBOM jobs.
- `steps/` contains the lower-level deployment, scanning, SQL, GitHub dispatch, and DACPAC steps.
- `vars/` contains shared and environment-specific settings (`dev`, `qa`, `prod`, common, service connections, and source location).
- `.azdo/pipelines/readme.md` documents the shared Bicep parameter process, deployment types, environments, variable group `Dadabase.Demo`, and setup requirements.

The Azure DevOps YAML authoring contracts are `.github/instructions/azure-devops-pipeline-instructions.md` and `.github/instructions/bicep-instructions.md`.

## 10. SQL and Data Operations

The SQL database project is `src/sql.database/sql.database.sqlproj`. Schema objects are under `src/sql.database/Dad/`, including tables, views, schemas, and deployment scripts. `Patch/` holds manual or operational scripts. Related explanatory material is in `Docs/sql/`.

For schema changes:

1. Update the SQL project object/script in the correct schema folder.
2. Check pre/post deployment behavior and seed/patch scripts.
3. Build and validate the DACPAC using the repository's SQL tooling/pipeline.
4. Update `MAP.md` if object ownership or deployment flow changes.

Use `.github/instructions/sql-database-dacpac-instructions.md` before editing SQL or DACPAC workflows. Avoid putting application-only persistence logic in the database project without documenting the boundary.

## 11. Copilot Customization Surface

### Repository instructions

`.github/copilot-instructions.md` is the repository-wide baseline. Focused instructions under `.github/instructions/` cover Blazor/CSS, C# style, .NET project structure, Bicep, GitHub Actions, Azure DevOps, SQL/DACPAC, testing, general best practices, code review, debugging, and other specialized tasks. Read the narrowest applicable instruction file before editing.

### Agents

`.github/agents/` contains project-local agent definitions such as C#/.NET, Bicep planning/implementation, Azure architecture, database, DevOps, security, Playwright, debugging, planning, squad, and Impeccable design agents. The directory is the source of truth for the exact available filenames; use file search rather than assuming an agent exists.

### Skills

`.github/skills/` contains reusable skills. `dadabase-playwright-testing` is the project-specific skill. Other local skills cover .NET/C#, EF Core, SQL, Bicep/Aspire/Azure DevOps, GitHub, testing, document generation, frontend/design, and skill creation. Some shared skills may also be supplied by the `my.copilot.skills` workspace referenced by `README.md` and `dadabase.demo.gh.code-workspace`; distinguish local repository skills from external workspace skills.

### Prompts and commands

`.github/prompts/` contains task prompts for migration planning/assessment, Azure deployment, CI/CD, Playwright, quality testing, refactoring, translation, and status. There is currently no `.github/commands/` directory in this repository; do not document a command as available unless it is added and verified. VS Code tasks are the main local command surface for web build/publish/watch.

## 12. Local Commands and Development Workflow

Prerequisites are .NET 10 SDK, Node.js 18+, Git, and optionally gitleaks for secret scanning. `npm install` installs JavaScript dependencies and runs the Husky `prepare` hook.

The workspace tasks target the web project:

```text
dotnet build src/web/Website/DadABase.Web.csproj
dotnet publish src/web/Website/DadABase.Web.csproj
dotnet watch run --project src/web/Website/DadABase.Web.csproj
```

Run the web host from `src/web/Website/` with `dotnet run`. For a local JSON-mode demo, no SQL server is required; use User Secrets or environment variables for AI/auth/database settings.

Husky pre-commit behavior is documented in `CONTRIBUTING.md`: gitleaks scans staged changes when installed, and staged C# files are formatted with `dotnet format` across the solution files. Never commit secrets or generated build/test output.

## 13. Branching and Change Policy

The primary branch is `main`. Do not commit or push unless the user explicitly asks. Never commit directly to `main` or `master`. Before a requested commit, check `git branch --show-current`; create a feature branch named `feature/short-description`, `fix/short-description`, or `chore/short-description` when necessary. Changes should be committed to that branch and proposed to `main` through a pull request.

For every source, infrastructure, workflow, test, or Copilot-customization change:

1. Update the relevant focused documentation if behavior or workflow changed.
2. Update this `MAP.md` when locations, ownership, commands, configuration, or architecture changed.
3. Prompt the user that `MAP.md` was updated or needs review before finishing.
4. Run the narrowest relevant build/test/lint validation available.

## 14. Known Caveats and Drift Signals

- `MAP.md` must stay more current than generated architecture exports. `Docs/Application-Architecture.md` contains useful diagrams but has historical claims, including older package versions and an MSTest description that does not match the current xUnit project files.
- The current web build task path in the workspace context is the valid path: `src/web/Website/DadABase.Web.csproj`. Older notes may mention a nonexistent `src/DadABase.Web/` path; verify task definitions before relying on them.
- `src/web/Website/applicationSettings.json` is a template and contains placeholder values. Treat all deployment secrets as external configuration.
- The default Playwright config targets a deployed site and the root `package.json` has no named Playwright test scripts. Inspect config selection and invoke Playwright directly.
- Data source selection changes behavior substantially. Tests and local runs may use JSON or in-memory data while production uses Azure SQL.
- MCP and AI features can evolve faster than the core joke CRUD path. Read their local project readmes and package references before assuming production readiness or provider behavior.

## 15. High-Value References

- [README.md](README.md): product overview, feature list, local setup, deployment options.
- [PRODUCT.md](PRODUCT.md): product goals and constraints.
- [DESIGN.md](DESIGN.md): visual/design system guidance.
- [CONTRIBUTING.md](CONTRIBUTING.md): prerequisites, hooks, test commands, contribution workflow.
- [Docs/Application-Architecture.md](Docs/Application-Architecture.md): diagrams and historical architecture context.
- [Docs/sql/README.md](Docs/sql/README.md): SQL documentation index.
- [.github/workflows-readme.md](.github/workflows-readme.md): GitHub Actions setup and secrets.
- [.azdo/pipelines/readme.md](.azdo/pipelines/readme.md): Azure DevOps pipeline setup.
- [.github/copilot-instructions.md](.github/copilot-instructions.md): repository-wide Copilot policy.
