// -------------------------------------------------------------------------------------------------
// This BICEP file creates the shared infrastructure for Azure Functions Flex Consumption using AVM
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
module applicationInsights 'br/public:avm/res/insights/component:0.7.1' = {
  name: 'appInsights-${uniqueString(functionInsightsName, resourceGroup().id)}'
  params: {
    name: functionInsightsName
    location: location
    tags: tags
    workspaceResourceId: workspaceId
    applicationType: 'web'
    disableLocalAuth: true
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    enableTelemetry: false
  }
}

// Backing storage for Azure Functions deployment packages
module storageAccount 'br/public:avm/res/storage/storage-account:0.32.0' = {
  name: 'funcStorage-${uniqueString(functionStorageAccountName, resourceGroup().id)}'
  params: {
    name: functionStorageAccountName
    location: location
    tags: tags
    skuName: 'Standard_LRS'
    kind: 'StorageV2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
      ipRules: []
      virtualNetworkRules: []
    }
    blobServices: {
      containers: [
        {
          name: deploymentStorageContainerName
        }
      ]
    }
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------
// Outputs
output appInsightsConnectionString string = applicationInsights.outputs.connectionString
output appInsightsInstrumentationKey string = applicationInsights.outputs.instrumentationKey
output appInsightsName string = applicationInsights.outputs.name
output storageAccountName string = storageAccount.outputs.name
output storagePrimaryBlobEndpoint string = storageAccount.outputs.primaryBlobEndpoint
output deploymentStorageContainerName string = deploymentStorageContainerName
