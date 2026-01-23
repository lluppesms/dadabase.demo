# Azure DevOps Deployment Template Notes

## 1. Azure DevOps Template Definitions

Typically, you would want to set up either Option (a), or Option (b) AND Option (c), but not all three jobs.

- **[infra-and-webapp-pipeline.yml](infra-and-webapp-pipeline.yml):** Deploys the main.bicep template, builds the website code, then deploys the website to the Azure App Service
- **[infra-only-pipeline.yml](infra-only-pipeline.yml):** Deploys the main.bicep template and does nothing else
- **[build-webapp-only-pipeline.yml](build-webapp-only-pipeline.yml):** Builds the website and then deploys the website to the Azure App Service
- **[deploy-webapp-only-pipeline.yml](deploy-webapp-only-pipeline.yml):** Deploys a previously built website to the Azure App Service
- **[scan-pipeline.yml](scan-pipeline.yml):** Performs a periodic security scan

---

## 2. Deploy Environments

These Azure DevOps YML files were designed to run as multi-stage environment deploys (i.e. DEV/QA/PROD). Each Azure DevOps environments can have permissions and approvals defined. For example, DEV can be published upon change, and QA/PROD environments can require an approval before any changes are made.

---

## 3. Setup Steps

- [Create Azure DevOps Service Connections](https://docs.luppes.com/CreateServiceConnections/)

- [Create Azure DevOps Environments](https://docs.luppes.com/CreateDevOpsEnvironments/)

- Create Azure DevOps Variable Groups - see next step in this document (the variables are unique to this project)

- [Create Azure DevOps Pipeline(s)](https://docs.luppes.com/CreateNewPipeline/)

- Run the infra-and-website-pipeline.yml pipeline to deploy the project to an Azure subscription.

---

## 4. These pipelines needs a variable group named "DadABase.Web"

To create this variable groups, customize and run this command in the Azure Cloud Shell.

Alternatively, you could define these variables in the Azure DevOps Portal on each pipeline, but a variable group is a more repeatable and scriptable way to do it.

``` bash
   az login

   az pipelines variable-group create 
     --organization=https://dev.azure.com/<yourAzDOOrg>/ 
     --project='<yourAzDOProject>' 
     --name DadABaseDemo 
     --variables 
         APP_NAME='full-dadabase'
         RESOURCE_GROUP_LOCATION='centralus'
         RESOURCE_GROUP_PREFIX='rg-dadabase' 
         INSTANCE_NUMBER='1'
         API_KEY='somesecretstring'
         OPENAI_CHAT_DEPLOYMENTNAME='gpt-5-mini'
         OPENAI_CHAT_MAXTOKENS='300'
         OPENAI_CHAT_TEMPERATURE='0.7'
         OPENAI_CHAT_TOPP='0.95'
         OPENAI_IMAGE_DEPLOYMENTNAME='gpt-image-1.5'
         OPENAI_IMAGE_ENDPOINT='https://<yourendpoint>.openai.azure.com/'
         OPENAI_CHAT_ENDPOINT='https://<yourendpoint>.cognitiveservices.azure.com/'
         OPENAI_IMAGE_APIKEY='yourkey'
         OPENAI_CHAT_APIKEY='yourkey'
         SQL_SERVER_NAME_PREFIX='your-dadabase-server'
         SQL_DATABASE_NAME='DadABase'
         SQLADMIN_LOGIN_USERID='youruser@yourdomain.com'
         SQLADMIN_LOGIN_USERSID='yoursid'
         SQLADMIN_LOGIN_TENANTID='yourtennant'
         ADMIN_USER_LIST='user1@domain.com,user2@domain.com'
         LOGIN_CLIENTID='<yourADClientId>'
         LOGIN_DOMAIN='<yourdomain>.onmicrosoft.com'
         LOGIN_INSTANCEENDPOINT='https://login.microsoftonline.com/'
         LOGIN_TENANTID='<yourTenantId>'
         AZURE_TENANT_ID='b2073a82-d60f-48a0-bda5-722a163c88ad'
         AZURE_SUBSCRIPTION_ID='6a97aa36-e4fc-4db0-baa6-4cffaba22e97'
         AZURE_CLIENT_ID='a99cb515-1f75-4af9-8a28-94761688d02b'
         KEYVAULT_OWNER_USERID='<yourAccountSid>'
         EXISTING_SERVICEPLAN_NAME=''
         EXISTING_SERVICEPLAN_RESOURCE_GROUP_NAME=''
         EXISTING_SQLSERVER_NAME=''
         EXISTING_SQLSERVER_RESOURCE_GROUP_NAME=''
```
