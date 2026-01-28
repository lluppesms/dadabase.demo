// --------------------------------------------------------------------------------
// This BICEP file will create an Azure SQL Database
// Article about audit settings...
// https://learn.microsoft.com/en-us/azure/templates/microsoft.sql/servers/auditingsettings?pivots=deployment-language-bicep
// --------------------------------------------------------------------------------
param sqlServerName string = uniqueString('sql', resourceGroup().id)
param sqlDBName string = 'SampleDB'
param existingSqlServerName string = ''
param existingSqlServerResourceGroupName string = ''

param adAdminUserId string = '' // 'somebody@somedomain.com'
param adAdminUserSid string = '' // '12345678-1234-1234-1234-123456789012'
param adAdminTenantId string = '' // '12345678-1234-1234-1234-123456789012'
param userAssignedIdentityResourceId string = ''
param location string = resourceGroup().location
param commonTags object = {}

// basic serverless config: Tier='GeneralPurpose', Family='Gen5', Name='GP_S_Gen5'
@allowed(['Basic','Standard','Premium','BusinessCritical','GeneralPurpose'])
param sqlSkuTier string = 'GeneralPurpose'
param sqlSkuFamily string = 'Gen5'
param sqlSkuName string = 'GP_S_Gen5'
param mincores int = 2 // number of cores (from 0.5 to 40)
param autopause int = 60 // time in minutes

// param storageAccountName string = ''

// @description('Enable Auditing of Microsoft support operations (DevOps)')
// param isMSDevOpsAuditEnabled bool = false

@description('The workspace to store audit logs.')
@metadata({
  strongType: 'Microsoft.OperationalInsights/workspaces'
  example: '/subscriptions/<subscription_id>/resourceGroups/<resource_group>/providers/Microsoft.OperationalInsights/workspaces/<workspace_name>'
})
param workspaceId string = ''

param sqlAdminUser string = ''
@secure()
param sqlAdminPassword string = ''
param addSecurityControlIgnoreTag bool = false

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~sqlserver.bicep' }
var securityControlIgnoreTag = addSecurityControlIgnoreTag ? { SecurityControl: 'Ignore' } : {}
var tags = union(commonTags, templateTag, securityControlIgnoreTag)

// Default to AD-only authentication; only enable SQL local auth if sqlAdminPassword has a value
var useSqlAuth = !empty(sqlAdminPassword)
var adAdminOnly = !useSqlAuth
var adminDefinition = adAdminUserId == '' ? {} : {
  administratorType: 'ActiveDirectory'
  principalType: 'Group'
  login: adAdminUserId
  sid: adAdminUserSid
  tenantId: adAdminTenantId
  azureADOnlyAuthentication: adAdminOnly
} 
var primaryUserIdentity = userAssignedIdentityResourceId

var deployNewServer = empty(existingSqlServerName)

// --------------------------------------------------------------------------------
resource existingSqlServerResource 'Microsoft.Sql/servers@2024-11-01-preview' existing = if (!deployNewServer) {
  name: existingSqlServerName
  scope: resourceGroup(existingSqlServerResourceGroupName)
}
resource existingSqlDBResource 'Microsoft.Sql/servers/databases@2024-11-01-preview' existing = if (!deployNewServer) {
  parent: existingSqlServerResource
  name: sqlDBName
}

resource sqlServerResource 'Microsoft.Sql/servers@2024-11-01-preview' = if (deployNewServer) {
  name: sqlServerName
  location: location
  tags: tags
  properties: {
    administrators: adminDefinition
    primaryUserAssignedIdentityId: primaryUserIdentity
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Enabled'
    version: '12.0'
    administratorLogin: sqlAdminUser != '' ? sqlAdminUser : null
    administratorLoginPassword: sqlAdminPassword != '' ? sqlAdminPassword : null
    //keyId: 'string' // A CMK URI of the key to use for encryption.
  }
  identity:{
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityResourceId}': {}
    }
  }
  // identity: {
  //   type: 'SystemAssigned'
  // }
}

resource sqlDBResource 'Microsoft.Sql/servers/databases@2024-11-01-preview' = if (deployNewServer) {
  parent: sqlServerResource
  name: sqlDBName
  location: location
  tags: tags
  sku: {
    name: sqlSkuName
    tier: sqlSkuTier
    family: sqlSkuFamily
    capacity: 2
  }
  //kind: 'v12.0,user,vcore,serverless'
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 4294967296  // 34359738368 = 32G; 4294967296 = 4G
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    autoPauseDelay: autopause
    requestedBackupStorageRedundancy: 'Geo'
    minCapacity: mincores
    isLedgerOn: false
  }
}

// This rule will allow all Azure services and resources to access this server
resource sqlAllowAllAzureIps 'Microsoft.Sql/servers/firewallRules@2024-11-01-preview' = if (deployNewServer) {
  name: 'AllowAllWindowsAzureIps'
  parent: sqlServerResource
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

var diagnosticSettingsName = 'SQLSecurityAuditEvents_3d229c42-c7e7-4c97-9a99-ec0d0d8b86c1'
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (deployNewServer) {
  scope: sqlDBResource
  name: diagnosticSettingsName
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'SQLSecurityAuditEvents'
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
      {
        category: 'DevOpsOperationsAudit'
        enabled: true
        retentionPolicy: {
          days: 0
          enabled: false
        }
      }
    ]
  }
}

// --------------------------------------------------------------------------------
// Attempts to set up auditing on the database... still not quite working right...!
//   Would be nice but not critical for now
// --------------------------------------------------------------------------------
// Current issue:  (fix storage issue or switch to figure out LAW export instead...!)
//   "error": {
//     "code": "BlobAuditingStorageOutboundFirewallNotAllowed",
//     "message": "Storage account 'xxxxx' is not in the list of Outbound Firewall Rules on the Azure SQL Server.
//                 Please add the target to the Outbound Firewall Rules on server 'xxxxx' and retry the operation."
//   }
// --------------------------------------------------------------------------------
// resource auditingSettings 'Microsoft.Sql/servers/auditingSettings@2021-11-01-preview' = {
//   parent: sqlServerResource
//   name: 'default'
//   properties: {
//     state: 'Enabled'
//     isAzureMonitorTargetEnabled: true
//   }
// }
// resource devOpsAuditingSettings 'Microsoft.Sql/servers/devOpsAuditingSettings@2021-11-01-preview' = if (isMSDevOpsAuditEnabled) {
//   parent: sqlServerResource
//   name: 'default'
//   properties: {
//     state: 'Enabled'
//     isAzureMonitorTargetEnabled: true
//   }
// }
resource sqlDBAuditingSettings 'Microsoft.Sql/servers/auditingSettings@2024-11-01-preview' = if (deployNewServer) { // if (isMSDevOpsAuditEnabled) {
  parent: sqlServerResource
  name: 'default'
  properties: {
    retentionDays: 7
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    isAzureMonitorTargetEnabled: true
    state: 'Enabled'
    //isManagedIdentityInUse: false
    // storageAccountAccessKey: storageAccountKey
    // storageAccountSubscriptionId: storageSubscriptionId
    // storageEndpoint: storageEndpoint
  }
}

// --------------------------------------------------------------------------------
var outputServerName = deployNewServer ? sqlServerResource.name : existingSqlServerResource.name
var outputDatabaseName = deployNewServer ? sqlDBResource.name : existingSqlDBResource.name

output serverName string = outputServerName
output serverId string = deployNewServer ? sqlServerResource.id : existingSqlServerResource.id
output apiVersion string = deployNewServer ? sqlServerResource.apiVersion : existingSqlServerResource.apiVersion
output databaseName string = outputDatabaseName
output databaseId string = deployNewServer ? sqlDBResource.id : existingSqlDBResource.id
output identityConnectionString string = 'Server=tcp:${outputServerName}.database.windows.net,1433;Initial Catalog=${outputDatabaseName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;Authentication="Active Directory Default";'
//output serverPrincipalId string = sqlServerResource.identity.principalId
