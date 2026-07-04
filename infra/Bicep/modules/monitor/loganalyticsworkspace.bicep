// --------------------------------------------------------------------------------
// Creates or references a Log Analytics Workspace
// --------------------------------------------------------------------------------
param logAnalyticsWorkspaceName string = 'myLogAnalyticsWorkspaceName'
param existingLogAnalyticsWorkspaceName string = ''
param location string = resourceGroup().location
param commonTags object = {}

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~loganalytics.bicep' }
var tags = union(commonTags, templateTag)
var useExisting = !empty(existingLogAnalyticsWorkspaceName)

// --------------------------------------------------------------------------------
resource existingLogWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = if (useExisting) {
  name: existingLogAnalyticsWorkspaceName
}

resource newLogWorkspaceResource 'Microsoft.OperationalInsights/workspaces@2021-06-01' = if (!useExisting) {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    sku: {
        name: 'PerGB2018' // Standard
    }
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    //you can limit the maximum daily ingestion on the Workspace by providing a value for dailyQuotaGb. 
    // Note: Bicep expects an integer, however in order to set the minimum possible value of 0.023 GB
    // you need to pass it as a string which will work just fine.
    // Note: this settings works in Azure DevOps pipelines, but fails in a GitHub action because it throws a warning/error:
    //   dailyQuotaGb: '0.023'
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

// --------------------------------------------------------------------------------
output id string = useExisting ? existingLogWorkspace.id : newLogWorkspaceResource!.id
output name string = useExisting ? existingLogWorkspace.name : newLogWorkspaceResource!.name
