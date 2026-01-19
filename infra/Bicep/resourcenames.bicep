// --------------------------------------------------------------------------------
// Bicep file that builds all the resource names used by other Bicep templates
// --------------------------------------------------------------------------------
param appName string = ''
// @allowed(['azd','gha','azdo','dev','demo','qa','stg','ct','prod'])
param environmentCode string = 'azd'
param instanceNumber string = '1'

param functionStorageNameSuffix string = 'func'
param functionFlexStorageNameSuffix string = 'flex'
param environmentSpecificFunctionName string = ''
param environmentSpecificFlexFunctionName string = ''

// --------------------------------------------------------------------------------
var sanitizedEnvironment = toLower(environmentCode)
var sanitizedAppNameWithDashes = replace(replace(toLower(appName), ' ', ''), '_', '')
//var sanitizedAppName = replace(replace(replace(toLower(appName), ' ', ''), '-', ''), '_', '')
var sanitizedAppInstanceNameWithDashes = replace(replace(toLower('${appName}${instanceNumber}'), ' ', ''), '_', '')
var sanitizedAppNameInstance = replace(replace(replace(toLower('${appName}${instanceNumber}'), ' ', ''), '_', ''), '-', '')

// pull resource abbreviations from a common JSON file
var resourceAbbreviations = loadJsonContent('./data/resourceAbbreviations.json')

// --------------------------------------------------------------------------------
// if there's an environment specific function name specified, use that, otherwise if it's azd -- 
// other resource names can be changed if desired, but if using the "azd deploy" command it expects the
// function name to be exactly "{appName}function" so don't change the functionAppName format if using azd
var webSiteName         = environmentCode == 'prod' ? toLower('${sanitizedAppNameWithDashes}') : toLower('${sanitizedAppInstanceNameWithDashes}-${sanitizedEnvironment}')
var baseStorageName     = toLower('${sanitizedAppNameInstance}${resourceAbbreviations.storageAccountSuffix}${sanitizedEnvironment}')

output functionApp object = {
    appName: 'main'
    name: toLower('${sanitizedAppInstanceNameWithDashes}-${resourceAbbreviations.functionApp}-${sanitizedEnvironment}')
    servicePlanName: toLower('${sanitizedAppInstanceNameWithDashes}-${resourceAbbreviations.functionApp}-${resourceAbbreviations.appServicePlanSuffix}-${sanitizedEnvironment}')
    storageName: take('${baseStorageName}${functionStorageNameSuffix}', 24)
    deploymentStorageContainerName: toLower('app-package-${sanitizedAppInstanceNameWithDashes}-${resourceAbbreviations.functionApp}')
    insightsName: '${sanitizedAppInstanceNameWithDashes}-${resourceAbbreviations.functionApp}-${resourceAbbreviations.appInsightsSuffix}-${sanitizedEnvironment}'
}

// --------------------------------------------------------------------------------
output logAnalyticsWorkspaceName string  = toLower('${sanitizedAppInstanceNameWithDashes}-${sanitizedEnvironment}-${resourceAbbreviations.logWorkspaceSuffix}')
output webSiteName string                = webSiteName
output webSiteAppServicePlanName string  = '${webSiteName}-${resourceAbbreviations.appServicePlanSuffix}'
output webSiteAppInsightsName string     = '${webSiteName}-${resourceAbbreviations.appInsightsSuffix}'
output sqlServerName string              = toLower('${sanitizedAppNameInstance}${resourceAbbreviations.sqlAbbreviation}${sanitizedEnvironment}')

output userAssignedIdentityName string   = toLower('${sanitizedAppNameInstance}-app-${resourceAbbreviations.managedIdentity}')

// Key Vaults and Storage Accounts can only be 24 characters long
output keyVaultName string               = take('${sanitizedAppNameInstance}${resourceAbbreviations.keyVaultAbbreviation}${sanitizedEnvironment}', 24)
output storageAccountName string         = take('${sanitizedAppNameInstance}${resourceAbbreviations.storageAccountSuffix}${sanitizedEnvironment}', 24)
