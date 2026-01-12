// --------------------------------------------------------------------------------
// Bicep module to create Azure SQL Server and Database
// --------------------------------------------------------------------------------
param sqlServerName string = uniqueString('sql', resourceGroup().id)
param sqlDBName string = 'DadABase'
param location string = resourceGroup().location
param commonTags object = {}

// SQL Server SKU configuration
@allowed(['Basic','Standard','Premium','BusinessCritical','GeneralPurpose'])
param sqlSkuTier string = 'GeneralPurpose'
param sqlSkuFamily string = 'Gen5'
param sqlSkuName string = 'GP_S_Gen5'
param minCores int = 1 
param autoPauseMinutes int = 60

// Authentication
param sqlAdminUser string = ''
@secure()
param sqlAdminPassword string = ''
param adAdminUserId string = '' // Azure AD admin user ID (e.g., 'admin@domain.com')
param adAdminUserSid string = '' // Azure AD admin SID (object ID)
param adAdminTenantId string = '' // Azure AD tenant ID

// Monitoring
@description('Log Analytics workspace for diagnostics')
param workspaceId string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~sqlserver.bicep' }
var tags = union(commonTags, templateTag)
var adAdminOnly = sqlAdminUser == '' 
var adminDefinition = adAdminUserId == '' ? {} : {
  administratorType: 'ActiveDirectory'
  principalType: 'User'
  login: adAdminUserId
  sid: adAdminUserSid
  tenantId: adAdminTenantId
  azureADOnlyAuthentication: adAdminOnly
} 

// --------------------------------------------------------------------------------
resource sqlServerResource 'Microsoft.Sql/servers@2023-02-01-preview' = {
  name: sqlServerName
  location: location
  tags: tags
  properties: {
    administrators: adminDefinition
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
    administratorLogin: sqlAdminUser
    administratorLoginPassword: sqlAdminPassword
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// Firewall rule to allow Azure services
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-02-01-preview' = {
  parent: sqlServerResource
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlDBResource 'Microsoft.Sql/servers/databases@2023-02-01-preview' = {
  parent: sqlServerResource
  name: sqlDBName
  location: location
  tags: tags
  sku: {
    name: sqlSkuName
    tier: sqlSkuTier
    family: sqlSkuFamily
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 4294967296  // 4GB
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    autoPauseDelay: autoPauseMinutes
    requestedBackupStorageRedundancy: 'Local'
    minCapacity: minCores
    isLedgerOn: false
  }
}

// Diagnostic settings if workspace ID is provided
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (!empty(workspaceId)) {
  scope: sqlDBResource
  name: 'diagnostic-settings'
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'SQLInsights'
        enabled: true
      }
      {
        category: 'AutomaticTuning'
        enabled: true
      }
      {
        category: 'QueryStoreRuntimeStatistics'
        enabled: true
      }
      {
        category: 'QueryStoreWaitStatistics'
        enabled: true
      }
      {
        category: 'Errors'
        enabled: true
      }
      {
        category: 'DatabaseWaitStatistics'
        enabled: true
      }
      {
        category: 'Timeouts'
        enabled: true
      }
      {
        category: 'Blocks'
        enabled: true
      }
      {
        category: 'Deadlocks'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'Basic'
        enabled: true
      }
      {
        category: 'InstanceAndAppAdvanced'
        enabled: true
      }
      {
        category: 'WorkloadManagement'
        enabled: true
      }
    ]
  }
}

// --------------------------------------------------------------------------------
output sqlServerName string = sqlServerResource.name
output sqlServerFQDN string = sqlServerResource.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDBResource.name
output sqlServerId string = sqlServerResource.id
output sqlDatabaseId string = sqlDBResource.id
output connectionString string = 'Server=tcp:${sqlServerResource.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDBName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
