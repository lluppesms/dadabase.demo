// --------------------------------------------------------------------------------
// Azure Container App Module
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
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: containerRegistryServer
          identity: managedIdentityId
        }
      ]
    }
    template: {
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
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

// Enable diagnostic logging
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (!empty(workspaceId)) {
  name: '${containerApp.name}-diagnostics'
  scope: containerApp
  properties: {
    workspaceId: workspaceId
    // logs: [
    //   // it says this is not supported...
    //   // {
    //   //   category: 'ContainerAppConsoleLogs'
    //   //   enabled: true
    //   // }
    //   {
    //     category: 'ContainerAppSystemLogs'
    //     enabled: true
    //   }
    // ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// --------------------------------------------------------------------------------
output id string = containerApp.id
output name string = containerApp.name
output fqdn string = containerApp.properties.configuration.ingress.fqdn
output url string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output systemPrincipalId string = containerApp.identity.principalId
output userManagedPrincipalId string = managedIdentityPrincipalId
