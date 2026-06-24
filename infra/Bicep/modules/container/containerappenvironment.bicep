// --------------------------------------------------------------------------------
// Azure Container Apps Environment Module
// --------------------------------------------------------------------------------
param environmentName string
param location string = resourceGroup().location
param commonTags object = {}

@description('The workspace for Container Apps logs.')
param workspaceId string

@description('The Application Insights connection string.')
@secure()
param appInsightsConnectionString string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~containerappenvironment.bicep' }
var tags = union(commonTags, templateTag)

// --------------------------------------------------------------------------------
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: reference(workspaceId, '2022-10-01').customerId
        sharedKey: listKeys(workspaceId, '2022-10-01').primarySharedKey
      }
    }
    daprAIConnectionString: appInsightsConnectionString
    zoneRedundant: false
  }
}

// --------------------------------------------------------------------------------
output id string = containerAppsEnvironment.id
output name string = containerAppsEnvironment.name
output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
output staticIp string = containerAppsEnvironment.properties.staticIp
