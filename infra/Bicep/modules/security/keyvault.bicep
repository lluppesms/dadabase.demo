// --------------------------------------------------------------------------------
// This BICEP file will create a KeyVault using AVM
// FYI: To purge a KV with soft delete enabled: > az keyvault purge --name kvName
// --------------------------------------------------------------------------------
param keyVaultName string = 'mykeyvaultname'
param location string = resourceGroup().location
param commonTags object = {}

@description('Administrators that should have access to administer key vault')
param adminUserObjectIds array = []
@description('Application that should have access to read key vault secrets')
param applicationUserObjectIds array = []

@description('Administrator UserId that should have access to administer key vault')
param keyVaultOwnerUserId string = ''
@description('Ip Address of the KV owner so they can read the vault, such as 254.254.254.254/32')
param keyVaultOwnerIpAddress string = ''

@description('Determines if Azure can deploy certificates from this Key Vault.')
param enabledForDeployment bool = true
@description('Determines if templates can reference secrets from this Key Vault.')
param enabledForTemplateDeployment bool = true
@description('Determines if this Key Vault can be used for Azure Disk Encryption.')
param enabledForDiskEncryption bool = true
@description('Determine if soft delete is enabled on this Key Vault.')
param enableSoftDelete bool = false
@description('Determine if purge protection is enabled on this Key Vault.')
param enablePurgeProtection bool = true
@description('The number of days to retain soft deleted vaults and vault objects.')
param softDeleteRetentionInDays int = 7
@description('Determines if access to the objects granted using RBAC. When true, access policies are ignored.')
param useRBAC bool = false

@allowed(['Enabled','Disabled'])
param publicNetworkAccess string = 'Enabled'
@allowed(['Allow','Deny'])
param allowNetworkAccess string = 'Allow'

@description('The workspace to store audit logs.')
@metadata({
  strongType: 'Microsoft.OperationalInsights/workspaces'
  example: '/subscriptions/<subscription_id>/resourceGroups/<resource_group>/providers/Microsoft.OperationalInsights/workspaces/<workspace_name>'
})
param workspaceId string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~keyvault.bicep' }
var tags = union(commonTags, templateTag)

var skuName = 'standard'
var subTenantId = subscription().tenantId

var ownerAccessPolicy = keyVaultOwnerUserId == '' ? [] : [
  {
    objectId: keyVaultOwnerUserId
    tenantId: subTenantId
    permissions: {
      certificates: [ 'all' ]
      secrets: [ 'all' ]
      keys: [ 'all' ]
    }
  } 
]
var adminAccessPolicies = [for adminUser in adminUserObjectIds: {
  objectId: adminUser
  tenantId: subTenantId
  permissions: {
    certificates: [ 'all' ]
    secrets: [ 'all' ]
    keys: [ 'all' ]
  }
}]
var applicationUserPolicies = [for appUser in applicationUserObjectIds: {
  objectId: appUser
  tenantId: subTenantId
  permissions: {
    secrets: [ 'get' ]
    keys: [ 'get', 'wrapKey', 'unwrapKey' ] // Azure SQL uses these permissions to access TDE key
  }
}]
var accessPolicies = union(ownerAccessPolicy, adminAccessPolicies, applicationUserPolicies)

var kvIpRules = keyVaultOwnerIpAddress == '' ? [] : [
  {
    value: keyVaultOwnerIpAddress
  }
] 

// --------------------------------------------------------------------------------
module keyVault 'br/public:avm/res/key-vault/vault:0.13.3' = {
  name: 'keyVault-${uniqueString(keyVaultName, resourceGroup().id)}'
  params: {
    name: keyVaultName
    location: location
    tags: tags
    sku: skuName
    enableRbacAuthorization: useRBAC
    accessPolicies: accessPolicies
    enableVaultForDeployment: enabledForDeployment
    enableVaultForTemplateDeployment: enabledForTemplateDeployment
    enableVaultForDiskEncryption: enabledForDiskEncryption
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionInDays
    enablePurgeProtection: enablePurgeProtection
    createMode: 'default'
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      defaultAction: allowNetworkAccess
      bypass: 'AzureServices'
      ipRules: kvIpRules
      virtualNetworkRules: []
    }
    diagnosticSettings: workspaceId != '' ? [
      {
        workspaceResourceId: workspaceId
        logCategoriesAndGroups: [
          { category: 'AuditEvent' }
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
output name string = keyVault.outputs.name
output id string = keyVault.outputs.resourceId
