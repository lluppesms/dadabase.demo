// ----------------------------------------------------------------------------------------------------
// Azure DevOps Pipeline - Bicep Parameter File
// ----------------------------------------------------------------------------------------------------

using 'main.bicep'

param appName = '#{appName}#'
param environmentCode = '#{environmentNameLower}#'

param adInstance = '#{adInstance}#'
param adDomain = '#{adDomain}#'
param adTenantId = '#{adTenantId}#'
param adClientId = '#{adClientId}#'
param webApiKey = '#{webApiKey}#'
param location = '#{location}#'
param servicePlanName = '#{servicePlanName}#'
param servicePlanResourceGroupName = '#{servicePlanResourceGroupName}#'
param webAppKind = 'linux' // 'linux' or 'windows'

param azureOpenAIChatEndpoint = '#{azureOpenAIChatEndpoint}#'
param azureOpenAIChatDeploymentName = '#{azureOpenAIChatDeploymentName}#'
param azureOpenAIChatApiKey = '#{azureOpenAIChatApiKey}#'
param azureOpenAIChatMaxTokens = '#{azureOpenAIChatMaxTokens}#'
param azureOpenAIChatTemperature = '#{azureOpenAIChatTemperature}#'
param azureOpenAIChatTopP = '#{azureOpenAIChatTopP}#'
param azureOpenAIImageEndpoint = '#{azureOpenAIImageEndpoint}#'
param azureOpenAIImageDeploymentName = '#{azureOpenAIImageDeploymentName}#'
param azureOpenAIImageApiKey = '#{azureOpenAIImageApiKey}#'

