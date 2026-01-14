# Set up GitHub Actions

The GitHub workflows in this project require several secrets set at the repository level or at the environment level.

---

## Workflow Definitions

- **[1-deploy-bicep.yml](./workflows/1-deploy-bicep.yml):** Deploys the main.bicep template with all new resources and does nothing else
- **[2-bicep-build-deploy-function.yml](./workflows/2-bicep-build-deploy-function.yml):** Builds an Azure Function and deploys it to Azure
- **[3-bicep-build-deploy-webapp.yml](./workflows/3-bicep-build-deploy-webapp.yml):** Builds an Azure Web App and deploys it to Azure
- **[4-build-deploy-dacpac.yml](./workflows/4-build-deploy-dacpac.yml):** Builds the database schema and deploys it to the SQL database
- **[5-run-sql-script.yml](./workflows/5-run-sql-script.yml):** Runs a SQL script against the Azure SQL Database
- **[6-pr-scan-build.yml](./workflows/6-pr-scan-build.yml):** Runs a build every pull request to ensure the code builds and passes all tests
- **[7-scan-code.yml](./workflows/7-scan-code.yml):** Periodically runs a security scan on the application and infrastructure code.
- **[8-smoke-test-webapp.yml](./workflows/8-smoke-test-webapp.yml):** Runs a Playwright smoke test of the web application after deployment

---

## Sequence of Workflows

When you first create the application, you will have to deploy the application using one of the workflows that deploy bicep.

In order for the workflows to be able to update the database, you must grant rights to the CICD service principal and the application managed identity in the database.

Grant full rights to your CICD Service Principal so it can deploy the DACPAC schema:

```sql
CREATE USER [yourServicePrincipalName] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [yourServicePrincipalName];
```

Grant user rights to your application managed identity so it can read and write data (not change the schema):

```sql
CREATE USER [yourAppManagedIdentityName] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [yourAppManagedIdentityName];
ALTER ROLE db_datawriter ADD MEMBER [yourAppManagedIdentityName];
GRANT EXECUTE ON SCHEMA::[dbo] TO [yourAppManagedIdentityName];
```

Once these rights are in place, before the application can run successfully, then you can deploy the SQL database schema and data using the [DACPAC deployment workflow](./workflows/4-build-deploy-dacpac.yml) and the [SQL script workflow](./workflows/5-run-sql-script.yml).  In the SQL Script workflow, choose the [InsertDefaultData.sql](../src/sql.database/Patch/InsertDefaultData.sql) script to populate the database with some starter data.

---

## Azure Credentials

You will need to set up the Azure Credentials secrets in the GitHub Secrets at the Repository level (or the environment level) before you do anything else.

See [https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions) for more info on how to create the service principal and set up these credentials.

> Note: this service principal must have contributor rights to your subscription (or resource group) to deploy the resources.

You can customize and run the following commands, or you can set these secrets up manually by going to the Settings -> Secrets -> Actions -> Secrets.

You can set these up at the Repository Level...

```bash
gh secret set AZURE_SUBSCRIPTION_ID -b <yourAzureSubscriptionId>
gh secret set AZURE_TENANT_ID -b <GUID-Entra-tenant-where-SP-lives>
gh secret set CICD_CLIENT_ID -b <GUID-application/client-Id>
```

but it's probably better to set up one set of credentials for each Environment:

```bash
gh secret set --env <ENV-NAME> AZURE_SUBSCRIPTION_ID -b <yourAzureSubscriptionId>
gh secret set --env <ENV-NAME> AZURE_TENANT_ID -b <GUID-Entra-tenant-where-SP-lives>
gh secret set --env <ENV-NAME> CICD_CLIENT_ID -b <GUID-application/client-Id>
```

These two secrets are optional if you want to grant an administrator access to the Key Vault and ACR and SQL database.  

```bash
gh secret set ADMIN_IP_ADDRESS 192.168.1.1
gh secret set ADMIN_PRINCIPAL_ID <yourGuid>
```

---

## Bicep Configuration Values

There are many values used by the Bicep templates to configure the resource names that are deployed. Make sure the App_Name variable is unique to your deployment. It will be used as the basis for the application name and for all the other Azure resources, some of which must be globally unique.

See the **[CreateGitHubSecrets.md](./CreateGitHubSecrets.md)** file for the full list of commands to create these variables and secrets.

---

## References

- [Deploying ARM Templates with GitHub Actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions)

---

[Home Page](../README.md)
