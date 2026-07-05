// --------------------------------------------------------------------------------
// This BICEP file will create or reference an Azure SignalR Service
// --------------------------------------------------------------------------------
param signalRServiceName string

@description('Name of an existing SignalR resource to use instead of creating a new one. Leave empty to create new.')
param existingSignalRName string = ''

param location string = resourceGroup().location
param commonTags object = {}
param environmentCode string = 'dev'

@description('Allowed origins for CORS. Add the web app URL here.')
param allowedOrigins array = ['*']

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~signalr.bicep' }
var tags = union(commonTags, templateTag)

// Tier configuration based on environment
var skuName = environmentCode == 'prod' ? 'Standard_S1' : 'Free_F1'
var skuTier = environmentCode == 'prod' ? 'Standard' : 'Free'
var skuCapacity = environmentCode == 'prod' ? 1 : 1

var useExistingSignalR = !empty(existingSignalRName)
var deployNewSignalR = !useExistingSignalR

// --------------------------------------------------------------------------------
resource existingSignalRResource 'Microsoft.SignalRService/signalR@2024-03-01' existing = if (useExistingSignalR) {
  name: useExistingSignalR ? existingSignalRName : signalRServiceName
}

resource signalRResource 'Microsoft.SignalRService/signalR@2024-03-01' = if (deployNewSignalR) {
  name: signalRServiceName
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
    capacity: skuCapacity
  }
  kind: 'SignalR'
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'True'
      }
      {
        flag: 'EnableMessagingLogs'
        value: 'True'
      }
    ]
    cors: {
      allowedOrigins: allowedOrigins
    }
    tls: {
      clientCertEnabled: false
    }
    publicNetworkAccess: 'Enabled'
  }
}

// --------------------------------------------------------------------------------
output signalRName string = useExistingSignalR ? existingSignalRResource.name : signalRResource.name
output signalRHostName string = useExistingSignalR ? existingSignalRResource.properties.hostName : signalRResource.properties.hostName

#disable-next-line outputs-should-not-contain-secrets
output signalRConnectionString string = useExistingSignalR ? existingSignalRResource.listKeys().primaryConnectionString : signalRResource.listKeys().primaryConnectionString
