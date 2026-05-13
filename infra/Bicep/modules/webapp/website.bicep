// --------------------------------------------------------------------------------
// This BICEP file will create an Azure Website using AVM
// --------------------------------------------------------------------------------
param webSiteName string = ''
param location string = resourceGroup().location
param appInsightsLocation string = resourceGroup().location
param environmentCode string = 'dev'
param commonTags object = {}
param managedIdentityId string
param managedIdentityPrincipalId string

@description('The workspace to store audit logs.')
param workspaceId string = ''

@description('The Name of the service plan to deploy into.')
param appServicePlanName string
param appServicePlanResourceGroupName string = resourceGroup().name
param webAppKind string = 'linux'

@description('Custom application settings to merge with the base settings.')
param customAppSettings object = {}

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~website.bicep'}
var azdTag = environmentCode == 'azd' ? { 'azd-service-name': 'web' } : {}
var tags = union(commonTags, templateTag)
var webSiteTags = union(commonTags, templateTag, azdTag)

var linuxFxVersion = webAppKind == 'linux' ? 'DOTNETCORE|10.0' : ''
var appInsightsName = toLower('${webSiteName}-insights')
var webAppKindValue = webAppKind == 'linux' ? 'app,linux' : 'app'

// Reference existing App Service Plan
resource appServiceResource 'Microsoft.Web/serverfarms@2024-11-01' existing = {
  name: appServicePlanName
  scope: resourceGroup(appServicePlanResourceGroupName)
}

// --------------------------------------------------------------------------------
// Application Insights
module appInsightsModule 'br/public:avm/res/insights/component:0.7.1' = {
  name: 'appInsights-${uniqueString(appInsightsName, resourceGroup().id)}'
  params: {
    name: appInsightsName
    location: appInsightsLocation
    tags: tags
    workspaceResourceId: workspaceId
    applicationType: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    enableTelemetry: false
  }
}

// Merge base settings with custom settings
var baseAppSettings = {
  APPINSIGHTS_INSTRUMENTATIONKEY: appInsightsModule.outputs.instrumentationKey
  APPLICATIONINSIGHTS_CONNECTION_STRING: appInsightsModule.outputs.connectionString
  ApplicationInsightsAgent_EXTENSION_VERSION: '~2'
}
var mergedAppSettings = union(baseAppSettings, customAppSettings)

// --------------------------------------------------------------------------------
// Web App
module webSiteModule 'br/public:avm/res/web/site:0.22.0' = {
  name: 'webSite-${uniqueString(webSiteName, resourceGroup().id)}'
  params: {
    name: webSiteName
    location: location
    tags: webSiteTags
    kind: webAppKindValue
    serverFarmResourceId: appServiceResource.id
    httpsOnly: true
    clientAffinityEnabled: false
    managedIdentities: {
      systemAssigned: true
      userAssignedResourceIds: [managedIdentityId]
    }
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      alwaysOn: true
      remoteDebuggingEnabled: false
      minimumElasticInstanceCount: 1
    }
    configs: [
      {
        name: 'appsettings'
        properties: mergedAppSettings
      }
      {
        name: 'logs'
        properties: {
          applicationLogs: {
            fileSystem: {
              level: 'Warning'
            }
          }
          httpLogs: {
            fileSystem: {
              retentionInMb: 40
              enabled: true
            }
          }
          failedRequestsTracing: {
            enabled: true
          }
          detailedErrorMessages: {
            enabled: true
          }
        }
      }
    ]
    diagnosticSettings: workspaceId != '' ? [
      {
        workspaceResourceId: workspaceId
        logCategoriesAndGroups: [
          { category: 'AppServiceIPSecAuditLogs' }
          { category: 'AppServiceAuditLogs' }
        ]
      }
    ] : []
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------
output name string = webSiteName
output hostName string = webSiteModule.outputs.defaultHostname
output systemPrincipalId string = webSiteModule.outputs.?systemAssignedMIPrincipalId ?? ''
output userManagedPrincipalId string = managedIdentityPrincipalId
output appInsightsName string = appInsightsName
output appInsightsKey string = appInsightsModule.outputs.instrumentationKey
output appInsightsConnectionString string = appInsightsModule.outputs.connectionString
