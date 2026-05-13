// --------------------------------------------------------------------------------
// Azure Container Apps Environment Module using AVM
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
module containerAppsEnvironment 'br/public:avm/res/app/managed-environment:0.13.2' = {
  name: 'containerAppsEnv-${uniqueString(environmentName, resourceGroup().id)}'
  params: {
    name: environmentName
    location: location
    tags: tags
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsWorkspaceResourceId: workspaceId
    }
    daprAIConnectionString: appInsightsConnectionString
    publicNetworkAccess: 'Enabled'
    zoneRedundant: false
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------
output id string = containerAppsEnvironment.outputs.resourceId
output name string = containerAppsEnvironment.outputs.name
output defaultDomain string = containerAppsEnvironment.outputs.defaultDomain
output staticIp string = containerAppsEnvironment.outputs.staticIp
