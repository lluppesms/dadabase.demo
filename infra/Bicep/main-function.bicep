// --------------------------------------------------------------------------------
// Main Bicep file that creates all of the Azure Resources for one environment
// --------------------------------------------------------------------------------
// To deploy this Bicep manually:
//   az deployment group create -n "manual-$(Get-Date -Format 'yyyyMMdd-HHmmss')" --resource-group rg_dadabase_full-dev --template-file 'main-function.bicep' --parameters functionAppName=lfldadabase-dev functionStorageName=lfldadabasedevstorefunc managedIdentityName=lfldadabase-app-id keyVaultName=lfldadabasevaultdev logAnalyticsWorkspaceName=lfl-dadabase-dev-law
// --------------------------------------------------------------------------------
param location string = resourceGroup().location

param functionAppName string
param functionStorageName string
param managedIdentityName string
param keyVaultName string
param logAnalyticsWorkspaceName string

param functionStorageSku string = 'Standard_LRS'
param functionAppSku string = 'B1' //  'Y1'
param functionAppSkuFamily string = 'B' // 'Y'
param functionAppSkuTier string = 'Dynamic'

// calculated variables disguised as parameters
param runDateTime string = utcNow()

// --------------------------------------------------------------------------------
var deploymentSuffix = '-${runDateTime}'
var resourceAbbreviations = loadJsonContent('./data/resourceAbbreviations.json')
var functionAppServicePlanName = '${functionAppName}-${resourceAbbreviations.appServicePlanSuffix}'
var functionInsightsName       = '${functionAppName}-${resourceAbbreviations.appInsightsSuffix}'


resource logWorkspaceResource 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  name: logAnalyticsWorkspaceName
}
resource existingIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = {
  name: managedIdentityName
}
 

module functionStorageModule 'app/storageaccount.bicep' = {
  name: 'functionstorage${deploymentSuffix}'
  params: {
    storageSku: functionStorageSku
    storageAccountName: functionStorageName // resourceNames.outputs.functionStorageName
    location: location
    // commonTags: commonTags
    allowNetworkAccess: 'Allow'
    publicNetworkAccess: 'Enabled'
    addSecurityControlIgnoreTag: true
  }
}

// --------------------------------------------------------------------------------
// Creation of storage file share failed with: 'The remote server returned an error: (403) Forbidden.'. Please check if the storage account is accessible.
// --------------------------------------------------------------------------------
module functionModule 'app/functionapp.bicep' = {
  name: 'function${deploymentSuffix}'
  params: {
    functionAppName: functionAppName
    functionAppServicePlanName: functionAppServicePlanName
    functionInsightsName: functionInsightsName
    managedIdentityId: existingIdentity.id
    keyVaultName: keyVaultName

    appInsightsLocation: location
    location: location
    //commonTags: commonTags

    functionKind: 'functionapp,linux'
    functionAppSku: functionAppSku
    functionAppSkuFamily: functionAppSkuFamily
    functionAppSkuTier: functionAppSkuTier
    functionStorageAccountName: functionStorageModule.outputs.name
    workspaceId: logWorkspaceResource.id
  }
}

module functionAppSettingsModule 'app/functionappsettings.bicep' = {
  name: 'functionAppSettings${deploymentSuffix}'
  params: {
    functionAppName: functionModule.outputs.name
    functionStorageAccountName: functionModule.outputs.storageAccountName
    functionInsightsKey: functionModule.outputs.insightsKey
    keyVaultName: keyVaultName
    customAppSettings: {
      OpenApi__HideSwaggerUI: 'false'
      OpenApi__HideDocument: 'false'
      OpenApi__DocTitle: 'Isolated .NET10 Functions Demo APIs'
      OpenApi__DocDescription: 'This repo is an example of how to use Isolated .NET10 Azure Functions'
    }
  }
}

// --------------------------------------------------------------------------------
output SUBSCRIPTION_ID string = subscription().subscriptionId
output FUNCTION_HOST_NAME string = functionModule.outputs.hostname
