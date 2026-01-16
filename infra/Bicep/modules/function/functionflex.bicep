// ----------------------------------------------------------------------------------------------------
// This BICEP file will create an .NET 10 Isolated Azure Flex Function
// See: https://github.com/Azure-Samples/azure-functions-flex-consumption-samples/blob/main/IaC/bicep/main.bicep
// Note that each flex function has to have it's own service plan, so that's in here also.
// ----------------------------------------------------------------------------------------------------
param functionAppName string
param functionAppServicePlanName string
param functionInsightsName string
param functionStorageAccountName string
param deploymentStorageContainerName string = ''

@description('SKU name for App Service Plan')
param appServicePlanSkuName string = 'FC1'

@description('SKU tier for App Service Plan')
param appServicePlanTier string = 'FlexConsumption'

param customAppSettings object = {}

@allowed(['functionapp', 'functionapp,linux'])
param functionKind string = 'functionapp,linux'
param runtimeName string = 'dotnet-isolated'
param runtimeVersion string = '10.0'
@minValue(10)
@maxValue(1000)
param maximumInstanceCount int = 50
@allowed([512, 2048, 4096])
param instanceMemoryMB int = 2048

param addRoleAssignments bool = true
param appInsightsName string
param storageAccountName string
param keyVaultName string

param location string = resourceGroup().location
param commonTags object = {}
param deploymentSuffix string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~functionflex.bicep' }
var azdTag = { 'azd-service-name': 'function' }
var functionTags = union(commonTags, templateTag, azdTag)

var resourceToken = toLower(uniqueString(subscription().id, resourceGroup().name, location, functionAppServicePlanName))
// Use provided container name or calculate a default
var actualDeploymentContainerName = !empty(deploymentStorageContainerName) ? deploymentStorageContainerName : 'app-package-${take(functionStorageAccountName, 32)}-${take(resourceToken, 7)}'

// --------------------------------------------------------------------------------
resource applicationInsightsResource 'Microsoft.Insights/components@2020-02-02' existing = {
  name: functionInsightsName
}
var applicationInsightsConnectionString = applicationInsightsResource.properties.ConnectionString

resource storageAccountResource 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: functionStorageAccountName
}
var storagePrimaryBlobEndpoint = storageAccountResource.properties.primaryEndpoints.blob

// Create the deployment blob container for Flex Consumption function app packages
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' existing = {
  parent: storageAccountResource
  name: 'default'
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: actualDeploymentContainerName
  properties: {
    publicAccess: 'None'
  }
}

var baseAppSettings = {
    AzureWebJobsStorage__accountName: functionStorageAccountName
    AzureWebJobsStorage__credential: 'managedidentity'
    AzureWebJobsStorage__blobServiceUri: 'https://${functionStorageAccountName}.blob.${environment().suffixes.storage}'
    AzureWebJobsStorage__queueServiceUri: 'https://${functionStorageAccountName}.queue.${environment().suffixes.storage}'
    AzureWebJobsStorage__tableServiceUri: 'https://${functionStorageAccountName}.table.${environment().suffixes.storage}'
    APPLICATIONINSIGHTS_CONNECTION_STRING: applicationInsightsConnectionString
    APPLICATIONINSIGHTS_AUTHENTICATION_STRING: 'Authorization=AAD'
}

// --------------------------------------------------------------------------------
// App Service Plan for Flex Consumption functions
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: functionAppServicePlanName
  location: location
  tags: functionTags
  kind: 'functionapp'
  sku: {
    name: appServicePlanSkuName
    tier: appServicePlanTier
  }
  properties: {
    reserved: true
  }
}

module functionAppResource 'br/public:avm/res/web/site:0.16.0' = {
  name: '${functionAppName}${deploymentSuffix}'
  dependsOn: [
    deploymentContainer
  ]
  params: {
    name: functionAppName
    location: location
    kind: functionKind
    tags: functionTags
    managedIdentities: {
      systemAssigned: true
    }
    serverFarmResourceId: appServicePlan.id
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storagePrimaryBlobEndpoint}${actualDeploymentContainerName}'
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }
      runtime: {
        name: runtimeName
        version: runtimeVersion
      }
    }
    siteConfig: {
      alwaysOn: false
    }
    configs: [{
      name: 'appsettings'
      properties: union(baseAppSettings, customAppSettings)
    }]
  }
}

module functionRoleAssignments '../iam/roleassignments.bicep' = if (addRoleAssignments) {
  name: '${functionAppName}-roles${deploymentSuffix}'
  params: {
    identityPrincipalId: functionAppResource.outputs.systemAssignedMIPrincipalId
    principalType: 'ServicePrincipal'
    // appInsightsName: appInsightsName
    storageAccountName: storageAccountName
    keyVaultName: keyVaultName
  }
}

// --------------------------------------------------------------------------------
output id string = functionAppResource.outputs.resourceId
output hostname string = functionAppResource.outputs.defaultHostname
output name string = functionAppName
output insightsName string = functionInsightsName
output insightsKey string = applicationInsightsResource.properties.InstrumentationKey
output storageAccountName string = functionStorageAccountName
output functionAppPrincipalId string = functionAppResource.outputs.?systemAssignedMIPrincipalId ?? ''
