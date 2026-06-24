// --------------------------------------------------------------------------------
// This BICEP file will create an Azure App Service Plan using AVM, or use an existing one
// --------------------------------------------------------------------------------
@description('The name of the app service plan')
param appServicePlanName string = ''
@description('The name of a pre-existing app service plan')
param existingServicePlanName string = ''
@description('The resource group name of a pre-existing app service plan')
param existingServicePlanResourceGroupName string = ''

param location string = resourceGroup().location
param commonTags object = {}
@allowed(['F1','B1','B2','S1','S2','S3'])
param sku string = 'B1'
param webAppKind string = 'linux'

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~website.bicep'}
var tags = union(commonTags, templateTag)

// --------------------------------------------------------------------------------
resource existingAppServiceResource 'Microsoft.Web/serverfarms@2024-11-01' existing = if (!empty(existingServicePlanName)) {
  name: existingServicePlanName
  scope: resourceGroup(existingServicePlanResourceGroupName == '' ? resourceGroup().name : existingServicePlanResourceGroupName)
}

module newAppServicePlan 'br/public:avm/res/web/serverfarm:0.7.0' = if (empty(existingServicePlanName)) {
  name: 'appServicePlan-${uniqueString(appServicePlanName, resourceGroup().id)}'
  params: {
    name: appServicePlanName
    location: location
    tags: tags
    skuName: sku
    kind: webAppKind
    reserved: webAppKind == 'linux' ? true : false
    enableTelemetry: false
  }
}

output name string = empty(existingServicePlanName) ? newAppServicePlan!.outputs.name : existingAppServiceResource.name
output id string = empty(existingServicePlanName) ? newAppServicePlan!.outputs.resourceId : existingAppServiceResource.id
output resourceGroupName string = empty(existingServicePlanName) ? resourceGroup().name : existingServicePlanResourceGroupName
