// ----------------------------------------------------------------------------------------------------
// This BICEP file will create an .NET 10 Isolated Azure Function
// ----------------------------------------------------------------------------------------------------
param functionAppName string = 'll-flex-test-2'
param functionAppServicePlanName string
param functionInsightsName string
param functionStorageAccountName string
@allowed([ 'functionapp', 'functionapp,linux' ])
param functionKind string = 'functionapp,linux'
param runtimeName string = 'dotnet-isolated'
param runtimeVersion string = '10.0'
param netFrameworkVersion string = 'v4.0'

param location string = resourceGroup().location
param appInsightsLocation string = resourceGroup().location
param commonTags object = {}
param managedIdentityId string

param keyVaultName string = ''

@description('The workspace to store audit logs.')
param workspaceId string = ''

param usePlaceholderDotNetIsolated string = '1'
param use32BitProcess string = 'false'
param functionsWorkerRuntime string = 'DOTNET-ISOLATED'
param functionsExtensionVersion string = '~4'
param nodeDefaultVersion string = '8.11.1'

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~functionapp.bicep' }
var azdTag = { 'azd-service-name': 'function' }
var tags = union(commonTags, templateTag)
var functionTags = union(commonTags, templateTag, azdTag)
var useKeyVaultConnection = false

// --------------------------------------------------------------------------------
resource appServiceResource 'Microsoft.Web/serverfarms@2021-03-01' existing = {
  name: functionAppServicePlanName
}

resource storageAccountResource 'Microsoft.Storage/storageAccounts@2019-06-01' existing = { name: functionStorageAccountName }
var accountKey = storageAccountResource.listKeys().keys[0].value
var functionStorageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountResource.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${accountKey}'
var functionStorageAccountKeyVaultReference = '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azurefilesconnectionstring)'

resource appInsightsResource 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: functionInsightsName
  location: appInsightsLocation
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    WorkspaceResourceId: workspaceId
  }
}

// --------------------------------------------------------------------------------
resource functionAppResource 'Microsoft.Web/sites@2024-11-01' = {
  name: functionAppName
  location: location
  kind: functionKind
  tags: functionTags
  identity: {
    //disable-next-line BCP036
    type: 'SystemAssigned, UserAssigned'
    //disable-next-line BCP036
    userAssignedIdentities: { '${managedIdentityId}': {} }
  }
  properties: {
    enabled: true
    hostNameSslStates: [
      {
        name: '${functionAppName}.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Standard'
      }
      {
        name: '${functionAppName}.scm.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Repository'
      }
    ]
    serverFarmId: appServiceResource.id
    reserved: true
    isXenon: false
    hyperV: false
    dnsConfiguration: {}
    outboundVnetRouting: {
      allTraffic: false
      applicationTraffic: false
      contentShareTraffic: false
      imagePullTraffic: false
      backupRestoreTraffic: false
    }
    siteConfig: {
      numberOfWorkers: 1
      //acrUseManagedIdentityCreds: false
      alwaysOn: false
      http20Enabled: true
      functionAppScaleLimit: 100
      minimumElasticInstanceCount: 0
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: useKeyVaultConnection ? functionStorageAccountKeyVaultReference : functionStorageAccountConnectionString
        }
        {
          name: 'AzureWebJobsDashboard'
          value: useKeyVaultConnection ? functionStorageAccountKeyVaultReference : functionStorageAccountConnectionString
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: useKeyVaultConnection ? functionStorageAccountKeyVaultReference : functionStorageAccountConnectionString
        }
        {
          name: 'StorageAccountConnectionString'
          value: useKeyVaultConnection ? functionStorageAccountKeyVaultReference : functionStorageAccountConnectionString
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsResource.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsightsResource.properties.InstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionsWorkerRuntime
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: functionsExtensionVersion
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: nodeDefaultVersion
        }
        {
          name: 'USE32BITWORKERPROCESS'
          value: use32BitProcess
        }
        {
          name: 'NET_FRAMEWORK_VERSION'
          value: netFrameworkVersion
        }
        {
          name: 'WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED'
          value: usePlaceholderDotNetIsolated
        }
      ]
    }
    functionAppConfig: {
      runtime: {
        name: runtimeName
        version: runtimeVersion
      }
      scaleAndConcurrency: {
        alwaysReady: []
        maximumInstanceCount: 20
        instanceMemoryMB: 2048
      }
    }
    scmSiteAlsoStopped: false
    clientAffinityEnabled: false
    clientAffinityProxyEnabled: false
    clientCertEnabled: false
    clientCertMode: 'Required'
    hostNamesDisabled: false
    ipMode: 'IPv4'
    containerSize: 1536
    dailyMemoryTimeQuota: 0
    httpsOnly: false
    endToEndEncryptionEnabled: false
    redundancyMode: 'None'
    publicNetworkAccess: 'Enabled'
    storageAccountRequired: false
  }
}

resource functionAppConfig 'Microsoft.Web/sites/config@2024-11-01' = {
  parent: functionAppResource
  name: 'web'
  properties: {
    numberOfWorkers: 1
    netFrameworkVersion: netFrameworkVersion
    requestTracingEnabled: false
    remoteDebuggingEnabled: false
    httpLoggingEnabled: false
    //acrUseManagedIdentityCreds: false
    logsDirectorySizeLimit: 35
    detailedErrorLoggingEnabled: false
    publishingUsername: 'REDACTED'
    scmType: 'None'
    use32BitWorkerProcess: false
    webSocketsEnabled: false
    alwaysOn: false
    managedPipelineMode: 'Integrated'
    loadBalancing: 'LeastRequests'
    experiments: {
      rampUpRules: []
    }
    autoHealEnabled: false
    vnetRouteAllEnabled: false
    vnetPrivatePortsCount: 0
    cors: {
      allowedOrigins: [
        'https://ms.portal.azure.com'
      ]
      supportCredentials: true
    }
    localMySqlEnabled: false
    ipSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictionsUseMain: false
    http20Enabled: true
    minTlsVersion: '1.2'
    scmMinTlsVersion: '1.2'
    ftpsState: 'FtpsOnly'
    preWarmedInstanceCount: 0
    functionAppScaleLimit: 100
    functionsRuntimeScaleMonitoringEnabled: false
    minimumElasticInstanceCount: 0
    azureStorageAccounts: {}
    http20ProxyFlag: 0
  }
}

resource functionAppBinding 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = {
  parent: functionAppResource
  name: '${functionAppResource.name}.azurewebsites.net'
  properties: {
    siteName: functionAppResource.name
    hostNameType: 'Verified'
  }
}


resource functionAppMetricLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${functionAppResource.name}-metrics'
  scope: functionAppResource
  properties: {
    workspaceId: workspaceId
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}
// https://learn.microsoft.com/en-us/azure/app-service/troubleshoot-diagnostic-logs
resource functionAppAuditLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${functionAppResource.name}-logs'
  scope: functionAppResource
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'FunctionAppLogs'
        enabled: true
      }
    ]
  }
}
resource appServiceMetricLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${appServiceResource.name}-metrics'
  scope: appServiceResource
  properties: {
    workspaceId: workspaceId
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// --------------------------------------------------------------------------------
output id string = functionAppResource.id
output hostname string = functionAppResource.properties.defaultHostName
output name string = functionAppName
output insightsName string = functionInsightsName
output insightsKey string = appInsightsResource.properties.InstrumentationKey
output storageAccountName string = functionStorageAccountName
