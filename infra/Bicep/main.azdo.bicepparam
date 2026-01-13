// ----------------------------------------------------------------------------------------------------
// Azure DevOps Pipeline - Bicep Parameter File
// ----------------------------------------------------------------------------------------------------
using './main.bicep'

param appName = '#{appName}#'
param environmentCode = '#{environmentNameLower}#'
param location = '#{location}#'

param adInstance = '#{adInstance}#'
param adDomain = '#{adDomain}#'
param adTenantId = '#{adTenantId}#'
param adClientId = '#{adClientId}#'
param webApiKey = '#{webApiKey}#'
param servicePlanName = '#{servicePlanName}#'
param servicePlanResourceGroupName = '#{servicePlanResourceGroupName}#'
param webAppKind = 'linux' // 'linux' or 'windows'

param sqlAdminLoginUserId = '$#{sqlAdminLoginUserId}#'
param sqlAdminLoginUserSid = '$#{sqlAdminLoginUserSid}#'
param sqlAdminLoginTenantId = '$#{sqlAdminLoginTenantId}#'

param adminUserId = '#{keyvaultOwnerUserId}#'

param azureOpenAIChatEndpoint = '#{aiChatEndpoint}#'
param azureOpenAIChatDeploymentName = '#{aiChatDeploymentName}#'
param azureOpenAIChatApiKey = '#{aiChatApiKey}#'
param azureOpenAIChatMaxTokens = '#{aiChatMaxTokens}#'
param azureOpenAIChatTemperature = '#{aiChatTemperature}#'
param azureOpenAIChatTopP = '#{aiChatTopP}#'
param azureOpenAIImageEndpoint = '#{aiImageEndpoint}#'
param azureOpenAIImageDeploymentName = '#{aiImageDeploymentName}#'
param azureOpenAIImageApiKey = '#{aiImageApiKey}#'

