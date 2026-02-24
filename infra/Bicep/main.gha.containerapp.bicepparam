// ----------------------------------------------------------------------------------------------------
// GitHub Workflow - Bicep Parameter File for Container Apps Deployment
// ----------------------------------------------------------------------------------------------------
using './main.bicep'

// Deployment configuration
param deploymentType = 'containerapp'
param containerImage = '#{CONTAINER_IMAGE}#'
param containerRegistrySku = 'Basic'
// param pipelineServicePrincipalObjectId = '#{PIPELINE_SERVICE_PRINCIPAL_OBJECT_ID}#'

param appName = '#{APP_NAME}#'
param environmentCode = '#{ENVCODE}#'
param location = '#{RESOURCE_GROUP_LOCATION}#'
param instanceNumber = '#{INSTANCE_NUMBER}#'

param adminUserList = '#{ADMIN_USER_LIST}#'
param adInstance = '#{LOGIN_INSTANCEENDPOINT}#'
param adDomain = '#{LOGIN_DOMAIN}#'
param adTenantId = '#{LOGIN_TENANTID}#'
param adClientId = '#{LOGIN_CLIENTID}#'
param webApiKey = '#{WEB_API_KEY}#'

// App Service parameters (not used for Container Apps but required by schema)
param servicePlanName = ''
param servicePlanResourceGroupName = ''
param webAppKind = 'linux'
param webSiteSku = 'B1'
param webStorageSku = 'Standard_LRS'

param sqlAdminLoginUserId = '#{SQLADMIN_LOGIN_USERID}#'
param sqlAdminLoginUserSid = '#{SQLADMIN_LOGIN_USERSID}#'
param sqlAdminLoginTenantId = '#{SQLADMIN_LOGIN_TENANTID}#'

param sqlDatabaseName = '#{SQL_DATABASE_NAME}#'
param existingSqlServerName = '#{EXISTING_SQLSERVER_NAME}#'
param existingSqlServerResourceGroupName = '#{EXISTING_SQLSERVER_RESOURCE_GROUP_NAME}#'

param adminUserId = '#{KEYVAULT_OWNER_USERID}#'

param azureOpenAIChatEndpoint = '#{OPENAI_CHAT_ENDPOINT}#'
param azureOpenAIChatDeploymentName = '#{OPENAI_CHAT_DEPLOYMENTNAME}#'
param azureOpenAIChatApiKey = '#{OPENAI_CHAT_APIKEY}#'
param azureOpenAIChatMaxTokens = '#{OPENAI_CHAT_MAXTOKENS}#'
param azureOpenAIChatTemperature = '#{OPENAI_CHAT_TEMPERATURE}#'
param azureOpenAIChatTopP = '#{OPENAI_CHAT_TOPP}#'
param azureOpenAIImageEndpoint = '#{OPENAI_IMAGE_ENDPOINT}#'
param azureOpenAIImageDeploymentName = '#{OPENAI_IMAGE_DEPLOYMENTNAME}#'
param azureOpenAIImageApiKey = '#{OPENAI_IMAGE_APIKEY}#'
