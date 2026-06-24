// --------------------------------------------------------------------------------------------------------------
// Bicep file to deploy a user assigned identity using AVM
// --------------------------------------------------------------------------------------------------------------
param identityName string = ''
param existingIdentityName string  = ''
param location string = resourceGroup().location
param tags object = {}

var useExistingIdentity = !empty(existingIdentityName)

resource existingIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = if (useExistingIdentity) {
  name: existingIdentityName
}

module newIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.5.0' = if (!useExistingIdentity) {
  name: 'identity-${uniqueString(identityName, resourceGroup().id)}'
  params: {
    name: identityName
    location: location
    tags: tags
    enableTelemetry: false
  }
}

// --------------------------------------------------------------------------------------------------------------
// Outputs
// --------------------------------------------------------------------------------------------------------------
output managedIdentityId string = useExistingIdentity ? existingIdentity.id : newIdentity!.outputs.resourceId
output managedIdentityName string = useExistingIdentity ? existingIdentity.name : newIdentity!.outputs.name
output managedIdentityTenantId string = useExistingIdentity ? existingIdentity.properties.tenantId : subscription().tenantId
output managedIdentityClientId string = useExistingIdentity ? existingIdentity.properties.clientId : newIdentity!.outputs.clientId
output managedIdentityPrincipalId string = useExistingIdentity ? existingIdentity.properties.principalId : newIdentity!.outputs.principalId
