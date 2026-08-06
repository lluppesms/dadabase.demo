# Set up GitHub Secrets

The GitHub workflows in this project require several secrets set at the repository level.

---

## Azure Credentials

Before you begin, you will need to set up the Azure Credentials secrets in the GitHub Secrets at the Repository level (or the environment level).  These secrets and credentials will allow the GitHub Actions to deploy into Azure.

See the reference links below for more info on how to create the service principal and set up the Federated Credentials.

> Note: this service principal must have **Contributor** rights to your subscription (or resource group) to deploy the resources. If you want to assign roles in the Bicep, it will also need the **User Access Administrator** role.  (Alternatively, you can put the service principal in the **Owner** role also, but that doesn't follow least privilege.)

### Update on Federated Credentials
Prior to July 2026, when you set up a federated identity credential in an App Registration to use in a GH Action, you only needed to supply the owner name and repo name and (environment/branch). All repositories created after **July 15, 2026** will now have to also supply the **IMMUTABLE** values (which are numeric values for the org/user and the repository).

To find those values, run these commands and they will return the numeric **IMMUTABLE** values:
```bash
gh api user --jq .id
gh api repos/<yourOrg>/<yourRepo> --jq .id
```

> NOTE: the first command is for a *USER* (i.e. lluppesms), NOT for an *ORG*…  I'm not sure if there is a different command for that.

Once the credentials are set up, customize and run this command to create these secrets:

``` bash
gh auth login

gh secret set --env dev AZURE_TENANT_ID -b <GUID>
gh secret set --env dev CICD_CLIENT_ID -b <GUID>
gh secret set --env dev AZURE_SUBSCRIPTION_ID -b <yourAzureSubscriptionId>
```

---

## Bicep Configuration Values

These variables and secrets are used by the Bicep templates to configure the resource names that are deployed.  Make sure the APP_NAME variable is unique to your deploy. It will be used as the basis for the website name and for all the other Azure resources, which must be globally unique.

To create these additional secrets and variables, customize and run this command:

Secret Values:

``` bash
gh auth login

gh variable set APP_NAME -b 'full-dadabase'
gh variable set RESOURCE_GROUP_LOCATION -b 'centralus'
gh variable set RESOURCE_GROUP_PREFIX -b 'rg-dadabase' 
gh variable set INSTANCE_NUMBER -b 1
gh variable set API_KEY -b 'somesecretstring'

gh secret set LOGIN_CLIENTID -b '<yourADClientId>'
gh secret set LOGIN_DOMAIN -b '<yourdomain>.onmicrosoft.com'
gh secret set LOGIN_INSTANCEENDPOINT -b 'https://login.microsoftonline.com/'
gh secret set LOGIN_TENANTID -b '<yourTenantId>'

gh variable set OPENAI_CHAT_DEPLOYMENTNAME -b 'gpt-5-mini'
gh variable set OPENAI_CHAT_MAXTOKENS -b '300'
gh variable set OPENAI_CHAT_TEMPERATURE -b '0.7'
gh variable set OPENAI_CHAT_TOPP -b '0.95'
gh variable set OPENAI_IMAGE_DEPLOYMENTNAME -b 'gpt-image-1.5'

gh variable set SQL_SERVER_NAME_PREFIX -b 'your-dadabase-server'
gh variable set SQL_DATABASE_NAME -b 'DadABase'
gh variable set SQLADMIN_LOGIN_USERID -b 'youruser@yourdomain.com'
gh variable set SQLADMIN_LOGIN_USERSID -b 'yoursid'
gh variable set SQLADMIN_LOGIN_TENANTID -b 'yourtennant'

gh variable set CREATE_USER_ASSIGNED_IDENTITY -b 'false'
gh variable set ADMIN_USER_LIST -b 'user1@domain.com,user2@domain.com'

gh variable set EXISTING_SERVICEPLAN_NAME -b ''
gh variable set EXISTING_SERVICEPLAN_RESOURCE_GROUP_NAME -b ''
gh variable set EXISTING_SQLSERVER_NAME -b ''
gh variable set EXISTING_SQLDATABASE_NAME -b ''
gh variable set EXISTING_SQLSERVER_RESOURCE_GROUP_NAME -b ''
gh variable set EXISTING_LOGANALYTICSWORKSPACE -b ''
gh variable set EXISTING_LOG_ANALYTICS_WORKSPACE_RESOURCE_GROUP_NAME -b ''
```

Leave the `EXISTING_*` variables blank to let Bicep create new resources. To reuse SQL resources, set both `EXISTING_SQLSERVER_NAME` and `EXISTING_SQLDATABASE_NAME`. To reuse an existing Log Analytics Workspace, set `EXISTING_LOGANALYTICSWORKSPACE` to its name and optionally `EXISTING_LOG_ANALYTICS_WORKSPACE_RESOURCE_GROUP_NAME` if it is in a different resource group. Set `CREATE_USER_ASSIGNED_IDENTITY` to `'true'` to provision a separate user-assigned managed identity; leave it `'false'` (default) to use each resource's own system-assigned identity.

---

## References

- [Deploying ARM Templates with GitHub Actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions)
- [Manage Federated Identity Credential in Entra Id](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation-create-trust?pivots=identity-wif-apps-methods-azp) (MS Learn)
- [Immutable subject claims for GitHub Actions OIDC tokens](https://github.blog/changelog/2026-04-23-immutable-subject-claims-for-github-actions-oidc-tokens/) (GitHub Changelog Announcement - April 2026)
- [Migrate GitHub Actions federated credentials to immutable subjects](https://learn.microsoft.com/en-us/entra/workload-id/workload-identities-github-immutable-subjects) (MS Learn)
- [GitHub Secrets CLI](https://cli.github.com/manual/gh_secret_set)
- [GitHub Variables CLI](https://cli.github.com/manual/gh_variable_set)
