# DadABase Web Application - Deployment Guide

This guide explains the two deployment options for the DadABase web application: **Azure App Service** and **Azure Container Apps**.

## Table of Contents

- [Overview](#overview)
- [Deployment Options](#deployment-options)
  - [Option 1: Azure App Service](#option-1-azure-app-service)
  - [Option 2: Azure Container Apps](#option-2-azure-container-apps)
- [Architecture Comparison](#architecture-comparison)
- [Prerequisites](#prerequisites)
- [Deployment Instructions](#deployment-instructions)
- [When to Use Which Option](#when-to-use-which-option)

## Overview

The DadABase web application can be deployed using two different Azure hosting options:

1. **Azure App Service** - Traditional PaaS hosting with code deployment
2. **Azure Container Apps** - Container-based deployment with advanced scaling

Both options share common infrastructure:

- Azure SQL Database
- Azure Key Vault
- Azure Storage Account
- Application Insights & Log Analytics
- Managed Identity for authentication

## Deployment Options

### Option 1: Azure App Service

**What it is:** Platform-as-a-Service (PaaS) hosting where you deploy compiled .NET code directly to Azure.

**Infrastructure Components:**

- App Service Plan (compute resources)
- App Service (web application host)
- Application Insights (monitoring)
- Shared resources (SQL, Storage, Key Vault)

**Deployment Method:**

- Code is built on GitHub Actions runner
- Artifacts are zipped and deployed via `webapps-deploy` action
- .NET 10.0 runtime is provided by the App Service platform

**Bicep Parameter:**

```bicep
param deploymentType = 'webapp'
```

**CI/CD Workflows:**

- **GitHub Actions:** `.github/workflows/3-bicep-build-deploy-webapp.yml` (Workflow: **3.bicep.build.deploy.webapp**)
- **Azure DevOps:** `.azdo/pipelines/3.1-bicep-build-deploy-webapp.yml` (Pipeline: **3.1-bicep-build-deploy-webapp**)

Both use parameter file `infra/Bicep/main.bicepparam` with web app deployments using `deploymentType='webapp'`.

### Option 2: Azure Container Apps

**What it is:** Modern, container-based hosting platform with Kubernetes-like capabilities and serverless scaling.

**Infrastructure Components:**
- Azure Container Registry (image storage)
- Container Apps Environment (shared compute environment)
- Container App (application instance)
- Application Insights (monitoring)
- Shared resources (SQL, Storage, Key Vault)

**Deployment Method:**

- Docker image is built from source
- Image is pushed to Azure Container Registry
- Container App pulls and runs the image
- Automatic image-based deployments

**Bicep Parameter:**

```bicep
param deploymentType = 'containerapp'
param containerImage = 'your-registry.azurecr.io/dadabase-web:latest'
```

**CI/CD Workflows:**

- **GitHub Actions:** `.github/workflows/3.1-bicep-build-deploy-containerapp.yml` (Workflow: **3.1.bicep.build.deploy.containerapp**)
- **Azure DevOps:** `.azdo/pipelines/3.2-bicep-build-deploy-containerapp.yml` (Pipeline: **3.2-bicep-build-deploy-containerapp**)

GitHub Actions and Azure DevOps both use the shared `infra/Bicep/main.bicepparam`, with deployment mode selected via `deploymentType='containerapp'` for this workflow.

## Architecture Comparison

| Feature | App Service | Container Apps |
|---------|-------------|----------------|
| **Deployment Unit** | Compiled .NET code (zip) | Docker container image |
| **Runtime Management** | Managed by Azure | Packaged in container |
| **Scaling** | Manual or auto-scale rules | Automatic scale-to-zero + HTTP scaling |
| **Minimum Cost** | Always running (Basic tier) | Scale to zero (consumption-based) |
| **Networking** | Built-in SSL/TLS | Automatic HTTPS with ingress |
| **Deployment Speed** | Fast (~1-2 min) | Moderate (~3-5 min, includes image build) |
| **Development Experience** | .NET-native, direct deployment | Docker-based, portable |
| **HTTPS Configuration** | Automatic with App Service domains | Automatic with Container Apps domains |

## Prerequisites

### Common for Both Options

1. **Azure Subscription** with appropriate permissions
2. **Azure CLI** installed and authenticated

### For GitHub Actions

3. **GitHub Repository Secrets** configured:
   - `AZURE_CLIENT_ID`
   - `AZURE_CLIENT_SECRET`
   - `AZURE_TENANT_ID`
   - `AZURE_SUBSCRIPTION_ID`

4. **GitHub Repository Variables** configured:
   - `APP_NAME`
   - `INSTANCE_NUMBER`
   - `RESOURCE_GROUP_NAME`
   - `RESOURCE_GROUP_LOCATION`
   - SQL, OpenAI, and authentication settings

### For Azure DevOps

3. **Azure DevOps Service Connection** configured with appropriate permissions
4. **Azure DevOps Variable Group** named `Dadabase.Demo` with:
   - `#{APP_NAME}#`
   - `#{INSTANCE_NUMBER}#`
  - `#{ENVCODE}#` (`dev`, `qa`, or `prod`)
   - `#{RESOURCE_GROUP_LOCATION}#`
   - SQL, OpenAI, and authentication settings (see variable group for complete list)

### Additional for Container Apps

5. **Docker** installed locally (for testing)
6. **Container image** already built or will be built by pipeline/workflow

## Deployment Instructions

### Deploy with Azure App Service

#### Via GitHub Actions

1. Go to **Actions** tab in your GitHub repository
2. Select workflow: **3.bicep.build.deploy.webapp**
3. Click **Run workflow**
4. Configure options:
   - Environment: `dev`, `staging`, or `prod`
   - Create Resource Group: `true` (first time)
   - Deploy Bicep: `true`
   - Bicep Mode: `create`
   - Build Web App: `true`
   - Deploy Web App: `true`
5. Click **Run workflow**

#### Via Azure DevOps

1. Go to **Pipelines** in your Azure DevOps project
2. Select pipeline: **3.1-bicep-build-deploy-webapp**
3. Click **Run pipeline**
4. Configure parameters:
   - Deploy To: `DEV`, `QA`, `PROD`, or `DEV-QA-PROD`
   - Deploy Bicep Infrastructure: `true`
   - Run Unit Tests: `true`
   - Run Playwright Smoke Test: `true`
   - Run MS DevSecOps Scan: `true`
   - Run GHAS Scan: `false`
5. Click **Run**

The pipeline uses parameter file `infra/Bicep/main.bicepparam`; deployment mode is selected by pipeline parameter `deploymentType` (set to `webapp` for this pipeline).

#### Via Azure CLI

```bash
# Set variables
RESOURCE_GROUP="rg-dadabase-dev"
LOCATION="eastus"
APP_NAME="dadabase"
ENV_CODE="dev"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Deploy infrastructure
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file infra/Bicep/main.bicep \
  --parameters \
    appName=$APP_NAME \
    environmentCode=$ENV_CODE \
    deploymentType=webapp

# Build and deploy application
dotnet publish src/web/Website/DadABase.Web.csproj -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
az webapp deploy \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME-$ENV_CODE \
  --src-path ../deploy.zip
```

### Deploy with Azure Container Apps

#### Via GitHub Actions

1. Go to **Actions** tab in your GitHub repository
2. Select workflow: **3.1.bicep.build.deploy.containerapp**
3. Click **Run workflow**
4. Configure options:
   - Environment: `dev`, `staging`, or `prod`
   - Create Resource Group: `true` (first time)
   - Deploy Bicep: `true`
   - Bicep Mode: `create`
   - Build & Push Docker Image: `true`
   - Deploy Container App: `true`
5. Click **Run workflow**

The workflow will:

- Deploy Container Registry and Container Apps Environment
- Build Docker image from `src/web/Dockerfile`
- Push image to Azure Container Registry
- Deploy Container App with the new image

#### Via Azure DevOps

1. Go to **Pipelines** in your Azure DevOps project
2. Select pipeline: **3.2-bicep-build-deploy-containerapp**
3. Click **Run pipeline**
4. Configure parameters:
   - Deploy To: `DEV`, `QA`, `PROD`, or `DEV-QA-PROD`
   - Deploy Bicep Infrastructure: `true`
   - Run Playwright Smoke Test: `true`
   - Run MS DevSecOps Scan: `true`
   - Run GHAS Scan: `false`
5. Click **Run**

The pipeline will:

- Deploy Container Registry and Container Apps Environment
- Build Docker image from `src/web/Dockerfile`
- Push image to Azure Container Registry
- Deploy Container App with the new image

The pipeline uses parameter file `infra/Bicep/main.bicepparam` with `deploymentType='containerapp'`.

#### Via Azure CLI

```bash
# Set variables
RESOURCE_GROUP="rg-dadabase-dev"
LOCATION="eastus"
APP_NAME="dadabase"
ENV_CODE="dev"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Deploy infrastructure (Container Apps)
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file infra/Bicep/main.bicep \
  --parameters \
    appName=$APP_NAME \
    environmentCode=$ENV_CODE \
    deploymentType=containerapp \
    containerImage='mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

# Get ACR name
ACR_NAME=$(az acr list --resource-group $RESOURCE_GROUP --query "[0].name" -o tsv)
ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --query loginServer -o tsv)

# Build and push Docker image
docker build -t dadabase-web:latest -f src/web/Dockerfile src/web
docker tag dadabase-web:latest $ACR_LOGIN_SERVER/dadabase-web:latest
az acr login --name $ACR_NAME
docker push $ACR_LOGIN_SERVER/dadabase-web:latest

# Update Container App
CONTAINER_APP_NAME=$(az containerapp list -g $RESOURCE_GROUP --query "[0].name" -o tsv)
az containerapp update \
  --name $CONTAINER_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --image $ACR_LOGIN_SERVER/dadabase-web:latest
```

## When to Use Which Option

### Use Azure App Service When:

✅ **Simplicity is priority** - Less infrastructure complexity  
✅ **.NET-native workflow** - Direct code deployment without Docker  
✅ **Predictable load** - Consistent traffic patterns  
✅ **Quick setup** - Faster initial deployment  
✅ **Cost predictability** - Fixed monthly cost  
✅ **Windows hosting needed** - Can run on Windows or Linux  

**Best for:**

- Traditional web applications
- Internal enterprise applications
- Applications with steady traffic
- Teams familiar with App Service

### Use Azure Container Apps When:

✅ **Modern DevOps** - Container-based CI/CD workflow  
✅ **Variable load** - Unpredictable or spiky traffic  
✅ **Scale to zero** - Cost savings during idle periods  
✅ **Microservices** - Part of a larger containerized architecture  
✅ **Local dev parity** - Develop with same containers  
✅ **Advanced scaling** - HTTP-based auto-scaling with multiple replicas  

**Best for:**

- Modern cloud-native applications
- Applications with variable traffic
- Cost-optimized workloads
- Multi-service architectures
- API backends with burst traffic

## Switching Between Deployment Types

You can switch between deployment types by redeploying the infrastructure with a different `deploymentType` parameter:

```bash
# Switch to Container Apps
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file infra/Bicep/main.bicep \
  --parameters deploymentType=containerapp ...

# Switch back to App Service
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file infra/Bicep/main.bicep \
  --parameters deploymentType=webapp ...
```

**Note:** The Bicep template uses conditional deployment - only the resources for the selected deployment type will be created. Shared resources (SQL, Storage, Key Vault) remain the same.

## Monitoring and Troubleshooting

Both deployment options integrate with:

- **Application Insights** for APM and distributed tracing
- **Log Analytics** for log aggregation
- **Azure Monitor** for metrics and alerts

### App Service Logs

```bash
az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP
```

### Container App Logs

```bash
az containerapp logs show --name $CONTAINER_APP_NAME --resource-group $RESOURCE_GROUP --follow
```

## Cost Comparison

### App Service (B1 - Basic Tier)

- **Fixed cost**: ~$13/month (Linux) or ~$55/month (Windows)
- **Always running**: Costs even when idle
- **Scaling**: Manual scale-up/out (costs increase with scale)

### Container Apps (Consumption Plan)

- **Variable cost**: Pay per vCPU-second and memory
- **Scale to zero**: No cost when idle
- **Automatic scaling**: Costs adjust automatically with load
- **Typical cost**: $5-20/month for low-medium traffic

## Additional Resources

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)

---

**Last Updated:** February 2026  
**Repository:** lluppesms/dadabase.demo
