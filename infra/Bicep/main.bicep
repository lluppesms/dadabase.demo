// --------------------------------------------------------------------------------
// Main Bicep file that creates all of the Azure Resources for one environment
// --------------------------------------------------------------------------------
// To deploy this Bicep manually:
// 	 az login
//   az account set --subscription <subscriptionId>
//   az deployment group create -n "manual-$(Get-Date -Format 'yyyyMMdd-HHmmss')" --resource-group rg_dadabase_web_full --template-file 'main.bicep' --parameters appName=xxx-dad-full environmentCode=dev adminUserId=xxxxxxxx-xxxx-xxxx
// --------------------------------------------------------------------------------
param appName string = ''
param environmentCode string = 'azd'
param location string = resourceGroup().location
param instanceNumber string = '1'

param servicePlanName string = ''
param servicePlanResourceGroupName string = '' // if using an existing service plan in a different resource group

param webAppKind string = 'linux' // 'linux' or 'windows'
param webSiteSku string = 'B1'
param webStorageSku string = 'Standard_LRS'
param webApiKey string = ''

param functionStorageSku string = 'Standard_LRS'
param environmentSpecificFunctionName string = ''

param sqlDatabaseName string = 'dadabase'
@allowed(['Basic','Standard','Premium','BusinessCritical','GeneralPurpose'])
param sqlSkuTier string = 'GeneralPurpose'
param sqlSkuFamily string = 'Gen5'
param sqlSkuName string = 'GP_S_Gen5'
param sqlAdminLoginUserId string = ''
param sqlAdminLoginUserSid string = ''
param sqlAdminLoginTenantId string = ''
param sqlAdminUser string = ''
@secure()
param sqlAdminPassword string = ''

param adInstance string = environment().authentication.loginEndpoint // 'https://login.microsoftonline.com/'
param adDomain string = ''
param adTenantId string = ''
param adClientId string = ''
param adCallbackPath string = '/signin-oidc'

param appDataSource string = 'JSON'
param appSwaggerEnabled string = 'true'

param azureOpenAIChatEndpoint string = ''
param azureOpenAIChatDeploymentName string = ''
param azureOpenAIChatApiKey string = ''
param azureOpenAIChatMaxTokens string = ''
param azureOpenAIChatTemperature string = ''
param azureOpenAIChatTopP string = ''
param azureOpenAIImageEndpoint string = ''
param azureOpenAIImageDeploymentName string = ''
param azureOpenAIImageApiKey string = ''

@description('Add Role Assignments for the user assigned identity?')
param addRoleAssignments bool = true

@description('Add this Admin User Id to KeyVault Access')
param adminUserId string = ''

// calculated variables disguised as parameters
param runDateTime string = utcNow()

// --------------------------------------------------------------------------------
var deploymentSuffix = '-${runDateTime}'
var commonTags = {         
  LastDeployed: runDateTime
  Application: appName
  Environment: environmentCode
}
var resourceGroupName = resourceGroup().name
// var resourceToken = toLower(uniqueString(resourceGroup().id, location))

// --------------------------------------------------------------------------------
module resourceNames 'resourcenames.bicep' = {
  name: 'resourcenames${deploymentSuffix}'
  params: {
    appName: appName
    environmentCode: environmentCode
    environmentSpecificFunctionName: environmentSpecificFunctionName
    instanceNumber: instanceNumber
  }
}
// --------------------------------------------------------------------------------
module logAnalyticsWorkspaceModule './modules/monitor/loganalyticsworkspace.bicep' = {
  name: 'logAnalytics${deploymentSuffix}'
  params: {
    logAnalyticsWorkspaceName: resourceNames.outputs.logAnalyticsWorkspaceName
    location: location
    commonTags: commonTags
  }
}

// --------------------------------------------------------------------------------
module storageModule './modules/storage/storageaccount.bicep' = {
  name: 'storage${deploymentSuffix}'
  params: {
    storageSku: webStorageSku
    storageAccountName: resourceNames.outputs.storageAccountName
    location: location
    commonTags: commonTags
  }
}

module functionStorageModule './modules/storage/storageaccount.bicep' = {
  name: 'functionstorage${deploymentSuffix}'
  params: {
    storageSku: functionStorageSku
    storageAccountName: resourceNames.outputs.functionApp.storageName
    location: location
    commonTags: commonTags
    allowNetworkAccess: 'Allow'
    publicNetworkAccess: 'Enabled'
    addSecurityControlIgnoreTag: true
  }
}

// --------------------------------------------------------------------------------
module sqlDbModule './modules/database/sqlserver.bicep' = {
  name: 'sql-server${deploymentSuffix}'
  params: {
    sqlServerName: resourceNames.outputs.sqlServerName
    sqlDBName: sqlDatabaseName
    existingSqlServerName: ''
    existingSqlServerResourceGroupName: ''
    sqlSkuTier: sqlSkuTier
    sqlSkuName: sqlSkuName
    sqlSkuFamily: sqlSkuFamily
    mincores: 1
    autopause: 60
    location: location
    commonTags: commonTags
    adAdminUserId: sqlAdminLoginUserId
    adAdminUserSid: sqlAdminLoginUserSid
    adAdminTenantId: sqlAdminLoginTenantId
    userAssignedIdentityResourceId: identity.outputs.managedIdentityId
    sqlAdminUser:sqlAdminUser
    sqlAdminPassword: sqlAdminPassword
    workspaceId: logAnalyticsWorkspaceModule.outputs.id
    addSecurityControlIgnoreTag: true
  }
}


// --------------------------------------------------------------------------------
module identity './modules/iam/identity.bicep' = {
  name: 'appIdentity${deploymentSuffix}'
  params: {
    identityName: resourceNames.outputs.userAssignedIdentityName
    location: location
  }
}
module appRoleAssignments './modules/iam/roleassignments.bicep' = if (addRoleAssignments) {
  name: 'appRoleAssignments${deploymentSuffix}'
  params: {
    identityPrincipalId: identity.outputs.managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
    storageAccountName: functionStorageModule.outputs.name
    keyVaultName:  keyVaultModule.outputs.name
  }
}
// module adminRoleAssignments './security/roleassignments.bicep' = if (addRoleAssignments) {
//   name: 'userRoleAssignments${deploymentSuffix}'
//   params: {
//     identityPrincipalId: adminUserId
//     principalType: 'User'
//     storageAccountName: functionStorageModule.outputs.name
//     keyVaultName:  keyVaultModule.outputs.name
//   }
// }


// --------------------------------------------------------------------------------
module keyVaultModule './modules/security/keyvault.bicep' = {
  name: 'keyVault${deploymentSuffix}'
  params: {
    keyVaultName: resourceNames.outputs.keyVaultName
    location: location
    commonTags: commonTags
    keyVaultOwnerUserId: adminUserId
    adminUserObjectIds: [ identity.outputs.managedIdentityPrincipalId ]
    applicationUserObjectIds: [ webSiteModule.outputs.webappAppPrincipalId ]
    workspaceId: logAnalyticsWorkspaceModule.outputs.id
    publicNetworkAccess: 'Enabled'
    allowNetworkAccess: 'Allow'
    useRBAC: true
  }
}

module keyVaultStorageSecret './modules/security/keyvaultsecretstorageconnection.bicep' = {
  name: 'keyVaultStorageSecret${deploymentSuffix}'
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'azurefilesconnectionstring'
    storageAccountName: functionStorageModule.outputs.name
  }
}

// --------------------------------------------------------------------------------
module appServicePlanModule './modules/webapp/websiteserviceplan.bicep' = {
  name: 'appService${deploymentSuffix}'
  params: {
    location: location
    commonTags: commonTags
    sku: webSiteSku
    environmentCode: environmentCode
    appServicePlanName: servicePlanName == '' ? resourceNames.outputs.webSiteAppServicePlanName : servicePlanName
    existingServicePlanName: servicePlanName
    existingServicePlanResourceGroupName: servicePlanResourceGroupName
    webAppKind: webAppKind
  }
}

module webSiteModule './modules/webapp/website.bicep' = {
  name: 'webSite${deploymentSuffix}'
  params: {
    webSiteName: resourceNames.outputs.webSiteName
    location: location
    appInsightsLocation: location
    commonTags: commonTags
    environmentCode: environmentCode
    webAppKind: webAppKind
    managedIdentityId: identity.outputs.managedIdentityId
    managedIdentityPrincipalId: identity.outputs.managedIdentityPrincipalId
    workspaceId: logAnalyticsWorkspaceModule.outputs.id
    appServicePlanName: appServicePlanModule.outputs.name
    appServicePlanResourceGroupName: appServicePlanModule.outputs.resourceGroupName
  }
}

// In a Linux app service, any nested JSON app key like AppSettings:MyKey needs to be 
// configured in App Service as AppSettings__MyKey for the key name. 
// In other words, any : should be replaced by __ (double underscore).
// NOTE: See https://learn.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal  
module webSiteAppSettingsModule './modules/webapp/websiteappsettings.bicep' = {
  name: 'webSiteAppSettings${deploymentSuffix}'
  params: {
    webAppName: webSiteModule.outputs.name
    appInsightsKey: webSiteModule.outputs.appInsightsKey
    customAppSettings: {
      AppSettings__AppInsights_InstrumentationKey: webSiteModule.outputs.appInsightsKey
      APPLICATIONINSIGHTS_CONNECTION_STRING: webSiteModule.outputs.appInsightsConnectionString
      AppSettings__DefaultConnection: sqlDbModule.outputs.identityConnectionString
      AppSettings__ProjectEntities: sqlDbModule.outputs.identityConnectionString
      AppSettings__EnvironmentName: environmentCode
      AppSettings__EnableSwagger: appSwaggerEnabled
      AppSettings__DataSource: appDataSource
      AppSettings__ApiKey: webApiKey
      AppSettings__AzureOpenAI__Chat__Endpoint: azureOpenAIChatEndpoint
      AppSettings__AzureOpenAI__Chat__DeploymentName: azureOpenAIChatDeploymentName
      AppSettings__AzureOpenAI__Chat__ApiKey: azureOpenAIChatApiKey
      AppSettings__AzureOpenAI__Chat__MaxTokens: azureOpenAIChatMaxTokens
      AppSettings__AzureOpenAI__Chat__Temperature: azureOpenAIChatTemperature
      AppSettings__AzureOpenAI__Chat__TopP: azureOpenAIChatTopP
      AppSettings__AzureOpenAI__Image__Endpoint: azureOpenAIImageEndpoint
      AppSettings__AzureOpenAI__Image__DeploymentName: azureOpenAIImageDeploymentName
      AppSettings__AzureOpenAI__Image__ApiKey: azureOpenAIImageApiKey
      AzureAD__Instance: adInstance
      AzureAD__Domain: adDomain
      AzureAD__TenantId: adTenantId
      AzureAD__ClientId: adClientId
      AzureAD__CallbackPath: adCallbackPath
    }
  }
}

// --------------------------------------------------------------------------------
// Function Flex Consumption - Shared Infrastructure (App Service Plan, App Insights, Storage)
// This is deployed once and shared by all function apps
// --------------------------------------------------------------------------------
module flexFunctionResourcesModule 'modules/functions/functionresources.bicep' = {
  name: 'flexFunctionResources${deploymentSuffix}'
  params: {
    functionInsightsName: resourceNames.outputs.functionApp.insightsName
    functionStorageAccountName: resourceNames.outputs.functionApp.deploymentStorageContainerName
    location: location
    commonTags: commonTags
    workspaceId: logAnalyticsWorkspaceModule.outputs.id
  }
}

//--------------------------------------------------------------------------------
module functionModule './modules/function/functionflex.bicep' = {
  name: 'function${deploymentSuffix}'
  params: {
    functionAppName: resourceNames.outputs.functionApp.name
    functionAppServicePlanName: resourceNames.outputs.functionApp.servicePlanName
    deploymentStorageContainerName: resourceNames.outputs.functionApp.deploymentStorageContainerName
    functionInsightsName: flexFunctionResourcesModule.outputs.appInsightsName
    functionStorageAccountName: flexFunctionResourcesModule.outputs.storageAccountName
    // appInsightsName: flexFunctionResourcesModule.outputs.appInsightsName
    // storageAccountName: flexFunctionResourcesModule.outputs.storageAccountName
    addRoleAssignments: addRoleAssignments
    keyVaultName: keyVaultModule.outputs.name
    location: location
    commonTags: commonTags
    deploymentSuffix: deploymentSuffix
    customAppSettings: {
      OpenApi__HideSwaggerUI: 'false'
      OpenApi__HideDocument: 'false'
      OpenApi__DocTitle: 'Isolated .NET10 Functions Demo APIs'
      OpenApi__DocDescription: 'This repo is an example of how to use Isolated .NET10 Azure Functions'
      // OpenAI settings
      OpenAI__Chat__DeploymentName: azureOpenAIChatDeploymentName
      OpenAI__Chat__Endpoint: azureOpenAIChatEndpoint
      // OpenAI__Chat__ApiKey: '@Microsoft.KeyVault(VaultName=${keyVaultModule.outputs.name};SecretName=${keyVaultSecretOpenAI.outputs.secretName})'
      OpenAI__Chat__ModelName: azureOpenAIImageDeploymentName
      OpenAI__Chat__Temperature: azureOpenAIChatTemperature
    }
  }
}

// --------------------------------------------------------------------------------
output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP_NAME string = resourceGroupName
output WEB_HOST_NAME string = webSiteModule.outputs.hostName
output FUNCTION_HOST_NAME string = functionModule.outputs.hostname

