# Azure DevOps Deployment Template Notes

## 1. Azure DevOps Pipeline Definitions

The following pipelines are available for various deployment scenarios:

**Infrastructure & Application Deployment:**
- **[1-deploy-bicep.yml](1-deploy-bicep.yml):** Deploys the main.bicep template to Azure (infrastructure only)
- **[2.1-bicep-build-deploy-webapp.yml](2.1-bicep-build-deploy-webapp.yml):** Deploys Bicep infrastructure and builds/deploys the Blazor Web App to Azure App Service
- **[2.2-bicep-build-deploy-containerapp.yml](2.2-bicep-build-deploy-containerapp.yml):** Deploys Bicep infrastructure and builds/deploys a containerized application to Azure Container Apps
- **[3-bicep-build-deploy-function.yml](3-bicep-build-deploy-function.yml):** Deploys Bicep infrastructure and builds/deploys Azure Functions

**Database & Schema:**
- **[4-build-deploy-dacpac.yml](4-build-deploy-dacpac.yml):** Builds and deploys the SQL database schema (DACPAC) to Azure SQL Database
- **[5-run-sql.yml](5-run-sql.yml):** Runs SQL scripts against an existing Azure SQL Database

**Application Deployment Only:**
- **[10-deploy-webapp-only-pipeline.yml](10-deploy-webapp-only-pipeline.yml):** Deploys a previously built Web App to Azure App Service without infrastructure changes

**Code Quality & Security:**
- **[6-pr-scan-build.yml](6-pr-scan-build.yml):** Scans and builds code on pull requests
- **[7-scan-code.yml](7-scan-code.yml):** Performs periodic security scans (GitHub Advanced Security & Microsoft Secure DevOps)
- **[8-dependabot.yml](dependabot.yml):** Automated dependency updates via Dependabot

**Testing:**
- **[9-smoke-test-webapp.yml](9-smoke-test-webapp.yml):** Runs smoke tests against deployed Web App
- **[11-auto-test-pipeline.yml](11-auto-test-pipeline.yml):** Runs automated tests (unit, integration, UI tests)

---

## 2. Deploy Environments

These Azure DevOps YML files were designed to run as multi-stage environment deploys (i.e. DEV/QA/PROD). Each Azure DevOps environments can have permissions and approvals defined. For example, DEV can be published upon change, and QA/PROD environments can require an approval before any changes are made.

---

## 3. Setup Steps

- [Create Azure DevOps Service Connections](https://docs.luppes.com/CreateServiceConnections/)

- [Create Azure DevOps Environments](https://docs.luppes.com/CreateDevOpsEnvironments/)

- Create Azure DevOps Variable Groups - see next step in this document (the variables are unique to this project)

- [Create Azure DevOps Pipeline(s)](https://docs.luppes.com/CreateNewPipeline/)

- Run one of the deployment pipelines (e.g., [2.1-bicep-build-deploy-webapp.yml](2.1-bicep-build-deploy-webapp.yml)) to deploy the project to an Azure subscription.

---

## 4. These pipelines need a variable group named "Dadabase.Demo"

To create these variable groups, customize and run this command in the Azure Cloud Shell.

Alternatively, you could define these variables in the Azure DevOps Portal on each pipeline, but a variable group is a more repeatable and scriptable way to do it.

``` bash
   az login

   az pipelines variable-group create 
     --organization=https://dev.azure.com/<yourAzDOOrg>/ 
     --project='<yourAzDOProject>' 
     --name Dadabase.Demo 
     --variables 
         APP_NAME='full-dadabase'
         RESOURCE_GROUP_LOCATION='centralus'
         RESOURCE_GROUP_PREFIX='rg-dadabase' 
         INSTANCE_NUMBER='1'
         API_KEY='somesecretstring'
         ADMIN_USER_LIST='user1@domain.com,user2@domain.com'

         AZURE_TENANT_ID='yourTenantId'
         AZURE_SUBSCRIPTION_ID='yourSubscriptionId'
         AZURE_CLIENT_ID='yourClientId'

         OPENAI_CHAT_DEPLOYMENTNAME='gpt-5-mini'
         OPENAI_CHAT_MAXTOKENS='300'
         OPENAI_CHAT_TEMPERATURE='0.7'
         OPENAI_CHAT_TOPP='0.95'
         OPENAI_IMAGE_DEPLOYMENTNAME='gpt-image-1.5'
         OPENAI_IMAGE_ENDPOINT='https://<yourendpoint>.openai.azure.com/'
         OPENAI_CHAT_ENDPOINT='https://<yourendpoint>.cognitiveservices.azure.com/'
         OPENAI_IMAGE_APIKEY='yourkey'
         OPENAI_CHAT_APIKEY='yourkey'

         SQL_SERVER_NAME_PREFIX='your-dadabase-server-prefix'
         SQL_DATABASE_NAME='DadABase'
         SQLADMIN_LOGIN_USERID='youruser@yourdomain.com'
         SQLADMIN_LOGIN_USERSID='yoursid'
         SQLADMIN_LOGIN_TENANTID='yourtennant'

         LOGIN_CLIENTID='yourADClientId'
         LOGIN_DOMAIN='<yourdomain>.onmicrosoft.com'
         LOGIN_INSTANCEENDPOINT='https://login.microsoftonline.com/'
         LOGIN_TENANTID='yourTenantId'

         KEYVAULT_OWNER_USERID='yourAccountSid'

         EXISTING_SERVICEPLAN_NAME=''
         EXISTING_SERVICEPLAN_RESOURCE_GROUP_NAME=''
         EXISTING_SQLSERVER_NAME=''
         EXISTING_SQLSERVER_RESOURCE_GROUP_NAME=''
```
