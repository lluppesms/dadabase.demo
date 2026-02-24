// --------------------------------------------------------------------------------
// Azure Container Registry Module
// --------------------------------------------------------------------------------
param containerRegistryName string
param location string = resourceGroup().location
param commonTags object = {}

@description('The pricing tier for the Container Registry')
@allowed(['Basic', 'Standard', 'Premium'])
param sku string = 'Basic'

@description('Enable admin user for the registry')
param adminUserEnabled bool = true

@description('The workspace to store diagnostic logs.')
param workspaceId string = ''

@description('Managed identity for pull/push access')
param managedIdentityPrincipalId string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~containerregistry.bicep' }
var tags = union(commonTags, templateTag)

// --------------------------------------------------------------------------------
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: adminUserEnabled
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    zoneRedundancy: 'Disabled'
  }
}

// Enable diagnostic logging
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (!empty(workspaceId)) {
  name: '${containerRegistry.name}-diagnostics'
  scope: containerRegistry
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'ContainerRegistryRepositoryEvents'
        enabled: true
      }
      {
        category: 'ContainerRegistryLoginEvents'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// Assign AcrPull role to managed identity
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(managedIdentityPrincipalId)) {
  name: guid(containerRegistry.id, managedIdentityPrincipalId, 'AcrPull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// --------------------------------------------------------------------------------
output id string = containerRegistry.id
output name string = containerRegistry.name
output loginServer string = containerRegistry.properties.loginServer
output resourceGroupName string = resourceGroup().name
