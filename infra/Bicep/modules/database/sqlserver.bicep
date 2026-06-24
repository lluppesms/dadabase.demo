// --------------------------------------------------------------------------------
// This BICEP file will create an Azure SQL Database using AVM
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

// basic serverless config: Tier='GeneralPurpose', Family='Gen5', Name='GP_S_Gen5_2'
// Note: AVM prepends 'SQLDB_' to the SKU name; use explicit vCore count suffix (e.g. _2) for serverless SKUs
@allowed(['Basic','Standard','Premium','BusinessCritical','GeneralPurpose'])
param sqlSkuTier string = 'GeneralPurpose'
param sqlSkuFamily string = 'Gen5'
param sqlSkuName string = 'GP_S_Gen5_2'
param mincores int = 2 // number of cores (from 0.5 to 40)
param autopause int = 60 // time in minutes

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

// --------------------------------------------------------------------------------
module sqlServer 'br/public:avm/res/sql/server:0.21.1' = if (deployNewServer) {
  name: 'sqlServer-${uniqueString(sqlServerName, resourceGroup().id)}'
  params: {
    name: sqlServerName
    location: location
    tags: tags
    administratorLogin: sqlAdminUser != '' ? sqlAdminUser : null
    administratorLoginPassword: sqlAdminPassword != '' ? sqlAdminPassword : null
    administrators: adAdminUserId != '' ? {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: adAdminUserId
      sid: adAdminUserSid
      tenantId: adAdminTenantId
      azureADOnlyAuthentication: adAdminOnly
    } : null
    managedIdentities: !empty(userAssignedIdentityResourceId) ? {
      userAssignedResourceIds: [userAssignedIdentityResourceId]
    } : null
    primaryUserAssignedIdentityResourceId: !empty(userAssignedIdentityResourceId) ? userAssignedIdentityResourceId : null
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Enabled'
    firewallRules: [
      {
        name: 'AllowAllWindowsAzureIps'
        startIpAddress: '0.0.0.0'
        endIpAddress: '0.0.0.0'
      }
    ]
    auditSettings: {
      state: 'Enabled'
      retentionDays: 7
      auditActionsAndGroups: [
        'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
        'FAILED_DATABASE_AUTHENTICATION_GROUP'
        'BATCH_COMPLETED_GROUP'
      ]
      isAzureMonitorTargetEnabled: true
    }
    databases: [
      {
        name: sqlDBName
        availabilityZone: -1
        sku: {
          name: sqlSkuName
          tier: sqlSkuTier
        }
        autoPauseDelay: autopause
        minCapacity: string(mincores)
        maxSizeBytes: 4294967296  // 4G
        zoneRedundant: false
        collation: 'SQL_Latin1_General_CP1_CI_AS'
        catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
        readScale: 'Disabled'
        requestedBackupStorageRedundancy: 'Geo'
        diagnosticSettings: workspaceId != '' ? [
          {
            workspaceResourceId: workspaceId
            logCategoriesAndGroups: [
              { category: 'SQLSecurityAuditEvents' }
              { category: 'DevOpsOperationsAudit' }
            ]
          }
        ] : []
      }
    ]
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------
var outputServerName = deployNewServer ? sqlServer!.outputs.name : existingSqlServerResource.name
var outputDatabaseName = deployNewServer ? sqlDBName : existingSqlDBResource.name

output serverName string = outputServerName
output serverId string = deployNewServer ? sqlServer!.outputs.resourceId : existingSqlServerResource.id
output apiVersion string = '2024-11-01-preview'
output databaseName string = outputDatabaseName
output databaseId string = deployNewServer ? resourceId('Microsoft.Sql/servers/databases', outputServerName, sqlDBName) : existingSqlDBResource.id
output identityConnectionString string = 'Server=tcp:${outputServerName}.database.windows.net,1433;Initial Catalog=${outputDatabaseName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;Authentication="Active Directory Default";'
