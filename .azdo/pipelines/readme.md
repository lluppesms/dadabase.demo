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

## 4. Troubleshooting: "Already Assigned" – Finding Existing GitHub ↔ Azure DevOps Connections

When you try to link a GitHub organization or repository to Azure DevOps (via **Azure Boards** or **Azure Pipelines** GitHub Apps) and you see a message such as *"already assigned"* or *"already connected"*, it means the GitHub organization is already authorized to a **different Azure DevOps organization**.

Each GitHub organization (or personal account) can only be linked to **one Azure DevOps organization** at a time through a given GitHub App installation.

### Where to Look

#### 1. GitHub – Installed GitHub Apps (Org level)

Navigate to your GitHub organization (or personal account) settings to see which apps are installed and which Azure DevOps organization they are pointing to:

| Account type | URL |
|---|---|
| **Organization** | `https://github.com/organizations/<YOUR_ORG>/settings/installations` |
| **Personal account** | `https://github.com/settings/installations` |

Look for **Azure Boards** and/or **Azure Pipelines** in the list and click **Configure** to see which repositories are authorized and which Azure DevOps organization they connect to.

#### 2. Azure DevOps – GitHub Connections (Org level)

In every Azure DevOps organization, administrators can see which GitHub repositories and organizations are connected:

```
https://dev.azure.com/<YOUR_AZDO_ORG>/_settings/githubconnection
```

If another DevOps organization already holds the connection, you will need to check each AzDO org you have access to.

#### 3. Azure DevOps – GitHub Connections (Project level – Azure Boards)

Within an individual Azure DevOps project, the Azure Boards GitHub connection is visible under:

```
https://dev.azure.com/<YOUR_AZDO_ORG>/<YOUR_PROJECT>/_settings/boards-external-integration
```

### How to Resolve the Conflict

1. Identify the Azure DevOps organization currently holding the GitHub App authorization (follow the steps above).
2. **Option A – Reuse the existing connection:** Create or use the existing Azure DevOps project in that organization and import the pipelines there.
3. **Option B – Move the connection:** In the **GitHub – Installed GitHub Apps** settings (section 1 above), revoke or uninstall the existing Azure Boards / Azure Pipelines app from the GitHub organization, then reinstall and authorize it against your desired Azure DevOps organization.
   > ⚠️ Revoking an existing connection will disconnect any pipelines or board integrations that rely on it. Notify any other teams using the connection before removing it.
4. **Option C – Contact your admin:** If you do not have admin access on the GitHub organization or the other Azure DevOps organization, ask an administrator to either reassign or create a separate GitHub App installation for your project.

---

## 5. These pipelines needs a variable group named "DadABaseDemo"

To create this variable groups, customize and run this command in the Azure Cloud Shell.

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
         AZURE_TENANT_ID='<yourTenantId>'
         AZURE_SUBSCRIPTION_ID='<yourSubscriptionId>'
         AZURE_CLIENT_ID='<yourClientId>'
         KEYVAULT_OWNER_USERID='<yourAccountSid>'
         EXISTING_SERVICEPLAN_NAME=''
         EXISTING_SERVICEPLAN_RESOURCE_GROUP_NAME=''
         EXISTING_SQLSERVER_NAME=''
         EXISTING_SQLSERVER_RESOURCE_GROUP_NAME=''
```
