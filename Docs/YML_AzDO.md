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
├── pipelines/
│   ├── 1-deploy-bicep.yml                   # Infrastructure-only deployment
│   ├── 3.1-bicep-build-deploy-webapp.yml    # App Service infrastructure and application deployment
│   ├── 3.2-bicep-build-deploy-containerapp.yml # Container Apps infrastructure and container deployment
│   ├── 4-build-deploy-dacpac.yml            # SQL Database deployment
│   ├── 5-run-sql-script.yml                        # SQL script execution
│   ├── 6-pr-scan-build.yml                  # Pull request scan and build
│   ├── 7-scan-code.yml                      # Security scanning pipeline
│   ├── 8-smoke-test-webapp.yml              # Smoke testing pipeline
│   ├── 9-build-deploy-webapp.yml            # Build and deploy web application
│   ├── 9-deploy-webapp-only-pipeline.yml    # Deploy previously built application
│   ├── 10-auto-test-pipeline.yml            # Automated testing pipeline
│   ├── 10-infra-build-deploy-dacpac.yml     # Infrastructure and database deployment
│   ├── readme.md                            # Pipeline documentation
│   │
│   ├── pipes/                               # Modular pipeline components
│   │   ├── infra-and-webapp-pipe.yml        # App Service infrastructure and app stage template
│   │   ├── infra-and-containerapp-pipe.yml  # Container Apps infrastructure and container stage template
│   │   ├── infra-and-function-pipe.yml      # Azure Functions infrastructure and function stage template
│   │   ├── infra-and-schema-pipe.yml        # Infrastructure and database schema stage template
│   │   ├── infra-only-pipe.yml              # Infrastructure-only stage template
│   │   ├── webapp-build-deploy-pipe.yml     # Web app build and deploy stage template
│   │   ├── webapp-build-pipe.yml            # Web app build stage template
│   │   ├── webapp-deploy-pipe.yml           # Web app deploy-only stage template
│   │   ├── function-only-pipe.yml           # Azure Functions-only stage template
│   │   ├── schema-only-pipe.yml             # Database schema-only stage template
│   │   ├── run-sql-pipe.yml                 # SQL script execution stage template
│   │   ├── scan-code-pipe.yml               # Code scanning stage template
│   │   │
│   │   └── templates/                       # Reusable job templates
│   │       ├── bicep-deploy-template.yml    # Infrastructure creation job template
│   │       ├── steps-bicep-deploy-template.yml # Bicep deployment steps
│   │       ├── webapp-build.yml             # Web app build job template
│   │       ├── webapp-deploy.yml            # Web app deployment job template
│   │       ├── containerapp-build.yml       # Container build and push job template
│   │       ├── containerapp-deploy.yml      # Container app deployment job template
│   │       ├── function-build.yml           # Azure Functions build job template
│   │       ├── function-deploy.yml          # Azure Functions deployment job template
│   │       ├── dacpac-build-template.yml    # Database DacPac build job template
│   │       ├── dacpac-deploy-template.yml   # Database DacPac deployment job template
│   │       ├── steps-dacpac-deploy-template.yml # DacPac deployment steps
│   │       ├── run-sql-template.yml         # SQL script execution job template
│   │       ├── steps-run-sql-template.yml   # SQL execution steps
│   │       ├── steps-run-dbcopy-template.yml # Database copy steps
│   │       ├── playwright-template.yml      # UI testing job template
│   │       └── scan-code-template.yml       # Code scanning job template
│   │
│   └── vars/                                # Variable definitions
│       ├── var-common.yml                   # Common variables across environments
│       ├── var-dev.yml                      # Development environment variables
│       ├── var-qa.yml                       # QA environment variables
│       ├── var-prod.yml                     # Production environment variables
│       └── var-service-connections.yml      # Azure DevOps service connection variables
```

## Pipeline Types

### 1. App Service Deployment Pipeline
- **Purpose**: Complete deployment of infrastructure and web application to Azure App Service
- **Entry Point**: `3.1-bicep-build-deploy-webapp.yml`
- **Uses Pipe**: `infra-and-webapp-pipe.yml`
- **Stages**:
  - Optional: Security scanning
  - Infrastructure provisioning (App Service mode)
  - Application build
  - Application deployment
  - Optional: Smoke testing

### 2. Container Apps Deployment Pipeline
- **Purpose**: Complete deployment of infrastructure and containerized application to Azure Container Apps
- **Entry Point**: `3.2-bicep-build-deploy-containerapp.yml`
- **Uses Pipe**: `infra-and-containerapp-pipe.yml`
- **Stages**:
  - Optional: Security scanning
  - Infrastructure provisioning (Container Apps mode)
  - Container image build and push to ACR
  - Container app deployment
  - Optional: Smoke testing

### 3. Infrastructure-Only Pipeline
- **Purpose**: Deploy only the Azure infrastructure
- **Entry Point**: `1-deploy-bicep.yml`
- **Uses Pipe**: `infra-only-pipe.yml`
- **Stages**:
  - Infrastructure provisioning

### 4. Application-Only Pipeline
- **Purpose**: Build and deploy only the application (no infrastructure changes)
- **Entry Points**: `9-build-deploy-webapp.yml` or `9-deploy-webapp-only-pipeline.yml`
- **Uses Pipe**: `webapp-build-deploy-pipe.yml` or `webapp-deploy-pipe.yml`
- **Stages**:
  - Application build (optional)
  - Application deployment

### 5. Database Deployment Pipelines
- **Purpose**: Deploy database schema and data
- **Entry Points**: `4-build-deploy-dacpac.yml`, `10-infra-build-deploy-dacpac.yml`, `5-run-sql-script.yml`
- **Uses Pipes**: `infra-and-schema-pipe.yml`, `schema-only-pipe.yml`, `run-sql-pipe.yml`
- **Stages**:
  - Optional: Infrastructure provisioning
  - DacPac build and deployment or SQL script execution

### 6. Scanning and Testing Pipelines
- **Purpose**: Security scanning and testing
- **Entry Points**: `7-scan-code.yml`, `10-auto-test-pipeline.yml`, `8-smoke-test-webapp.yml`
- **Uses Pipes**: `scan-code-pipe.yml`
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
DadABase.Web
  - appName
  - apiKey
  - adDomain
  - serviceConnectionName
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
2. **Pipe Files**: Stage-level templates that organize jobs and container all of the steps necessary to deploy one environment
3. **Template Files**: Job-level templates with reusable task sequences
4. **Step Templates**: Granular, reusable steps for common operations

### Template Parameters
- All templates should accept parameters for customization
- Default values should be provided where appropriate
- Clear parameter documentation within the template

### Template Types
- **Stage Templates**: `*-pipe.yml` files defining complete stages
- **Job Templates**: `*-template.yml` files defining reusable jobs
- **Step Templates**: Reusable step sequences within templates

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
  - template: pipes/infra-and-webapp-pipe.yml
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
