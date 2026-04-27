// --------------------------------------------------------------------------------
// This BICEP file will create storage account using AVM
// FYI: To purge a storage account with soft delete enabled: > az storage account purge --name storeName
// --------------------------------------------------------------------------------
param storageAccountName string = 'mystorageaccountname'
param location string = resourceGroup().location
param commonTags object = {}

// @allowed([ 'Standard_LRS', 'Standard_GRS', 'Standard_RAGRS' ])
param storageSku string = 'Standard_LRS'
param storageAccessTier string = 'Hot'
param containerNames array = ['input','output']
@allowed(['Enabled','Disabled'])
param publicNetworkAccess string = 'Enabled'
@allowed(['Allow','Deny'])
param allowNetworkAccess string = 'Deny' // except for Azure Services
param addSecurityControlIgnoreTag bool = false

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~storageAccount.bicep' }
var securityControlIgnoreTag = addSecurityControlIgnoreTag ? { SecurityControl: 'Ignore' } : {}
var tags = union(commonTags, templateTag, securityControlIgnoreTag)

// --------------------------------------------------------------------------------
module storageAccount 'br/public:avm/res/storage/storage-account:0.32.0' = {
  name: 'storageAccount-${uniqueString(storageAccountName, resourceGroup().id)}'
  params: {
    name: storageAccountName
    location: location
    tags: tags
    skuName: storageSku
    kind: 'StorageV2'
    accessTier: storageAccessTier
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      defaultAction: allowNetworkAccess
      bypass: 'AzureServices'
      ipRules: []
      virtualNetworkRules: []
    }
    blobServices: {
      deleteRetentionPolicyEnabled: true
      deleteRetentionPolicyDays: 7
      containerDeleteRetentionPolicyEnabled: true
      containerDeleteRetentionPolicyDays: 7
      containers: [for containerName in containerNames: {
        name: containerName
        publicAccess: 'None'
      }]
    }
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------
output id string = storageAccount.outputs.resourceId
output name string = storageAccount.outputs.name
