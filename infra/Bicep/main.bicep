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

param servicePlanName string = ''
param servicePlanResourceGroupName string = '' // if using an existing service plan in a different resource group

param webAppKind string = 'linux' // 'linux' or 'windows'
param webSiteSku string = 'B1'
param webStorageSku string = 'Standard_LRS'
param webApiKey string = ''

param functionStorageSku string = 'Standard_LRS'
param functionAppSku string = 'B1' //  'Y1'
param functionAppSkuFamily string = 'B' // 'Y'
param functionAppSkuTier string = 'Dynamic'
param environmentSpecificFunctionName string = ''

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
    functionStorageNameSuffix: 'func'
    dataStorageNameSuffix: 'data'
    environmentSpecificFunctionName: environmentSpecificFunctionName
  }
}
// --------------------------------------------------------------------------------
module logAnalyticsWorkspaceModule 'app/loganalyticsworkspace.bicep' = {
  name: 'logAnalytics${deploymentSuffix}'
  params: {
    logAnalyticsWorkspaceName: resourceNames.outputs.logAnalyticsWorkspaceName
    location: location
    commonTags: commonTags
  }
}

// --------------------------------------------------------------------------------
module storageModule 'app/storageaccount.bicep' = {
  name: 'storage${deploymentSuffix}'
  params: {
    storageSku: webStorageSku
    storageAccountName: resourceNames.outputs.storageAccountName
    location: location
    commonTags: commonTags
  }
}

module functionStorageModule 'app/storageaccount.bicep' = {
  name: 'functionstorage${deploymentSuffix}'
  params: {
    storageSku: functionStorageSku
    storageAccountName: resourceNames.outputs.functionStorageName
    location: location
    commonTags: commonTags
    allowNetworkAccess: 'Allow'
    publicNetworkAccess: 'Enabled'
    addSecurityControlIgnoreTag: true
  }
}
module functionFlexStorageModule 'app/storageaccount.bicep' = {
  name: 'functionFlexStorage${deploymentSuffix}'
  params: {
    storageSku: functionStorageSku
    storageAccountName: resourceNames.outputs.functionFlexStorageName
    location: location
    commonTags: commonTags
    allowNetworkAccess: 'Allow'
    publicNetworkAccess: 'Enabled'
    addSecurityControlIgnoreTag: true
  }
}


// --------------------------------------------------------------------------------
module identity './security/identity.bicep' = {
  name: 'appIdentity${deploymentSuffix}'
  params: {
    identityName: resourceNames.outputs.userAssignedIdentityName
    location: location
  }
}
module appRoleAssignments './security/roleassignments.bicep' = if (addRoleAssignments) {
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
module keyVaultModule './security/keyvault.bicep' = {
  name: 'keyVault${deploymentSuffix}'
  params: {
    keyVaultName: resourceNames.outputs.keyVaultName
    location: location
    commonTags: commonTags
    adminUserObjectIds: [ adminUserId ]
    applicationUserObjectIds: [ identity.outputs.managedIdentityPrincipalId ]
    workspaceId: logAnalyticsWorkspaceModule.outputs.id
    publicNetworkAccess: 'Enabled'
    allowNetworkAccess: 'Allow'
    useRBAC: true
  }
}

module keyVaultStorageSecret './security/keyvaultsecretstorageconnection.bicep' = {
  name: 'keyVaultStorageSecret${deploymentSuffix}'
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'azurefilesconnectionstring'
    storageAccountName: functionStorageModule.outputs.name
  }
}

// --------------------------------------------------------------------------------
module appServicePlanModule 'app/websiteserviceplan.bicep' = {
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

module webSiteModule 'app/website.bicep' = {
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
module webSiteAppSettingsModule 'app/websiteappsettings.bicep' = {
  name: 'webSiteAppSettings${deploymentSuffix}'
  params: {
    webAppName: webSiteModule.outputs.name
    appInsightsKey: webSiteModule.outputs.appInsightsKey
    customAppSettings: {
      AppSettings__AppInsights_InstrumentationKey: webSiteModule.outputs.appInsightsKey
      APPLICATIONINSIGHTS_CONNECTION_STRING: webSiteModule.outputs.appInsightsConnectionString
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

//--------------------------------------------------------------------------------
module functionModule 'app/functionapp.bicep' = {
  name: 'function${deploymentSuffix}'
  dependsOn: [ appRoleAssignments ]
  params: {
    functionAppName: resourceNames.outputs.functionAppName
    functionAppServicePlanName: appServicePlanModule.outputs.name
    functionInsightsName: resourceNames.outputs.functionInsightsName
    managedIdentityId: identity.outputs.managedIdentityId
    keyVaultName: keyVaultModule.outputs.name

    appInsightsLocation: location
    location: location
    commonTags: commonTags

    functionKind: 'functionapp,linux'
    functionAppSku: functionAppSku
    functionAppSkuFamily: functionAppSkuFamily
    functionAppSkuTier: functionAppSkuTier
    functionStorageAccountName: functionStorageModule.outputs.name
    workspaceId: logAnalyticsWorkspaceModule.outputs.id
  }
}

module functionAppSettingsModule 'app/functionappsettings.bicep' = {
  name: 'functionAppSettings${deploymentSuffix}'
  params: {
    functionAppName: functionModule.outputs.name
    functionStorageAccountName: functionModule.outputs.storageAccountName
    functionInsightsKey: functionModule.outputs.insightsKey
    keyVaultName: keyVaultModule.outputs.name
    customAppSettings: {
      OpenApi__HideSwaggerUI: 'false'
      OpenApi__HideDocument: 'false'
      OpenApi__DocTitle: 'Isolated .NET10 Functions Demo APIs'
      OpenApi__DocDescription: 'This repo is an example of how to use Isolated .NET10 Azure Functions'
    }
  }
}

//--------------------------------------------------------------------------------
module functionFlexModule 'app/functionflex.bicep' = {
  name: 'functionFlex${deploymentSuffix}'
  dependsOn: [ appRoleAssignments ]
  params: {
    functionAppName: resourceNames.outputs.functionFlexAppName
    functionAppServicePlanName: resourceNames.outputs.functionFlexAppServicePlanName
    functionInsightsName: resourceNames.outputs.functionFlexInsightsName
    functionStorageAccountName: functionFlexStorageModule.outputs.name
    location: location
    commonTags: commonTags
    workspaceId: logAnalyticsWorkspaceModule.outputs.id
    adminPrincipalId: adminUserId
  }
}


// --------------------------------------------------------------------------------
output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP_NAME string = resourceGroupName
output WEB_HOST_NAME string = webSiteModule.outputs.hostName
output FUNCTION_HOST_NAME string = functionModule.outputs.hostname
output FLEX_FUNCTION_HOST_NAME string = functionFlexModule.outputs.hostname
