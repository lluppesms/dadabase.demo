# Azure DevOps Deployment Template Notes

## 1. Azure DevOps Pipeline Definitions

### Bicep Parameter File Process

- Azure DevOps now uses a single shared Bicep parameter file: `infra/Bicep/main.bicepparam`.
- Deployment mode is selected through pipeline/template parameter `deploymentType` with supported values: `webapp`, `containerapp`, `functionapp`, `all`.
- The previous mode-specific files (`main.azdo.web.bicepparam`, `main.azdo.containerapp.bicepparam`, `main.azdo.function.bicepparam`) have been retired.

The following pipelines are available for various deployment scenarios.

### Infrastructure and Application Deployment

- [1-deploy-bicep.yml](1-deploy-bicep.yml): Deploys the main.bicep template to Azure (infrastructure only)
- [2.1-bicep-build-deploy-webapp.yml](2.1-bicep-build-deploy-webapp.yml): Deploys Bicep infrastructure and builds/deploys the Blazor Web App to Azure App Service
- [2.2-bicep-build-deploy-containerapp.yml](2.2-bicep-build-deploy-containerapp.yml): Deploys Bicep infrastructure and builds/deploys a containerized application to Azure Container Apps
- [3-bicep-build-deploy-function.yml](3-bicep-build-deploy-function.yml): Deploys Bicep infrastructure and builds/deploys Azure Functions

### Database and Schema

- [4-build-deploy-dacpac.yml](4-build-deploy-dacpac.yml): Builds and deploys the SQL database schema (DACPAC) to Azure SQL Database
- [5-run-sql-script.yml](5-run-sql-script.yml): Runs SQL scripts against an existing Azure SQL Database

### Application Deployment Only

- [10-deploy-webapp-only-pipeline.yml](10-deploy-webapp-only-pipeline.yml): Deploys a previously built Web App to Azure App Service without infrastructure changes

### Code Quality and Security

- [6-pr-scan-build.yml](6-pr-scan-build.yml): Scans and builds code on pull requests
- [7-scan-code.yml](7-scan-code.yml): Performs periodic security scans (GitHub Advanced Security and Microsoft Secure DevOps)
- [9-dependabot.yml](dependabot.yml): Automated dependency updates via Dependabot

### Testing

- [9-smoke-test-webapp.yml](9-smoke-test-webapp.yml): Runs smoke tests against deployed Web App
- [11-auto-test-pipeline.yml](11-auto-test-pipeline.yml): Runs automated tests (unit, integration, UI tests)

---

## 2. Deploy Environments

These Azure DevOps YML files are designed to run as multi-stage environment deployments (DEV/QA/PROD). Each Azure DevOps environment can have permissions and approvals defined. For example, DEV can publish on change, while QA and PROD can require approval before changes are made.

---

## 3. Setup Steps

- [Create Azure DevOps Service Connections](https://docs.luppes.com/CreateServiceConnections/)
- [Create Azure DevOps Environments](https://docs.luppes.com/CreateDevOpsEnvironments/)
- Create Azure DevOps Variable Groups (see next section; variables are unique to this project)
- [Create Azure DevOps Pipeline(s)](https://docs.luppes.com/CreateNewPipeline/)
- Run one deployment pipeline (for example, [2.1-bicep-build-deploy-webapp.yml](2.1-bicep-build-deploy-webapp.yml))

---

## 4. Required Variable Group: Dadabase.Demo

To create this variable group, customize and run this command in Azure Cloud Shell.

You can also define these variables in the Azure DevOps portal per pipeline, but a variable group is more repeatable and scriptable.

```bash
az login

az pipelines variable-group create \
  --organization=https://dev.azure.com/<yourAzDOOrg>/ \
  --project='<yourAzDOProject>' \
  --name Dadabase.Demo \
  --variables \
      APP_NAME='full-dadabase' \
      RESOURCE_GROUP_LOCATION='centralus' \
      RESOURCE_GROUP_PREFIX='rg-dadabase' \
      INSTANCE_NUMBER='1' \
      API_KEY='somesecretstring' \
      ADMIN_USER_LIST='user1@domain.com,user2@domain.com' \
      AZURE_TENANT_ID='yourTenantId' \
      AZURE_SUBSCRIPTION_ID='yourSubscriptionId' \
      AZURE_CLIENT_ID='yourClientId' \
      OPENAI_CHAT_DEPLOYMENTNAME='gpt-5-mini' \
      OPENAI_CHAT_MAXTOKENS='300' \
      OPENAI_CHAT_TEMPERATURE='0.7' \
      OPENAI_CHAT_TOPP='0.95' \
      OPENAI_IMAGE_DEPLOYMENTNAME='gpt-image-1.5' \
      OPENAI_IMAGE_ENDPOINT='https://<yourendpoint>.openai.azure.com/' \
      OPENAI_CHAT_ENDPOINT='https://<yourendpoint>.cognitiveservices.azure.com/' \
      OPENAI_IMAGE_APIKEY='yourkey' \
      OPENAI_CHAT_APIKEY='yourkey' \
      SQL_SERVER_NAME_PREFIX='your-dadabase-server-prefix' \
      SQL_DATABASE_NAME='DadABase' \
      SQLADMIN_LOGIN_USERID='youruser@yourdomain.com' \
      SQLADMIN_LOGIN_USERSID='yoursid' \
      SQLADMIN_LOGIN_TENANTID='yourtennant' \
      LOGIN_CLIENTID='yourADClientId' \
      LOGIN_DOMAIN='<yourdomain>.onmicrosoft.com' \
      LOGIN_INSTANCEENDPOINT='https://login.microsoftonline.com/' \
      LOGIN_TENANTID='yourTenantId' \
      KEYVAULT_OWNER_USERID='yourAccountSid' \
      EXISTING_SERVICEPLAN_NAME='' \
      EXISTING_SERVICEPLAN_RESOURCE_GROUP_NAME='' \
      EXISTING_SQLSERVER_NAME='' \
      EXISTING_SQLSERVER_RESOURCE_GROUP_NAME=''
```
