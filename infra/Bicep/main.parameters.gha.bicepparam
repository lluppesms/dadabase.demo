// ----------------------------------------------------------------------------------------------------
// GitHub Workflow - Bicep Parameter File
// ----------------------------------------------------------------------------------------------------

using 'main.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{envCode}#'

param adInstance = '#{LOGIN_INSTANCEENDPOINT}#'
param adDomain = '#{LOGIN_DOMAIN}#'
param adTenantId = '#{LOGIN_TENANTID}#'
param adClientId = '#{LOGIN_CLIENTID}#'
param apiKey = '#{API_KEY}#'
param location = '#{RESOURCEGROUP_LOCATION}#'
param servicePlanName = '#{SERVICEPLAN_NAME}#'
param servicePlanResourceGroupName = '#{SERVICEPLAN_RESOURCEGROUP_NAME}#'
param webAppKind = 'linux' // 'linux' or 'windows'

param azureOpenAIChatEndpoint = '#{OPENAI_CHAT_ENDPOINT}#'
param azureOpenAIChatDeploymentName = '#{OPENAI_CHAT_DEPLOYMENTNAME}#'
param azureOpenAIChatApiKey = '#{OPENAI_CHAT_APIKEY}#'
param azureOpenAIChatMaxTokens = '#{OPENAI_CHAT_MAXTOKENS}#'
param azureOpenAIChatTemperature = '#{OPENAI_CHAT_TEMPERATURE}#'
param azureOpenAIChatTopP = '#{OPENAI_CHAT_TOPP}#'
param azureOpenAIImageEndpoint = '#{OPENAI_IMAGE_ENDPOINT}#'
param azureOpenAIImageDeploymentName = '#{OPENAI_IMAGE_DEPLOYMENTNAME}#'
param azureOpenAIImageApiKey = '#{OPENAI_IMAGE_APIKEY}#'
