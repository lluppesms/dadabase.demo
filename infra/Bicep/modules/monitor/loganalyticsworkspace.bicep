// --------------------------------------------------------------------------------
// Creates a Log Analytics Workspace using AVM
// --------------------------------------------------------------------------------
param logAnalyticsWorkspaceName string = 'myLogAnalyticsWorkspaceName'
param location string = resourceGroup().location
param commonTags object = {}

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~loganalytics.bicep' }
var tags = union(commonTags, templateTag)

// --------------------------------------------------------------------------------
module logAnalyticsWorkspace 'br/public:avm/res/operational-insights/workspace:0.15.0' = {
  name: 'logAnalyticsWorkspace-${uniqueString(logAnalyticsWorkspaceName, resourceGroup().id)}'
  params: {
    name: logAnalyticsWorkspaceName
    location: location
    tags: tags
    skuName: 'PerGB2018'
    dataRetention: 30
    dailyQuotaGb: '1'
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------
output id string = logAnalyticsWorkspace.outputs.resourceId
output name string = logAnalyticsWorkspace.outputs.name
