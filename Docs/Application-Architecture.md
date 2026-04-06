# Dad-A-Base Architecture Document

> **Version:** 1.1 · Generated: 2026-04-06 · Repository: [lluppesms/dadabase.demo](https://github.com/lluppesms/dadabase.demo)

---

## 1. Executive Summary

**Dad-A-Base** is a full-stack demonstration repository that showcases modern .NET 10 development practices, cloud-native architecture on Azure, and end-to-end DevOps automation — all delivered through a dad joke management application.

The repository serves as a reference implementation for:
- .NET 10 Blazor Server web applications with MudBlazor UI
- Azure-hosted serverless APIs via Azure Functions (Flex Consumption)
- Model Context Protocol (MCP) servers for AI agent integration
- Dual data-source architecture (JSON fallback + Azure SQL)
- Full Infrastructure-as-Code using Azure Bicep
- Multi-platform CI/CD (GitHub Actions + Azure DevOps)
- AI-powered features using Azure OpenAI and Microsoft Agent Framework
- Playwright end-to-end testing and MSTest unit testing

---

## 2. Repository Structure

```
dadabase.demo/
├── src/                         Application source code
│   ├── web/                     Blazor Web App + Data layer
│   │   ├── Website/             DadABase.Web (.NET 10 Blazor Server)
│   │   ├── Data/                DadABase.Data (shared data library)
│   │   └── Tests/               Unit tests (MSTest)
│   ├── function/                Azure Function (serverless API)
│   │   ├── Function/            DadABase.Function (.NET 10 Isolated Worker)
│   │   ├── DataLayer/           Function-specific data layer
│   │   ├── Entities/            Function-specific entity models
│   │   └── Tests/               Function unit tests
│   ├── mcp/                     MCP Servers
│   │   ├── DadJokeMCP/          Stdio-based MCP server
│   │   └── DadJokeMCPSSE/       SSE-transport MCP server
│   ├── console/                 Console application
│   ├── analyzer/                Data analysis tool
│   └── sql.database/            SQL Database project (DACPAC)
├── infra/
│   ├── Bicep/                   Bicep IaC templates
│   └── azd-main.bicep           Azure Developer CLI entry point
├── .github/
│   ├── workflows/               GitHub Actions CI/CD workflows
│   └── instructions/            Copilot instructions and skills
├── .azuredevops/
│   └── policies/                Azure DevOps policy configuration
├── playwright/                  Playwright E2E test suites
├── Docs/                        Project documentation
│   ├── Application-Architecture.md  Architecture documentation
│   ├── Application-Architecture.pdf  Architecture document (PDF export)
│   ├── Application-Architecture.pptx Architecture presentation deck
│   └── Export/                  Generated export artifacts
├── TestHarness/                 HTTP test files
└── azure.yaml                   Azure Developer CLI configuration
```

---

## 3. Application Architecture

### 3.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Client Browser                                │
└────────────────────────────┬────────────────────────────────────────────┘
                             │ HTTPS
┌────────────────────────────▼────────────────────────────────────────────┐
│                  DadABase.Web  (Blazor Server / .NET 10)                │
│                                                                         │
│  ┌────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  Blazor    │  │  REST API    │  │  AI Helper   │  │  Export      │  │
│  │  Pages     │  │  Controllers │  │  (GenAI)     │  │  Service     │  │
│  └──────┬─────┘  └──────┬───────┘  └──────┬───────┘  └──────────────┘  │
│         │               │                  │                             │
│  ┌──────▼───────────────▼──────────────────▼───────────────────────┐   │
│  │              DadABase.Data  (Shared Data Library)               │   │
│  │   IJokeRepository ─┬─ JokeSQLRepository (EF Core + Azure SQL)  │   │
│  │                    └─ JokeJsonRepository (flat file fallback)   │   │
│  └──────────────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────────────┘
                             │
         ┌───────────────────┼──────────────────────────┐
         │                   │                           │
┌────────▼────────┐  ┌───────▼──────────┐  ┌───────────▼────────────┐
│   Azure SQL DB  │  │  Azure OpenAI    │  │  Azure Blob Storage     │
│   (joke data)   │  │  GPT-4o / DALL-E │  │  (AI-generated images)  │
└─────────────────┘  └──────────────────┘  └────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                  DadABase.Function  (Azure Functions v4)                │
│                  .NET 10 Isolated Worker / Flex Consumption Plan        │
│   - HTTP Trigger (joke API)                                             │
│   - Health Check Trigger                                                │
│   - OpenAPI / Swagger support                                           │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                          MCP Servers                                    │
│   DadJokeMCP (stdio)  │  DadJokeMCPSSE (SSE)                           │
│   Tools: GetDadJoke, GetDadJokesByCategory, GetDadJokeCategories       │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Data Source Strategy

The application supports two data sources, switchable via the `DataSource` configuration key:

| Mode | Value | Description |
|------|-------|-------------|
| JSON | `JSON` | Flat-file fallback — no database required, suitable for demos and local dev |
| SQL  | `SQL` / `SQLDB` / `DATABASE` | Azure SQL Database via Entity Framework Core |

This is implemented via the repository pattern (`IJokeRepository`) with two concrete implementations:
- `JokeJsonRepository` — reads from `Data/Jokes.json`
- `JokeSQLRepository` — uses EF Core with `DadABaseDbContext`

---

## 4. Source Code Projects

### 4.1 DadABase.Web

| Property | Value |
|----------|-------|
| Path | `src/web/Website/` |
| Framework | .NET 10 (`net10.0`) |
| Project type | `Microsoft.NET.Sdk.Web` (Blazor Server) |
| UI library | MudBlazor 9.2.0 |
| Solution | `dadabase.net10.web.sln` |

**Key NuGet packages:**

| Package | Version | Purpose |
|---------|---------|---------|
| MudBlazor | 9.2.0 | Component library (UI) |
| Azure.AI.OpenAI | 2.8.0-beta.1 | Azure OpenAI SDK |
| Microsoft.Agents.AI.OpenAI | 1.0.0-preview | Microsoft Agent Framework |
| Azure.Identity | 1.20.0 | Managed Identity / DefaultAzureCredential |
| Azure.Monitor.OpenTelemetry.AspNetCore | 1.4.0 | App Insights / OpenTelemetry |
| Azure.Storage.Blobs | 12.27.0 | Blob storage for images |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.5 | EF Core SQL provider |
| Microsoft.Identity.Web | 4.7.0 | Entra ID / OIDC authentication |
| Swashbuckle.AspNetCore | 10.1.7 | Swagger/OpenAPI |
| Blazored.LocalStorage | 4.5.0 | Browser local storage |
| AutoMapper | 16.1.1 | Object mapping |
| Azure.Extensions.AspNetCore.Configuration.Secrets | 1.5.0 | Azure Key Vault configuration provider |
| Azure.Identity.Broker | 1.5.0 | Windows auth broker for local development |
| CurrieTechnologies.Razor.SweetAlert2 | 5.6.0 | SweetAlert2 JS dialogs in Blazor |
| Microsoft.ApplicationInsights | 3.1.0 | Application Insights core telemetry |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.5 | JWT bearer authentication |
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 10.0.5 | OpenID Connect authentication |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.5 | ASP.NET Core Identity EF Core integration |
| Microsoft.Extensions.Configuration.AzureAppConfiguration | 8.5.0 | Azure App Configuration provider |
| Microsoft.Extensions.Logging.ApplicationInsights | 2.23.0 | Application Insights logging provider |
| Microsoft.Identity.Client | 4.83.3 | Microsoft Authentication Library (MSAL) |
| Microsoft.Identity.Web.UI | 4.7.0 | Identity Web UI components |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.23.0 | Docker/container Visual Studio tooling |
| Microsoft.EntityFrameworkCore.Tools | 10.0.5 | EF Core CLI tooling and migrations |
| Newtonsoft.Json | 13.0.4 | JSON serialization |
| Swashbuckle.AspNetCore.Filters | 10.0.1 | Swagger/OpenAPI operation filters |

**Pages:**

| Page | Description |
|------|-------------|
| `/` (Index) | Home page — displays a random joke with AI image generation |
| `/Random` | Random joke viewer |
| `/Search` | Joke search by text or category |
| `/JokeDetail/{id}` | Single joke detail view |
| `/JokeEditor` | Create / edit jokes with AI-powered category assignment |
| `/Export` | Export joke data |
| `/Admin` | Administration page |
| `/About` | About page |

**API Controllers:**

| Controller | Route | Description |
|-----------|-------|-------------|
| `JokeController` | `GET/POST/PUT/DELETE api/joke` | Full CRUD for jokes |
| `CategoryController` | `api/category` | Joke category management |
| `ConfigController` | `api/config` | Application configuration |
| `BaseAPIController` | (base class) | API key authentication, AutoMapper setup |

**Authentication:**
- Microsoft Entra ID via OpenID Connect (`Microsoft.Identity.Web`)
- API Key header authentication (`[ApiKey]` attribute via `AuthorizeApiKey.cs`)
- Anonymous access supported alongside authenticated access

**Shared components:**
- `JokeDisplayComponent` — reusable joke card
- `MessageBubbleComponent` — dialog/chat bubble

### 4.2 DadABase.Data

| Property | Value |
|----------|-------|
| Path | `src/web/Data/` |
| Framework | .NET 10 |
| Referenced by | DadABase.Web |

**Domain models:**

| Model | Table | Description |
|-------|-------|-------------|
| `Joke` | `dbo.Joke` | Core joke entity (JokeId, JokeTxt, JokeCategoryId, Attribution, SortOrderNbr, Rating, ActiveInd, audit fields) |
| `JokeCategory` | `dbo.JokeCategory` | Category definitions |
| `JokeJokeCategory` | join table | Many-to-many joke ↔ category |
| `JokeRating` | `dbo.JokeRating` | User ratings for jokes |
| `JsonJoke` | n/a | Deserialization model for JSON data source |

**Repository interface:** `IJokeRepository`

```
ListAll()          - all active jokes
GetRecentAdditions()  - most recently added jokes
GetOne(id)         - single joke by ID
GetRandomJoke()    - random joke selection
GetJokeCategories() - category name list
FindBySearchText() - text + category search
Save() / Delete()  - write operations
```

### 4.3 DadABase.Function

| Property | Value |
|----------|-------|
| Path | `src/function/Function/` |
| Framework | .NET 10 (`net10.0`) |
| Runtime | Azure Functions v4, Isolated Worker |
| Plan | Flex Consumption |
| Solution | `dadabase.net10.function.sln` |

**HTTP triggers:**

| Trigger | File | Description |
|---------|------|-------------|
| `TriggerHttp` | `TriggerHttp.cs` | Primary joke API endpoint |
| `TriggerHealthCheck` | `TriggerHealthCheck.cs` | Health check endpoint |

**Supporting projects:**
- `DadABase.DataLayer` — EF Core data access
- `DadABase.Entities` — Entity definitions

### 4.4 MCP Servers

| Project | Path | Transport | Description |
|---------|------|-----------|-------------|
| `DadJokeMCP` | `src/mcp/DadJokeMCP/` | stdio | Standard MCP server |
| `DadJokeMCPSSE` | `src/mcp/DadJokeMCPSSE/` | SSE (HTTP) | Server-Sent Events transport |

**Exposed MCP tools:**

| Tool | Description |
|------|-------------|
| `GetDadJoke` | Returns a random dad joke |
| `GetDadJokesByCategory` | Returns all jokes in a named category |
| `GetDadJokeCategories` | Returns the list of available categories |

### 4.5 Console Application

| Property | Value |
|----------|-------|
| Path | `src/console/` |
| Framework | .NET 10 |
| Purpose | CLI-based joke retrieval for terminal users |

### 4.6 DadJoke Analyzer

| Property | Value |
|----------|-------|
| Path | `src/analyzer/` |
| Purpose | Offline data analysis and record processing tool |
| Key class | `RecordProcessor` |

### 4.7 SQL Database Project

| Property | Value |
|----------|-------|
| Path | `src/sql.database/` |
| Type | SQL Server Database Project (`.sqlproj`) |
| Output | DACPAC artifact |
| Schema | `dbo/` — tables, views, stored procedures |

**Tables defined:**
- `Joke` — core joke storage
- `JokeCategory` — categories
- `JokeJokeCategory` — many-to-many associations
- `JokeRating` — user ratings

**Views:** `CreateJokeView.sql`

---

## 5. AI/GenAI Integration

The web application uses **Azure OpenAI** and **Microsoft Agent Framework** for three AI-powered capabilities:

```
┌──────────────────────────────────────────────────────────┐
│                        AIHelper                          │
│    (Microsoft.Agents.AI + Azure.AI.OpenAI)               │
│                                                          │
│  ┌──────────────────────┐  ┌──────────────────────────┐  │
│  │ jokeCategoryAgent    │  │ jokeDescriptionAgent     │  │
│  │ Model: GPT-4o        │  │ Model: GPT-4o            │  │
│  │ Purpose: Auto-assign │  │ Purpose: Describe joke   │  │
│  │ categories to jokes  │  │ for image prompt         │  │
│  └──────────────────────┘  └──────────────────────────┘  │
│  ┌──────────────────────┐  ┌──────────────────────────┐  │
│  │ jokeAnalyzerAgent    │  │ imageGenerator           │  │
│  │ Model: GPT-4o        │  │ Model: DALL-E 3          │  │
│  │ Purpose: Analyze     │  │ Purpose: Generate image  │  │
│  │ joke content         │  │ for joke; stored in Blob │  │
│  └──────────────────────┘  └──────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

| Capability | Model | Description |
|-----------|-------|-------------|
| Category classification | GPT-4o | Auto-assigns up to 2 categories to a new joke |
| Joke description | GPT-4o | Generates a scene description for DALL-E image prompting |
| Joke analysis | GPT-4o | General joke content analysis |
| Image generation | DALL-E 3 | Creates an AI illustration per joke; saved to Azure Blob Storage |

**Configuration keys:**
- `AzureOpenAIChatEndpoint`, `AzureOpenAIChatDeploymentName`, `AzureOpenAIChatApiKey`
- `AzureOpenAIImageEndpoint`, `AzureOpenAIImageDeploymentName`, `AzureOpenAIImageApiKey`

---

## 6. Infrastructure as Code (Bicep)

All Azure resources are defined in `infra/Bicep/`.

### 6.1 Deployment Types

The `deploymentType` parameter controls which resources are provisioned:

| Value | Resources Deployed |
|-------|-------------------|
| `webapp` | App Service, SQL DB, Key Vault, App Insights, Storage, IAM |
| `containerapp` | Container Apps, Container Registry, SQL DB, Key Vault, App Insights, IAM |
| `functionapp` | Azure Functions, SQL DB, Key Vault, App Insights, Storage, IAM |
| `all` | All of the above combined |

### 6.2 Module Map

```
infra/Bicep/
├── main.bicep                   Entry point — orchestrates all modules
├── main.bicepparam              Default parameter values
├── resourcenames.bicep          Centralised resource naming convention
├── modules/
│   ├── webapp/                  Azure App Service plan + web app
│   ├── container/               Azure Container Apps + Container Registry
│   ├── function/                Azure Functions (Flex Consumption)
│   ├── functions/               Additional function app configuration
│   ├── database/                Azure SQL Server + database
│   ├── monitor/                 Application Insights + Log Analytics
│   ├── security/                Azure Key Vault
│   ├── storage/                 Azure Storage Accounts
│   └── iam/                     Managed Identity + RBAC role assignments
└── data/
    ├── resourceAbbreviations.json   Resource type abbreviation lookup
    └── roleDefinitions.json         RBAC role definition IDs
```

### 6.3 Key Infrastructure Resources

| Resource | Bicep Module | Notes |
|----------|-------------|-------|
| Azure App Service | `modules/webapp/` | Linux or Windows, configurable SKU (default: B1) |
| Azure Container Apps | `modules/container/` | With Container Registry (Basic/Standard/Premium) |
| Azure Functions | `modules/function/` | Flex Consumption plan, .NET 10 Isolated |
| Azure SQL Database | `modules/database/` | Default: GeneralPurpose GP_S_Gen5; configurable |
| Azure Key Vault | `modules/security/` | Secrets management; supports admin user access |
| Application Insights | `modules/monitor/` | OpenTelemetry exporter connected |
| Azure Storage | `modules/storage/` | Used by Function + web app (images) |
| Managed Identity | `modules/iam/` | User-assigned + system-assigned; role assignments |

### 6.4 Security Design

- **No passwords in config** — Managed Identity (`DefaultAzureCredential`) used for all Azure service connections
- **Key Vault** centralises all secrets; app reads from Key Vault at startup
- **`websiteOnly` flag** — optionally skips SQL and Function resources for stripped-down deployments
- **RBAC** — role assignments provisioned by Bicep (`addRoleAssignments` parameter)
- **`pipelineServicePrincipalObjectId`** — grants `AcrPush` to the DevOps service principal on Container Registry

### 6.5 Common Resource Tags

Every resource is tagged with:

```json
{
  "LastDeployed": "<utcNow>",
  "Application": "<appName>",
  "Environment": "<environmentCode>"
}
```

---

## 7. CI/CD Pipelines

### 7.1 GitHub Actions Workflows

| Workflow | File | Purpose |
|--------- |------|---------|
| Deploy Infrastructure | `1-deploy-bicep.yml` | Provisions all Azure resources via Bicep |
| Build & Deploy Web (App Service) | `2.1-bicep-build-deploy-webapp.yml` | Build, test, and deploy Blazor app to App Service |
| Build & Deploy Web (Container App) | `2.2-bicep-build-deploy-containerapp.yml` | Build Docker image, push to ACR, deploy to Container Apps |
| Build & Deploy Function | `3-bicep-build-deploy-function.yml` | Build and deploy Azure Function |
| Deploy DACPAC | `4-build-deploy-dacpac.yml` | Build SQL project and deploy DACPAC to Azure SQL |
| Run SQL Script | `5-run-sql-script.yml` | Execute ad-hoc SQL scripts |
| PR Scan + Build | `6-pr-scan-build.yml` | Pull request validation gate |
| Code Scanning | `7-scan-code.yml` | Security scanning (CodeQL / GHAS) |
| Smoke Tests | `8-smoke-test-webapp.yml` | Post-deployment Playwright validation |
| AZD Deploy | `azure-dev.yml` | Azure Developer CLI full-stack deploy |

**Reusable workflow templates:**

| Template | Purpose |
|----------|---------|
| `template-bicep-deploy.yml` | Shared Bicep deploy step |
| `template-webapp-build.yml` | Shared .NET build + test step |
| `template-webapp-deploy.yml` | Shared App Service deploy step |
| `template-containerapp-build.yml` | Docker build + ACR push |
| `template-containerapp-deploy.yml` | Container Apps deploy |
| `template-function-build.yml` | Function build step |
| `template-function-deploy.yml` | Function deploy step |
| `template-dacpac-build.yml` | SQL project build |
| `template-dacpac-deploy.yml` | DACPAC deploy step |
| `template-scan-code.yml` | Code scanning step |
| `template-smoke-test.yml` | Playwright smoke tests |
| `template-load-config.yml` | Environment configuration loader |
| `template-run-sql.yml` | SQL script execution |

### 7.2 Azure DevOps Pipelines

Dedicated Azure DevOps pipeline YAML files are not included in this repository. The `.azuredevops/` folder contains only repository policy configuration:

| Path | Purpose |
|------|---------|
| `.azuredevops/policies/copilot-preferences.yml` | Repository-level GitHub Copilot policy settings |

To use Azure DevOps CI/CD with this project, the GitHub Actions workflow templates in `.github/workflows/` can be adapted and imported manually into an Azure DevOps project. The `Docs/YML_AzDO.md` guide describes this process.

### 7.3 Deployment Methods Summary

| Method | Entry Point | Difficulty |
|--------|------------|------------|
| GitHub Actions | `.github/workflows/` | ⭐⭐⭐ |
| Azure DevOps | Adapt `.github/workflows/` templates (see `Docs/YML_AzDO.md`) | ⭐⭐⭐⭐ |
| Azure Developer CLI | `azure.yaml` + `azd up` | ⭐⭐ |

---

## 8. Testing

### 8.1 Unit Tests

| Project | Location | Framework | Notes |
|---------|----------|-----------|-------|
| Web unit tests | `src/web/Tests/` | MSTest + Coverlet | API controllers, data layer |
| Function unit tests | `src/function/Tests/` | MSTest | Trigger and data layer tests |

### 8.2 End-to-End Tests (Playwright)

| Suite | Location | Description |
|-------|----------|-------------|
| Basic UI Tests | `playwright/basic-tests/` | Core navigation + smoke tests |
| UI Tests | `playwright/ui-tests/` | Full user-journey UI tests |
| API Tests | `playwright/api-tests/` | API endpoint validation |

**Playwright configurations:**

| Config | Purpose |
|--------|---------|
| `playwright.config.ts` | Default local configuration |
| `playwright.config.local.ts` | Local development |
| `playwright.config.cicd.ts` | CI/CD pipeline execution |
| `playwright.config.test-service.ts` | Azure Playwright Test Service |
| `playwright.config.workspace.ts` | Workspace-scoped |

---

## 9. Authentication & Security

```
┌─────────────────────────────────────────────────────────┐
│                    Authentication Paths                 │
│                                                         │
│  Browser User ──→ Microsoft Entra ID (OIDC)             │
│                   Microsoft.Identity.Web                │
│                   JWT Bearer tokens                     │
│                                                         │
│  API Consumer ──→ API Key header (X-Api-Key)            │
│                   AuthorizeApiKey attribute             │
│                   Falls back to anonymous               │
│                                                         │
│  Azure Services → DefaultAzureCredential                │
│                   Managed Identity (User + System)      │
│                   No passwords in config                │
└─────────────────────────────────────────────────────────┘
```

**Security features:**
- Azure Key Vault for all secrets at runtime
- Managed Identity eliminates credential management
- HTTPS enforced throughout
- API key validation on all REST API routes
- `[AllowAnonymous]` selectively applied to read-only pages
- No hardcoded secrets — all config via environment variables or Key Vault

---

## 10. Theme and UI

- **MudBlazor 9.2.0** component library throughout
- Light/dark theme toggle stored in browser `localStorage`
- **Retro 90s theme** easter egg (toggled via theme key `"nineties"`)
- Scoped CSS per component via `.razor.css` pattern
- Bootstrap spacing utilities and CSS variables
- Responsive layout — tested across viewport sizes

---

## 11. Configuration Reference

Key application settings (`AppSettings` / `applicationSettings.json`):

| Setting | Description |
|---------|-------------|
| `DataSource` | `JSON` or `SQL` — selects data repository |
| `DefaultConnection` | SQL connection string (when DataSource = SQL) |
| `KeyVaultName` | Azure Key Vault name for secret loading |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights telemetry endpoint |
| `AzureOpenAIChatEndpoint` | Azure OpenAI chat endpoint URL |
| `AzureOpenAIChatDeploymentName` | GPT-4o deployment name |
| `AzureOpenAIChatApiKey` | OpenAI API key (or use Managed Identity) |
| `AzureOpenAIImageEndpoint` | DALL-E endpoint URL |
| `AzureOpenAIImageDeploymentName` | DALL-E 3 deployment name |
| `AzureOpenAIImageApiKey` | Image API key |
| `VisualStudioTenantId` | Local dev only — scopes VS credentials to a tenant |
| `SwaggerEnabled` | `true`/`false` — toggles Swagger UI |
| `ApiKey` | Expected API key value for API key auth |
| `AdminUsers` | Comma-separated admin user list |

---

## 12. Key Design Patterns

| Pattern | Where Used | Description |
|---------|-----------|-------------|
| Repository Pattern | `DadABase.Data` | `IJokeRepository` abstracts data source (JSON vs SQL) |
| Code-Behind | All Blazor pages | `.razor` + `.razor.cs` separation of concerns |
| Scoped CSS | All components | `.razor.css` per component |
| Dependency Injection | `Program.cs` | All services registered via DI |
| Options Pattern | `AppSettings` | Typed configuration object |
| Dual Data Source | Data layer | Runtime switch between JSON and SQL |
| API Key Auth | REST controllers | Custom attribute-based gate |
| Managed Identity | Azure connections | `DefaultAzureCredential` throughout |
| Agent Framework | `AIHelper.cs` | Multiple `AIAgent` instances for different tasks |
| DACPAC | Database project | Schema-as-code with automated deployment |

---

## 13. Copilot / GitHub Copilot Configuration

The repository includes an extensive GitHub Copilot configuration in `.github/`:

| Path | Purpose |
|------|---------|
| `copilot-instructions.md` | Global coding conventions for Copilot |
| `instructions/` | Domain-specific instruction files (7 files) |
| `skills/` | Specialized Copilot custom skills (100+ skills) |
| `agents/` | Custom Copilot agent definitions (34 agents) |
| `prompts/` | Reusable prompt templates (12 files) |
| `actions/` | Reusable composite GitHub Actions (`load-project-config/`, `login-action/`) |
| `config/` | Repository-level configuration files |
| `scripts/` | Utility scripts |

The Copilot instructions enforce: Blazor code-behind pattern, scoped CSS, PascalCase/camelCase conventions, file-scoped namespaces, async/await, DI-first design, and Bootstrap + CSS variables for theming.

---

## 14. Azure Developer CLI (AZD) Integration

The repository is `azd`-compatible:

| File | Purpose |
|------|---------|
| `azure.yaml` | AZD service and infrastructure mapping |
| `infra/azd-main.bicep` | AZD-specific Bicep entry point |
| `infra/azd-main.parameters.json` | AZD parameter defaults |
| `.github/workflows/azure-dev.yml` | AZD CI/CD integration |

---

## 15. Documentation

| Document | Location | Description |
|----------|----------|-------------|
| README | `README.md` | Project overview and quick start |
| Coding Standards | `Docs/Coding_Standards.md` | Team conventions |
| Infrastructure as Code | `Docs/Infra_As_Code.md` | Bicep deployment guide |
| SQL DACPAC | `Docs/SQL-DacPac.md` | Database deployment guide |
| Azure DevOps Pipelines | `Docs/YML_AzDO.md` | AzDO pipeline reference |
| GitHub Actions | `Docs/YML_GitHub.md` | GHA workflow reference |
| Deployment Options | `Docs/Deployment_Options.md` | Deployment method comparison |
| Deployment Quick Ref | `Docs/Deployment_QuickRef.md` | Quick reference card |
| Export Functionality | `Docs/ExportFunctionality.md` | Data export guide |
| SQL Permissions | `Docs/SQL-Permissions-Queries.md` | SQL permission reference |
| Database Fallback | `Docs/DATABASE-FALLBACK.md` | JSON fallback mode guide |
| .NET 10 Upgrade Notes | `Docs/console-dotnet10-upgrade.md` | Upgrade notes |
| Architecture (this doc) | `Docs/DadABase-Architecture.md` | This document |
| Architecture PDF | `Docs/DadABase-Architecture.pdf` | PDF export of this document |
| Architecture Deck | `Docs/DadABase-Architecture.pptx` | PowerPoint presentation deck |

---

*Generated by GitHub Copilot architecture-export skill — 2026-04-06 · Updated to v1.1*
