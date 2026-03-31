# Azure DevOps YAML Pipeline Guidelines for DadABase Projects

This document outlines the structured approach to Azure DevOps YAML pipelines used in the DadABase project. Following these guidelines will help maintain consistency across future projects.

## Table of Contents
1. [Pipeline Structure](#pipeline-structure)
2. [File Organization](#file-organization)
3. [Pipeline Types](#pipeline-types)
4. [Variable Management](#variable-management)
5. [Environment Strategy](#environment-strategy)
6. [Templates and Reusability](#templates-and-reusability)
7. [Security and Scanning](#security-and-scanning)
8. [Testing Strategy](#testing-strategy)
9. [Best Practices](#best-practices)
10. [Setup and Configuration](#setup-and-configuration)

## Pipeline Structure

### Main Components
- **Root Pipelines**: Entry points that orchestrate the overall deployment process
- **Pipeline Templates**: Reusable components designed set up all tasks for one environment (or multiple environments)
- **Variable Files**: Environment-specific and common variable definitions
- **Template Steps**: Granular, reusable task sequences

### Standard Pipeline Flow
1. **Parameter Definition**: Define pipeline parameters for user configuration
2. **Variable Loading**: Load environment-specific and common variables
3. **Stages Definition**: Define logical deployment stages
4. **Jobs Configuration**: Configure jobs within each stage
5. **Step Execution**: Execute steps within each job

## File Organization

```
.azdo/
└── pipelines/
    ├── 1-deploy-bicep.yml                   # Infrastructure-only deployment
    ├── 2.1-bicep-build-deploy-webapp.yml    # App Service infrastructure and application deployment
    ├── 2.2-bicep-build-deploy-containerapp.yml # Container Apps infrastructure and container deployment
    ├── 3-bicep-build-deploy-function.yml    # Azure Functions infrastructure and function deployment
    ├── 4-build-deploy-dacpac.yml            # SQL Database deployment
    ├── 5-run-sql-script.yml                 # SQL script execution
    ├── 6-pr-scan-build.yml                  # Pull request scan and build
    ├── 6.1-pr-review-scan-build.yml         # Pull request review with LLM assistance
    ├── 7-scan-code.yml                      # Security scanning pipeline
    ├── 7.1-scan-github-repo.yml             # GitHub repo security scanning
    ├── 8-smoke-test-webapp.yml              # Smoke testing pipeline
    ├── 9-dependabot.yml                     # Automated dependency updates
    ├── 10-deploy-webapp-only-pipeline.yml   # Deploy previously built application
    ├── 11-auto-test-pipeline.yml            # Automated testing pipeline
    ├── dependabot.yml                       # Dependabot configuration
    ├── readme.md                            # Pipeline documentation
    │
    ├── stages/                              # Stage-level templates
    │   ├── bicep-and-webapp-stages.yml      # App Service infrastructure and app stages
    │   ├── bicep-and-containerapp-stages.yml # Container Apps infrastructure and container stages
    │   ├── bicep-and-function-stages.yml    # Azure Functions infrastructure and function stages
    │   ├── bicep-and-schema-stages.yml      # Infrastructure and database schema stages
    │   ├── bicep-only-stages.yml            # Infrastructure-only stages
    │   ├── webapp-build-deploy-stages.yml   # Web app build and deploy stages
    │   ├── webapp-build-stages.yml          # Web app build-only stages
    │   ├── webapp-deploy-stages.yml         # Web app deploy-only stages
    │   ├── function-build-deploy-stages.yml # Azure Functions build and deploy stages
    │   ├── dacpac-deploy-stages.yml         # Database DacPac deployment stages
    │   ├── run-sql-stages.yml               # SQL script execution stages
    │   ├── scan-code-stages.yml             # Code scanning stages
    │   ├── scan-github-repo-stages.yml      # GitHub repo scanning stages
    │   └── pr-review-llm-stages.yml         # PR review with LLM stages
    │
    ├── jobs/                                # Job-level templates
    │   ├── bicep-deploy-job.yml             # Infrastructure deployment job
    │   ├── webapp-build-job.yml             # Web app build job
    │   ├── webapp-deploy-job.yml            # Web app deployment job
    │   ├── containerapp-build-job.yml       # Container image build and push job
    │   ├── containerapp-deploy-job.yml      # Container app deployment job
    │   ├── function-build-job.yml           # Azure Functions build job
    │   ├── function-deploy-job.yml          # Azure Functions deployment job
    │   ├── dacpac-build-job.yml             # Database DacPac build job
    │   ├── dacpac-deploy-job.yml            # Database DacPac deployment job
    │   ├── run-sql-job.yml                  # SQL script execution job
    │   ├── scan-code-job.yml                # Code scanning job
    │   └── playwright-job.yml              # UI testing job
    │
    ├── steps/                               # Step-level templates
    │   ├── bicep-deploy-steps.yml           # Bicep deployment steps
    │   ├── dacpac-deploy-steps.yml          # DacPac deployment steps
    │   ├── run-sql-steps.yml                # SQL script execution steps
    │   ├── run-dbcopy-steps.yml             # Database copy steps
    │   ├── scan-code-compile-steps.yml      # Code scan compile steps
    │   ├── github-dispatch-workflow-steps.yml  # GitHub workflow dispatch steps
    │   ├── github-get-auth-token-steps.yml  # GitHub auth token steps
    │   └── github-monitor-workflow-steps.yml # GitHub workflow monitoring steps
    │
    ├── scripts/                             # Helper scripts
    │   └── Review-PR-Using-LLM.ps1          # PowerShell script for LLM-assisted PR review
    │
    └── vars/                                # Variable definitions
        ├── var-common.yml                   # Common variables across environments
        ├── var-dev.yml                      # Development environment variables
        ├── var-qa.yml                       # QA environment variables
        ├── var-prod.yml                     # Production environment variables
        ├── var-source-location-app.yml      # Source location and project path variables
        └── var-service-connections.yml      # Azure DevOps service connection variables
```

## Pipeline Types

### 1. App Service Deployment Pipeline
- **Purpose**: Complete deployment of infrastructure and web application to Azure App Service
- **Entry Point**: `2.1-bicep-build-deploy-webapp.yml`
- **Uses Stages**: `bicep-and-webapp-stages.yml`
- **Stages**:
  - Optional: Security scanning
  - Infrastructure provisioning (App Service mode)
  - Application build
  - Application deployment
  - Optional: Smoke testing

### 2. Container Apps Deployment Pipeline
- **Purpose**: Complete deployment of infrastructure and containerized application to Azure Container Apps
- **Entry Point**: `2.2-bicep-build-deploy-containerapp.yml`
- **Uses Stages**: `bicep-and-containerapp-stages.yml`
- **Stages**:
  - Optional: Security scanning
  - Infrastructure provisioning (Container Apps mode)
  - Container image build and push to ACR
  - Container app deployment
  - Optional: Smoke testing

### 3. Azure Functions Deployment Pipeline
- **Purpose**: Complete deployment of infrastructure and Azure Functions
- **Entry Point**: `3-bicep-build-deploy-function.yml`
- **Uses Stages**: `bicep-and-function-stages.yml`
- **Stages**:
  - Optional: Security scanning
  - Infrastructure provisioning (Functions mode)
  - Function app build
  - Function app deployment

### 4. Infrastructure-Only Pipeline
- **Purpose**: Deploy only the Azure infrastructure
- **Entry Point**: `1-deploy-bicep.yml`
- **Uses Stages**: `bicep-only-stages.yml`
- **Stages**:
  - Infrastructure provisioning

### 5. Application-Only Pipeline
- **Purpose**: Deploy a previously built application without infrastructure changes
- **Entry Point**: `10-deploy-webapp-only-pipeline.yml`
- **Uses Stages**: `webapp-deploy-stages.yml`
- **Stages**:
  - Application deployment

### 6. Database Deployment Pipelines
- **Purpose**: Deploy database schema and data
- **Entry Points**: `4-build-deploy-dacpac.yml`, `5-run-sql-script.yml`
- **Uses Stages**: `dacpac-deploy-stages.yml`, `run-sql-stages.yml`
- **Stages**:
  - Optional: Infrastructure provisioning
  - DacPac build and deployment or SQL script execution

### 7. Scanning and Testing Pipelines
- **Purpose**: Security scanning and testing
- **Entry Points**: `6-pr-scan-build.yml`, `7-scan-code.yml`, `8-smoke-test-webapp.yml`, `11-auto-test-pipeline.yml`
- **Uses Stages**: `scan-code-stages.yml`
- **Stages**:
  - Code scanning
  - Unit testing
  - UI testing

## Variable Management

### Variable Hierarchy
1. **Pipeline Parameters**: User-configurable options defined at pipeline runtime
2. **Variable Groups**: Central, shared variables across pipelines (e.g., `DadABase.Web`)
3. **Environment-Specific Variables**: Variables defined in environment-specific files
4. **Common Variables**: Shared variables across all environments

### Variable Files
- **var-common.yml**: Common settings (resource group prefix, location, SKUs, project paths)
- **var-dev.yml**, **var-qa.yml**, **var-prod.yml**: Environment-specific overrides
- **var-service-connections.yml**: Azure DevOps service connection configurations

### Variable Group Requirements

For projects, a variable group similar to this is required, which will defined variables that are UNIQUE to this deployment of this project:

``` yml
Dadabase.Demo
  - appName
  - apiKey
  - adDomain
  - serviceConnectionName
  - RESOURCE_GROUP_LOCATION
  - APP_NAME
```

## Environment Strategy

### Multi-Stage Deployment
- **Development (DEV)**: Continuous integration, minimal approvals
- **QA**: Testing environment, may require approvals
- **Production (PROD)**: Live environment, requires approvals

### Environment-Specific Configuration
- Different resource SKUs per environment
- Different approval requirements
- Environment-specific variable overrides

### Dynamic Environment Selection
- User-selectable environment via pipeline parameters
- Conditional stage execution based on environment selection

## Templates and Reusability

### Template Hierarchy
1. **Root Pipeline Files**: Entry points with parameters and stage orchestration
2. **Stage Files** (`stages/`): Stage-level templates that organize jobs and contain all tasks for one or more environments
3. **Job Files** (`jobs/`): Job-level templates with reusable task sequences
4. **Step Files** (`steps/`): Granular, reusable steps for specific operations

### Template Parameters
- All templates should accept parameters for customization
- Default values should be provided where appropriate
- Clear parameter documentation within the template

### Template Types
- **Stage Templates**: `*-stages.yml` files in the `stages/` folder
- **Job Templates**: `*-job.yml` files in the `jobs/` folder
- **Step Templates**: `*-steps.yml` files in the `steps/` folder

## Security and Scanning

### Scanning Integration
- GitHub Advanced Security (GHAS) scanning
- Microsoft DevSecOps scanning
- Custom security scanning steps

### Scanning Options
- Configurable via pipeline parameters
- Integration with build validation
- Scheduled security scanning

### Secure Variable Handling
- Sensitive data stored in variable groups or key vaults
- Secrets masked in pipeline logs
- Principle of least privilege for service connections

## Testing Strategy

### Testing Levels
1. **Unit Tests**: Run during build stage
2. **UI Tests**: Run post-deployment
3. **Smoke Tests**: Validate deployment success

### Testing Configuration
- Optional test execution via parameters
- Configurable test types (unit, UI)
- Test results published as pipeline artifacts

## Best Practices

### Pipeline Design
1. **Modularity**: Use templates for reusable components
2. **Flexibility**: Provide parameters for customization
3. **Readability**: Use clear stage and job names
4. **Efficiency**: Minimize redundant steps

### Environment Isolation
1. **Service Connections**: Use separate service connections per environment
2. **Variable Overrides**: Use environment-specific variable files
3. **Approvals**: Configure appropriate approval gates

### Code Management
1. **Template Versioning**: Version templates for backward compatibility
2. **Documentation**: Maintain clear documentation in readme files
3. **Parameter Defaults**: Provide sensible defaults for optional parameters

## Setup and Configuration

### Prerequisites
1. **Service Connections**: Azure service connections for deployments
2. **Environments**: DevOps environments with appropriate approvals
3. **Variable Groups**: Required variable groups with necessary values

### Pipeline Creation Steps
1. Create Azure DevOps service connections for each environment
2. Create Azure DevOps environments with appropriate approval policies
3. Create the required variable group with appropriate values
4. Import the desired pipeline YAML file
5. Run the pipeline with appropriate parameters

### Service Connection Configuration
- Service connection per environment
- Appropriate permissions for resource deployment
- RBAC principles applied to service identities

---

## Example Usage

### 1. All-in-One Deployment
```yaml
trigger:
  branches:
    include:
      - main

parameters:
  - name: deployToEnvironment
    displayName: Deploy To
    type: string
    values:
      - DEV
      - QA
      - PROD
    default: DEV

variables:
  - group: Dadabase.Demo
  - template: vars/var-service-connections.yml

stages:
  - template: stages/bicep-and-webapp-stages.yml
    parameters:
      environments: [${{ parameters.deployToEnvironment }}]
      runUnitTests: true
```

### 2. Infrastructure-Only Deployment
```yaml
parameters:
  - name: deployToEnvironment
    displayName: Deploy To
    type: string
    values:
      - DEV
      - QA
      - PROD
    default: DEV

variables:
  - group: Dadabase.Demo
  - template: vars/var-service-connections.yml

stages:
  - template: pipes/infra-only-pipe.yml
    parameters:
      environments: [${{ parameters.deployToEnvironment }}]
```

---

*This document was created to guide Azure DevOps pipeline development for projects similar to DadABase. Follow these practices to maintain consistency and quality across CI/CD deployments.*
