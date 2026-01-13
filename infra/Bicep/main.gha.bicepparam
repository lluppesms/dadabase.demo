// ----------------------------------------------------------------------------------------------------
// GitHub Workflow - Bicep Parameter File
// ----------------------------------------------------------------------------------------------------
using './main.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{ENVCODE}#'
param location = '#{RESOURCE_GROUP_LOCATION}#'
param instanceNumber = '#{INSTANCE_NUMBER}#'

param adInstance = '#{LOGIN_INSTANCEENDPOINT}#'
param adDomain = '#{LOGIN_DOMAIN}#'
param adTenantId = '#{LOGIN_TENANTID}#'
param adClientId = '#{LOGIN_CLIENTID}#'
param webApiKey = '#{WEB_API_KEY}#'
param servicePlanName = '#{EXISTING_SERVICEPLAN_NAME}#'
param servicePlanResourceGroupName = '#{EXISTING_SERVICEPLAN_RESOURCE_GROUP_NAME}#'
param webAppKind = 'linux' // 'linux' or 'windows'

param sqlAdminLoginUserId = '#{SQLADMIN_LOGIN_USERID}#'
param sqlAdminLoginUserSid = '#{SQLADMIN_LOGIN_USERSID}#'
param sqlAdminLoginTenantId = '#{SQLADMIN_LOGIN_TENANTID}#'

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
