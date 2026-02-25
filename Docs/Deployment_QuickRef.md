# DadABase Deployment - Quick Reference

## Quick Start Commands

### App Service Deployment (Traditional)
```bash
# Deploy infrastructure and code
az deployment group create \
  --resource-group rg-dadabase-dev \
  --template-file infra/Bicep/main.bicep \
  --parameters deploymentType=appservice appName=dadabase environmentCode=dev
```

### Container Apps Deployment (Modern)
```bash
# Deploy infrastructure
az deployment group create \
  --resource-group rg-dadabase-dev \
  --template-file infra/Bicep/main.bicep \
  --parameters deploymentType=containerapp appName=dadabase environmentCode=dev

# Build and push image
ACR_NAME=$(az acr list --resource-group rg-dadabase-dev --query "[0].name" -o tsv)
az acr build --registry $ACR_NAME --image dadabase-web:latest --file src/web/Dockerfile src/web

# Update container app
CONTAINER_APP=$(az containerapp list -g rg-dadabase-dev --query "[0].name" -o tsv)
az containerapp update --name $CONTAINER_APP --resource-group rg-dadabase-dev \
  --image $ACR_NAME.azurecr.io/dadabase-web:latest
```

## CI/CD Workflows

### GitHub Actions

| Workflow | File | Purpose |
|----------|------|---------|
| **App Service** | `.github/workflows/3-bicep-build-deploy-webapp.yml` | Deploy to Azure App Service |
| **Container Apps** | `.github/workflows/3.1-bicep-build-deploy-containerapp.yml` | Deploy to Azure Container Apps |

### Azure DevOps Pipelines

| Pipeline | File | Purpose |
|----------|------|---------|
| **App Service** | `.azdo/pipelines/3.1-bicep-build-deploy-webapp.yml` | Deploy to Azure App Service |
| **Container Apps** | `.azdo/pipelines/3.2-bicep-build-deploy-containerapp.yml` | Deploy to Azure Container Apps |

## Key Files

### Infrastructure (Bicep)
- `infra/Bicep/main.bicep` - Main infrastructure template (supports both types)
- `infra/Bicep/resourcenames.bicep` - Resource naming conventions
- `infra/Bicep/modules/webapp/` - App Service modules
- `infra/Bicep/modules/container/` - Container Apps modules
  - `containerregistry.bicep` - Azure Container Registry
  - `containerappenvironment.bicep` - Container Apps Environment
  - `containerapp.bicep` - Container App instance

### Docker
- `src/web/Dockerfile` - Multi-stage production Dockerfile
- `src/web/.dockerignore` - Files excluded from Docker build

### GitHub Actions Workflows
- `.github/workflows/template-containerapp-build.yml` - Build & push Docker image
- `.github/workflows/template-containerapp-deploy.yml` - Deploy to Container Apps
- `.github/workflows/template-webapp-deploy.yml` - Deploy to App Service

### Azure DevOps Pipelines
- `.azdo/pipelines/pipes/templates/containerapp-build.yml` - Build & push Docker image
### GitHub Actions

| File | Purpose |
|------|---------|
| `main.gha.bicepparam` | App Service deployment parameters (uses `deploymentType=appservice`) |
| `main.gha.containerapp.bicepparam` | Container Apps deployment parameters (uses `deploymentType=containerapp`) |

### Azure DevOps

| File | Purpose |
|------|---------|
| `main.azdo.bicepparam` | App Service deployment parameters (uses `deploymentType=appservice`) |
| `main.azdo.containerapp.bicepparam` | Container Apps deployment parameters (uses `deploymentType=containerapp`)tion

## Parameter Files

| File | Purpose |
|------|---------|
| `main.gha.bicepparam` | App Service deployment parameters |
| `main.gha.containerapp.bicepparam` | Container Apps deployment parameters |

## Environment Variables

Both deployment types require these environment variables:

```bash
# Database
AppSettings__DefaultConnection="SQL connection string"

# Azure OpenAI
AppSettings__AzureOpenAI__Chat__Endpoint="https://..."
AppSettings__AzureOpenAI__Chat__DeploymentName="gpt-4"

# Authentication
AzureAD__TenantId="..."
AzureAD__ClientId="..."

# Application Insights
APPLICATIONINSIGHTS_CONNECTION_STRING="..."
```

## Deployment Decision Tree

```
Need to deploy DadABase?
│
├─ Want simplicity & .NET-native workflow?
│  └─ Use App Service
│
├─ Want containers & scale-to-zero?
│  └─ Use Container Apps
│
├─ Existing App Service plan to share?
│  └─ Use App Service
│
└─ Variable/unpredictable traffic?
   └─ Use Container Apps
```

## Cost Estimates

| Deployment Type | Minimum Monthly Cost | Notes |
|----------------|---------------------|-------|
| **App Service (B1)** | ~$13 | Always running |
| **Container Apps** | ~$5-10 | Scales to zero |

*Excludes SQL, Storage, and other shared resources*

## Common Issues & Solutions

### App Service
**Issue:** Deployment slot swaps not working
- **Solution:** Ensure `WEBSITE_SWAP_WARMUP_PING_PATH` is set

**Issue:** Cold start performance
- **Solution:** Enable "Always On" or upgrade to higher tier

### Container Apps
**Issue:** Image pull authentication failed
- **Solution:** Verify managed identity has AcrPull role on registry

**Issue:** Container fails to start
- **Solution:** Check logs with `az containerapp logs show`

- **Solution:** Verify environment variables are set correctly

## Monitoring & Logs

### App Service
``bash
# Stream logs
az webapp log tail --name $APP_NAME --resource-group $RG_NAME

# View Application Insights
az monitor app-insights query \
  --app $APP_INSIGHTS_NAME \
  --analytics-query "requests | where timestamp > ago(1h)" \
  --resource-group $RG_NAME
```

### Container Apps
```bash
# View console logs
az containerapp logs show \
  --name $CONTAINER_APP_NAME \
  --resource-group $RG_NAME \
  --follow

# View system logs
az containerapp logs show \
  --name $CONTAINER_APP_NAME \
  --resource-group $RG_NAME \
  --type system
```

## Switching Deployment Types

To switch between App Service and Container Apps:

1. Update Bicep parameter: `deploymentType='appservice'` or `'containerapp'`
2. Redeploy infrastructure
3. Deploy application using appropriate workflow

**Note:** Both deployment types can coexist in the same resource group, but you'll pay for both.

## URLs

After deployment, your application will be available at:

- **App Service:** `https://{appname}-{env}.azurewebsites.net`
- **Container Apps:** `https://{appname}-{env}.{region}.azurecontainerapps.io`

## Support

For detailed documentation, see:
- [Deployment_Options.md](Deployment_Options.md) - Comprehensive deployment guide
- [Infra_As_Code.md](Infra_As_Code.md) - Infrastructure overview

---

**Last Updated:** February 2026
