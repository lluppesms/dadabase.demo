// --------------------------------------------------------------------------------
// Azure Container App Module using AVM
// --------------------------------------------------------------------------------
param containerAppName string
param location string = resourceGroup().location
param environmentCode string = 'dev'
param commonTags object = {}

@description('The Container Apps Environment ID.')
param containerAppsEnvironmentId string

@description('The container image to deploy.')
param containerImage string

@description('The Container Registry login server.')
param containerRegistryServer string

@description('Managed Identity for pulling images from ACR.')
param managedIdentityId string
param managedIdentityPrincipalId string

@description('The workspace for diagnostic logs.')
param workspaceId string = ''

@description('Application Insights connection string.')
param appInsightsConnectionString string = ''

@description('Custom application settings/environment variables.')
param customAppSettings object = {}

@description('Minimum number of replicas.')
@minValue(0)
@maxValue(30)
param minReplicas int = 1

@description('Maximum number of replicas.')
@minValue(1)
@maxValue(30)
param maxReplicas int = 3

@description('CPU cores allocated to the container.')
param cpu string = '0.5'

@description('Memory allocated to the container.')
param memory string = '1Gi'

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~containerapp.bicep' }
var azdTag = environmentCode == 'azd' ? { 'azd-service-name': 'web' } : {}
var tags = union(commonTags, templateTag, azdTag)

// Base environment variables that are always applied
var baseEnvVars = [
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: appInsightsConnectionString
  }
  {
    name: 'ASPNETCORE_URLS'
    value: 'http://+:8080'
  }
  {
    name: 'DOTNET_RUNNING_IN_CONTAINER'
    value: 'true'
  }
  {
    name: 'ASPNETCORE_HTTP_PORTS'
    value: '8080'
  }
]

// Convert custom settings object to array format for Container Apps
var customEnvVars = [for setting in items(customAppSettings): {
  name: setting.key
  value: setting.value
}]

// Merge base and custom environment variables
var allEnvVars = union(baseEnvVars, customEnvVars)

// --------------------------------------------------------------------------------
module containerApp 'br/public:avm/res/app/container-app:0.22.1' = {
  name: 'containerApp-${uniqueString(containerAppName, resourceGroup().id)}'
  params: {
    name: containerAppName
    location: location
    tags: tags
    environmentResourceId: containerAppsEnvironmentId
    managedIdentities: {
      systemAssigned: true
      userAssignedResourceIds: [managedIdentityId]
    }
    registries: [
      {
        server: containerRegistryServer
        identity: managedIdentityId
      }
    ]
    ingressExternal: true
    ingressTargetPort: 8080
    ingressTransport: 'http'
    ingressAllowInsecure: false
    activeRevisionsMode: 'Single'
    scaleSettings: {
      minReplicas: minReplicas
      maxReplicas: maxReplicas
    }
    containers: [
      {
        name: containerAppName
        image: containerImage
        resources: {
          cpu: json(cpu)
          memory: memory
        }
        env: allEnvVars
      }
    ]
    diagnosticSettings: workspaceId != '' ? [
      {
        workspaceResourceId: workspaceId
        metricCategories: [
          { category: 'AllMetrics' }
        ]
      }
    ] : []
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------
output id string = containerApp.outputs.resourceId
output name string = containerApp.outputs.name
output fqdn string = containerApp.outputs.fqdn
output url string = 'https://${containerApp.outputs.fqdn}'
output systemPrincipalId string = containerApp.outputs.?systemAssignedMIPrincipalId ?? ''
output userManagedPrincipalId string = managedIdentityPrincipalId
