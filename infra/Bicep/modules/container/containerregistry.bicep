// --------------------------------------------------------------------------------
// Azure Container Registry Module using AVM
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

@description('Optional Object ID of pipeline service principal to grant AcrPush')
param pipelineServicePrincipalObjectId string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~containerregistry.bicep' }
var tags = union(commonTags, templateTag)

var roleAssignmentsList = concat(
  !empty(managedIdentityPrincipalId) ? [
    {
      principalId: managedIdentityPrincipalId
      principalType: 'ServicePrincipal'
      roleDefinitionIdOrName: 'AcrPull'
    }
  ] : [],
  !empty(pipelineServicePrincipalObjectId) ? [
    {
      principalId: pipelineServicePrincipalObjectId
      principalType: 'ServicePrincipal'
      roleDefinitionIdOrName: 'AcrPush'
    }
  ] : []
)

// --------------------------------------------------------------------------------
module containerRegistry 'br/public:avm/res/container-registry/registry:0.12.1' = {
  name: 'containerRegistry-${uniqueString(containerRegistryName, resourceGroup().id)}'
  params: {
    name: containerRegistryName
    location: location
    tags: tags
    acrSku: sku
    acrAdminUserEnabled: adminUserEnabled
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    roleAssignments: roleAssignmentsList
    diagnosticSettings: workspaceId != '' ? [
      {
        workspaceResourceId: workspaceId
        logCategoriesAndGroups: [
          { category: 'ContainerRegistryRepositoryEvents' }
          { category: 'ContainerRegistryLoginEvents' }
        ]
        metricCategories: [
          { category: 'AllMetrics' }
        ]
      }
    ] : []
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------
output id string = containerRegistry.outputs.resourceId
output name string = containerRegistry.outputs.name
output loginServer string = containerRegistry.outputs.loginServer
output resourceGroupName string = resourceGroup().name
