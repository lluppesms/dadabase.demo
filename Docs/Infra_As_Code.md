# Infrastructure as Code Guidelines for DadABase Projects

This document outlines the structured approach to Infrastructure as Code (IaC) used in the DadABase project. Following these guidelines will help maintain consistency across future projects.

## Table of Contents
1. [Folder Structure](#folder-structure)
2. [File Naming Conventions](#file-naming-conventions)
3. [Resource Naming Standards](#resource-naming-standards)
4. [Bicep File Structure](#bicep-file-structure)
5. [Deployment Methods](#deployment-methods)
6. [Parameterization](#parameterization)
7. [Tagging Strategy](#tagging-strategy)
8. [Environment Considerations](#environment-considerations)
9. [Best Practices](#best-practices)
10. [Examples](#examples)

## Folder Structure

```
/infra
│   azd-main.bicep               # Main entry point for Azure Developer CLI (azd) deployments
│   azd-main.parameters.json     # ARM JSON parameters for azd deployments
└───Bicep/
    │   main.bicep               # Main template that deploys all resources
    │   main.bicepparam          # Bicep native parameter file (token-replaced by CI/CD)
    │   main.parameters.json     # ARM JSON parameter file (token-replaced by CI/CD)
    │   resourcenames.bicep      # Centralized resource naming module
    │   Run_Deploy_Locally.ps1   # Helper script for local manual deployments
    ├───data/
    │       resourceAbbreviations.json  # Resource abbreviation definitions
    │       roleDefinitions.json        # RBAC role definitions
    ├───modules/
    │   ├───container/
    │   │       containerapp.bicep
    │   │       containerappenvironment.bicep
    │   │       containerregistry.bicep
    │   ├───database/
    │   │       sqlserver.bicep
    │   ├───function/
    │   │       functionapp.bicep
    │   │       functionappsettings.bicep
    │   │       functionflex.bicep
    │   │       functionflexrbac.bicep
    │   ├───functions/
    │   │       functionresources.bicep
    │   ├───iam/
    │   │       identity.bicep
    │   │       roleassignments.bicep
    │   ├───monitor/
    │   │       loganalyticsworkspace.bicep
    │   ├───security/
    │   │       keyvault.bicep
    │   │       keyvaultlistsecretnames.bicep
    │   │       keyvaultsecretstorageconnection.bicep
    │   ├───storage/
    │   │       storageaccount.bicep
    │   └───webapp/
    │           website.bicep
    │           websiteserviceplan.bicep
    └───scripts/
            Add-FederatedCredentials.ps1
            AddCosmosRole.ps1
```

### Key Components

- **Entry Points**:
  - `Bicep/main.bicep`: Primary Bicep file for CI/CD pipeline deployments
  - `azd-main.bicep`: Entry point for Azure Developer CLI (`azd`) deployments — lives at the `/infra/` root, not under `Bicep/`

- **Resource Modules**: All resource modules live under `Bicep/modules/` organized by resource category:
  - `container/`: Container Apps, Container Apps Environment, Container Registry
  - `database/`: SQL Server / Azure SQL Database
  - `function/`: Function App (consumption, flex) and settings
  - `functions/`: Aggregated function resources module
  - `iam/`: User-assigned Managed Identity and RBAC role assignments
  - `monitor/`: Log Analytics Workspace
  - `security/`: Key Vault and Key Vault secret helpers
  - `storage/`: Storage Account
  - `webapp/`: Web App and App Service Plan

- **Supporting Files**:
  - `resourcenames.bicep`: Centralized resource naming logic — all names computed here and passed as outputs to other modules
  - `data/resourceAbbreviations.json`: Resource name abbreviations used by `resourcenames.bicep`
  - `modules/iam/roleassignments.bicep`: All RBAC role assignments in one file, separating security-admin operations from resource provisioning. This allows deployments to skip role assignments when the pipeline service principal lacks Owner/User Access Administrator rights.

## File Naming Conventions

- **Main Files**: `main.bicep` (under `Bicep/`), `azd-main.bicep` (under `infra/` root)
- **Resource Modules**: All lowercase, no separators — concatenate the resource type words directly
  - Examples: `website.bicep`, `storageaccount.bicep`, `containerregistry.bicep`, `loganalyticsworkspace.bicep`, `roleassignments.bicep`
- **Parameter Files**: Two formats are maintained in parallel:
  - `main.bicepparam`: Bicep native format; references `main.bicep` via `using` and uses `#{TOKEN}#` placeholders replaced at CI/CD runtime
  - `main.parameters.json`: ARM JSON format with the same `#{TOKEN}#` placeholders, used by older pipeline tooling
  - `azd-main.parameters.json`: ARM JSON format using `${AZURE_*}` environment variable substitution for `azd` deployments
- **Scripts**: Use PascalCase with descriptive verb-noun names
  - Examples: `Run_Deploy_Locally.ps1`, `Add-FederatedCredentials.ps1`, `AddCosmosRole.ps1`

## Resource Naming Standards

Resource naming is centralized in `resourcenames.bicep` which provides consistent naming across all deployments. All other modules receive names as parameters — they never compute names themselves.

1. **Naming Pattern**: `[sanitizedAppName][instanceNumber]-[resourceAbbreviation]-[environmentCode]`
   - The app name is lowercased with spaces/underscores stripped; `-` separators are retained for most names but stripped for storage and key vault names
   - Example: `dadabase1-appsvcplan-dev` for an App Service Plan in dev
   - For `prod`, some names omit the environment suffix

2. **Length-Constrained Resources**: Key Vault and Storage Account names are truncated to 24 characters using Bicep's `take()` function

3. **Abbreviations**: Loaded at runtime from `data/resourceAbbreviations.json`:
   - `appServicePlanSuffix` for App Service Plans
   - `appInsightsSuffix` for Application Insights
   - `logWorkspaceSuffix` for Log Analytics Workspaces
   - `functionApp` for Function Apps
   - `containerRegistry`, `containerApp`, `containerAppEnvironment` for container resources
   - `keyVaultAbbreviation`, `storageAccountSuffix`, `managedIdentity`, `sqlAbbreviation` for other resources

4. **Environment Codes**: Standardized environment identifiers:
   - `dev`, `demo`, `qa`, `stg`, `prod` for normal environments
   - Special codes: `azd` for Azure Developer CLI deployments

## Bicep File Structure

Each Bicep file should follow this structure:

1. **Header Comment Block** (use a line of dashes, 80 chars is the standard width):
   ```bicep
   // --------------------------------------------------------------------------------
   // This BICEP file will create [resource description]
   // --------------------------------------------------------------------------------
   // Optional: include a manual test deploy command for reference
   // --------------------------------------------------------------------------------
   ```

2. **Parameters Section**: resource modules do NOT receive `appName` or `environmentCode` directly — they get pre-computed names from `resourcenames.bicep` via `main.bicep`:
   ```bicep
   param webSiteName string = ''
   param location string = resourceGroup().location
   param environmentCode string = 'dev'
   param commonTags object = {}
   ```

3. **Variables Section** — always include a `templateTag` and merge it with `commonTags`:
   ```bicep
   var templateTag = { TemplateFile: '~[filename].bicep' }
   var tags = union(commonTags, templateTag)
   ```
   For `azd` deployments, some modules also add `azdTag`:
   ```bicep
   var azdTag = environmentCode == 'azd' ? { 'azd-service-name': 'web' } : {}
   var webSiteTags = union(commonTags, templateTag, azdTag)
   ```

4. **Resource Definitions** — reference named existing resources via the `existing` keyword when needed:
   ```bicep
   resource webSite 'Microsoft.Web/sites@2024-11-01' = {
     // Resource properties
   }
   ```

5. **Outputs Section** — all meaningful identifiers and connection values should be output:
   ```bicep
   output webSiteName string = webSite.name
   output webSiteId string = webSite.id
   output systemPrincipalId string = webSite.identity.principalId
   ```

## Parameterization

### Parameter Types

1. **Required Parameters** (`main.bicep`):
   - `appName`: Base name for all resources (no spaces; used to derive all resource names)
   - `environmentCode`: Environment identifier (dev, qa, prod, azd, etc.)
   - `deploymentType`: Controls which resource sets are deployed — `'webapp'`, `'containerapp'`, `'functionapp'`, or `'all'`

2. **Optional with Defaults**:
   - `location`: Default is `resourceGroup().location`
   - `instanceNumber`: Allows multiple instances of the same app in one environment (default `'1'`)
   - `websiteOnly`: Boolean that skips SQL and Function resources when `true`
   - SKU parameters: `webSiteSku`, `webStorageSku`, `functionStorageSku`, SQL sku params

3. **Sensitive Parameters** (marked `@secure()`):
   - `sqlAdminPassword` and similar credentials
   - Should always be provided via pipeline secrets or Key Vault references — never hardcoded

4. **Calculated Parameters**:
   - `runDateTime`: Defaults to `utcNow()` — used as a deployment suffix and in the `LastDeployed` tag

### Parameter Files

Two parallel parameter file formats are maintained:

- `main.bicepparam` (Bicep native): References `main.bicep` via `using './main.bicep'`; values use `#{TOKEN}#` token replacement syntax filled in by CI/CD pipelines at deploy time
- `main.parameters.json` (ARM JSON): Same `#{TOKEN}#` token replacement, used by older tooling or Azure CLI deployments
- `azd-main.parameters.json` (at `/infra/` root): Uses `${AZURE_ENV_NAME}` / `${AZURE_LOCATION}` environment variable substitution for `azd provision` / `azd up`

## Tagging Strategy

Apply consistent tagging to all resources:

```bicep
var commonTags = {
  LastDeployed: runDateTime
  Application: appName
  Environment: environmentCode
}

var templateTag = { TemplateFile: '~website.bicep' }
var tags = union(commonTags, templateTag)
```

### Required Tags

- **Application**: Name of the application
- **Environment**: Deployment environment
- **LastDeployed**: Timestamp of last deployment
- **TemplateFile**: Source Bicep file name
- **azd-env-name**: For resources deployed with Azure Developer CLI
- **azd-service-name**: For service components in azd deployments

## Environment Considerations

### Multi-Environment Design

The infrastructure is designed to support multiple environments:

1. **Dev/Test Environments**:
   - Use `B1` SKUs for App Service by default
   - Basic monitoring configuration

2. **Production Environments**:
   - Use `S1` or higher SKUs
   - Enhanced monitoring
   - Production naming (no environment suffix on resource names)

### Deployment Type Selection

The `deploymentType` parameter controls which resource sets `main.bicep` provisions:

```bicep
param deploymentType string = 'webapp'  // 'webapp' | 'containerapp' | 'functionapp' | 'all'
```

Internal boolean flags derived from `deploymentType`:
```bicep
var deployWebAppEffective       = contains(['webapp', 'all'], deploymentTypeNormalized)
var deployContainerAppEffective = contains(['containerapp', 'all'], deploymentTypeNormalized)
var deployFunctionEffective     = contains(['functionapp', 'all'], deploymentTypeNormalized) && !websiteOnly
```

### Conditionally Deployed Resources

Use the `if (condition)` modifier on a module to deploy conditionally:

```bicep
module sqlDbModule './modules/database/sqlserver.bicep' = if (!websiteOnly) {
  name: 'sql-server${deploymentSuffix}'
  params: { ... }
}
```

The `websiteOnly` parameter is a shortcut to skip all SQL and Function resources when only the web tier is needed (e.g., when using `appDataSource = 'JSON'`).

## Best Practices

1. **Modularity**:
   - Break down templates into logical, reusable modules
   - Each resource type should have its own Bicep file

2. **Naming Consistency**:
   - Use centralized naming in `resourcenames.bicep`
   - Follow consistent naming patterns for all resources

3. **Dependencies**:
   - Explicitly define dependencies with `dependsOn` if needed, otherwise rely on Bicep's implicit dependency resolution
   - Use symbolic resource references instead of string references

4. **Error Handling**:
   - Include conditional checks for resource existence
   - Provide clear error messages in output

5. **Documentation**:
   - Include thorough comments in Bicep files
   - Document parameter requirements and constraints

6. **Security**:
   - Use managed identities for authentication where possible
   - Never hardcode secrets in templates
   - Apply principle of least privilege for role assignments

7. **Resource Provisioning Order**:
   - Deploy dependencies before dependent resources
   - Use logical dependency chains

## Examples

### Resource Module Example

```bicep
// --------------------------------------------------------------------------------
// This BICEP file will create an Azure Website
// --------------------------------------------------------------------------------
param webSiteName string = ''
param location string = resourceGroup().location
param environmentCode string = 'dev'
param commonTags object = {}
param managedIdentityId string

var templateTag = { TemplateFile: '~website.bicep' }
var azdTag = environmentCode == 'azd' ? { 'azd-service-name': 'web' } : {}
var webSiteTags = union(commonTags, templateTag, azdTag)

resource webSite 'Microsoft.Web/sites@2024-11-01' = {
  name: webSiteName
  location: location
  tags: webSiteTags
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: { '${managedIdentityId}': {} }
  }
  properties: {
    // Properties
  }
}

output webSiteUrl string = 'https://${webSite.properties.defaultHostName}'
output systemPrincipalId string = webSite.identity.principalId
output userManagedPrincipalId string = webSite.identity.userAssignedIdentities[managedIdentityId].principalId
```

### Main Deployment Example

```bicep
// Always compute names first, then pass them to every module
module resourceNames 'resourcenames.bicep' = {
  name: 'resourcenames${deploymentSuffix}'
  params: {
    appName: appName
    environmentCode: environmentCode
    instanceNumber: instanceNumber
  }
}

// Deploy webapp conditionally based on deploymentType
module webSiteModule './modules/webapp/website.bicep' = if (deployWebAppEffective) {
  name: 'website${deploymentSuffix}'
  params: {
    webSiteName: resourceNames.outputs.webSiteName
    location: location
    environmentCode: environmentCode
    commonTags: commonTags
    managedIdentityId: identity.outputs.managedIdentityId
    managedIdentityPrincipalId: identity.outputs.managedIdentityPrincipalId
    appServicePlanName: resourceNames.outputs.webSiteAppServicePlanName
  }
}
```

---

*This document was created to guide infrastructure development for projects similar to DadABase. Follow these practices to maintain consistency and quality across infrastructure deployments.*
