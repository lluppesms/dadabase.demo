# Set up GitHub Secrets

The GitHub workflows in this project require several secrets set at the repository level.

---

## Azure Resource Creation Credentials

You need to set up the Azure Credentials secret in the GitHub Secrets at the Repository level before you do anything else.

See [https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions) for more info.

To create these secrets, customize and run this command::

``` bash
gh auth login

gh secret set --env dev AZURE_TENANT_ID -b <GUID>
gh secret set --env dev CICD_CLIENT_ID -b <GUID>
gh secret set --env dev AZURE_SUBSCRIPTION_ID -b <yourAzureSubscriptionId>
```

---

## Bicep Configuration Values

These variables and secrets are used by the Bicep templates to configure the resource names that are deployed.  Make sure the App_Name variable is unique to your deploy. It will be used as the basis for the website name and for all the other Azure resources, which must be globally unique.
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

DEPRECATED: gh variable set WEB_APP_PROJECT_FOLDER_NAME -b 'src/web/Website'
DEPRECATED: gh variable set WEB_APP_PROJECT_NAME -b 'DadABase.Web'
DEPRECATED: gh variable set WEB_APP_TEST_FOLDER_NAME -b 'src/web/Tests'
DEPRECATED: gh variable set WEB_APP_TEST_PROJECT_NAME -b 'DadABase.Tests'

DEPRECATED: gh variable set FUNCTION_APP_PROJECT_FOLDER_NAME -b 'src/function/Function'
DEPRECATED: gh variable set FUNCTION_APP_PROJECT_NAME -b 'DadABase.Function'
DEPRECATED: gh variable set FUNCTION_APP_TEST_FOLDER_NAME -b 'src/function/Tests'
DEPRECATED: gh variable set FUNCTION_APP_TEST_PROJECT_NAME -b 'DadABase.Function.Tests'

DEPRECATED: gh variable set SQL_APP_PROJECT_FOLDER_NAME -b 'src/sql.database'
DEPRECATED: gh variable set SQL_APP_SOLUTION_NAME -b 'sql.database'
DEPRECATED: gh variable set SQL_APP_PROJECT_NAME -b 'sql.database'

DEPRECATED: gh variable set CONSOLE_APP_PROJECT_FOLDER_NAME -b 'src/console'
DEPRECATED: gh variable set CONSOLE_APP_PROJECT_NAME -b 'dadabase.net10.console'

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
```

---

## References

[Deploying ARM Templates with GitHub Actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions)

[GitHub Secrets CLI](https://cli.github.com/manual/gh_secret_set)
