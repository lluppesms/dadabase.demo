// -------------------------------------------------------------------------------------------------
// This BICEP file creates the shared infrastructure for Azure Functions Flex Consumption
// - Application Insights
// - Storage Account (for function deployment packages)
// -------------------------------------------------------------------------------------------------

@description('Name of the Application Insights instance')
param functionInsightsName string

@description('Name of the storage account for function deployment')
param functionStorageAccountName string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Common tags to apply to resources')
param commonTags object = {}

@description('The workspace to store audit logs')
param workspaceId string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~functionserviceplan.bicep' }
var tags = union(commonTags, templateTag)
var resourceToken = toLower(uniqueString(subscription().id, resourceGroup().name, location))
var deploymentStorageContainerName = 'app-package-${take(functionStorageAccountName, 32)}-${take(resourceToken, 7)}'

// --------------------------------------------------------------------------------
// Application Insights for monitoring
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: functionInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspaceId
    DisableLocalAuth: true
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Backing storage for Azure Functions deployment packages
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: functionStorageAccountName
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    dnsEndpointType: 'Standard'
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: deploymentStorageContainerName
}

// --------------------------------------------------------------------------------
// Outputs
output appInsightsConnectionString string = applicationInsights.properties.ConnectionString
output appInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey
output appInsightsName string = applicationInsights.name
output storageAccountName string = storageAccount.name
output storagePrimaryBlobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output deploymentStorageContainerName string = deploymentStorageContainerName
