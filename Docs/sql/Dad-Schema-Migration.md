---
title: Dad Schema Migration Analysis
description: Analysis and migration plan for moving DadABase SQL objects from dbo to the Dad schema
author: GitHub Copilot
ms.date: 2026-06-30
ms.topic: how-to
keywords:
  - dadabase
  - azure sql
  - dacpac
  - sql schema
  - entity framework
estimated_reading_time: 12
---

## Executive Summary

Move the DadABase application-owned database objects from `dbo` to a dedicated
`Dad` schema with an in-place transfer during a maintenance window. The runbook
below provides an exact, copy-paste set of `ALTER SCHEMA ... TRANSFER` commands
for all DadABase tables and stored procedures currently modeled by the SQL
project, plus validation and permission re-grants.

This path preserves existing data in place and avoids a drop-and-reload step. The
tradeoff is that every dependency should be validated after transfer, and all app
code must already target `[Dad]` before traffic is reopened.

## Current State

The repository has one SQL database project and several application surfaces that
assume either `dbo` or the SQL Server default schema.

| Area | Current behavior | Migration effect |
|------|------------------|------------------|
| DACPAC project | `src/sql.database/sql.database.sqlproj` models application-owned database objects. | Point project includes at the `Dad` folder, add `Schemas/Dad.sql`, and add a pre-deploy cleanup script. |
| SQL tables | Table scripts define `Joke`, `JokeCategory`, `JokeJokeCategory`, and `JokeRating`. | Create tables under `[Dad]`. Drop legacy `[dbo]` tables during the offline cutover and import the export file afterward. |
| SQL modules | Stored procedures and `vw_Jokes` are created under `[dbo]` and contain unqualified table names. | Drop and recreate modules under `[Dad]`. Explicitly qualify internal references as `[Dad].[...]`. |
| Seed and patch scripts | `InsertDefaultData.sql`, `InsertDefaultData_v1.sql`, and patch scripts use unqualified or `[dbo]` names. | Qualify all permanent object references as `[Dad].[...]` and update `DBCC CHECKIDENT` names. |
| Web SQL repository | `JokeSQLRepository` calls `[dbo]` procedures and uses unqualified raw SQL table names. | Update stored procedure calls and raw SQL table references to `[Dad]`. |
| EF Core models | Entity classes use `[Table("Joke")]` and similar attributes without a schema. | Add `Schema = "Dad"` to application-owned entity attributes or configure explicit mappings in each relevant context. |
| Analyzer | `JokeDbContext` uses EF Core models without a schema. | Map analyzer entities to `Dad` so analysis writes and reads the same schema. |
| Connection strings | Current strings identify server and database only. | No schema is set in the connection string. Prefer explicit EF and SQL mapping over user default schema. |
| Permissions docs | Current examples grant broad database data roles or `EXECUTE ON SCHEMA::[dbo]`. | Use schema-scoped least privilege: DML plus execute on `SCHEMA::[Dad]` only. |

## Database Objects To Move

These objects are owned by the DadABase SQL project and should be moved to the
`Dad` schema.

| Object type | Object name | Source file |
|-------------|-------------|-------------|
| Schema | `Dad` | New SQL project script, such as `Schemas/Dad.sql` |
| Table | `[Dad].[Joke]` | `src/sql.database/Dad/Tables/Joke.sql` |
| Table | `[Dad].[JokeCategory]` | `src/sql.database/Dad/Tables/JokeCategory.sql` |
| Table | `[Dad].[JokeJokeCategory]` | `src/sql.database/Dad/Tables/JokeJokeCategory.sql` |
| Table | `[Dad].[JokeRating]` | `src/sql.database/Dad/Tables/JokeRating.sql` |
| View | `[Dad].[vw_Jokes]` | `src/sql.database/Dad/Views/vw_Jokes.sql` |
| Procedure | `[Dad].[usp_Get_Random_Joke]` | `src/sql.database/Dad/Stored Procedures/usp_Get_Random_Joke.sql` |
| Procedure | `[Dad].[usp_Joke_Reseed_Identities]` | `src/sql.database/Dad/Stored Procedures/usp_Joke_Reseed_Identities.sql` |
| Procedure | `[Dad].[usp_Joke_Search]` | `src/sql.database/Dad/Stored Procedures/usp_Joke_Search.sql` |
| Procedure | `[Dad].[usp_Joke_Import]` | `src/sql.database/Dad/Stored Procedures/usp_Joke_Import.sql` |
| Procedure | `[Dad].[usp_Joke_Update_ImageTxt]` | `src/sql.database/Dad/Stored Procedures/usp_Joke_Update_ImageTxt.sql` |

The database project does not currently define ASP.NET Identity tables. The
`ApplicationDbContext` exists in the web project and uses `IdentityDbContext`, but
there are no Identity table scripts in the DACPAC. Confirm whether Identity
tables exist in deployed databases before declaring that every database object has
moved. If they exist and should also leave `dbo`, plan that as a separate identity
schema decision because it has different application and migration risks.

## Important Platform Behavior

Several SQL Server and EF Core behaviors shape the implementation plan.

* `ALTER SCHEMA [Dad] TRANSFER [dbo].[Joke]` moves a table in place and preserves
  data, keys, indexes, and constraints.
* Dropping the legacy `[dbo]` tables is destructive. Validate the export file and
  keep a database backup or copy before the pre-deploy cleanup runs.
* When transferring procedures and views, validate module definitions afterward.
  If any definition still contains hardcoded `[dbo]` references, update and
  redeploy that module under `[Dad]`.
* `CREATE SCHEMA [Dad]` creates a database-level schema. The deployment principal
  needs permission to create schemas or needs `db_owner`.
* EF Core maps SQL Server entities to `dbo` by convention when no schema is
  specified. You can use `[Table("Joke", Schema = "Dad")]`, `.ToTable("Joke",
  schema: "Dad")`, or `modelBuilder.HasDefaultSchema("Dad")`.
* `HasDefaultSchema("Dad")` affects all relational objects in that model. Be
  careful with `ApplicationDbContext` because it inherits from `IdentityDbContext`
  and could also move Identity objects if migrations are used later.
* SqlPackage publish updates a target database to match the DACPAC model. Use a
  generated deploy script or deploy report for the first schema move, and keep the
  default data-loss blocking behavior enabled.
* Existing databases may have database-level change tracking enabled even though
  the DACPAC does not model it. Use `/p:ScriptDatabaseOptions=False` for script
  generation and publish so SqlPackage does not try to disable database-level
  change tracking before the pre-deploy cleanup runs.

## Recommended Migration Strategy

### Phase 1: Prepare The SQL Project

Add a schema script and change the DACPAC source model to `Dad`.

1. Add a schema definition script, for example `src/sql.database/Schemas/Dad.sql`:

    ```sql
    CREATE SCHEMA [Dad];
    GO
    ```

2. Rename the SQL project folder from `dbo` to `Dad`, or create a new `Dad`
   folder and move the object scripts into it.

3. Update `src/sql.database/sql.database.sqlproj`:

   ```xml
   <PreDeploy Include="Dad\Pre.Deployment.sql" />
   <PostDeploy Include="Dad\Post.Deployment.sql" />
   <Build Include="Schemas\Dad.sql" />
   <Build Include="Dad\Stored Procedures\usp_Get_Random_Joke.sql" />
   <Build Include="Dad\Stored Procedures\usp_Joke_Import.sql" />
  <Build Include="Dad\Stored Procedures\usp_Joke_Reseed_Identities.sql" />
   <Build Include="Dad\Stored Procedures\usp_Joke_Search.sql" />
   <Build Include="Dad\Stored Procedures\usp_Joke_Update_ImageTxt.sql" />
   <Build Include="Dad\Tables\Joke.sql" />
   <Build Include="Dad\Tables\JokeCategory.sql" />
   <Build Include="Dad\Tables\JokeJokeCategory.sql" />
   <Build Include="Dad\Tables\JokeRating.sql" />
   <Build Include="Dad\Views\vw_Jokes.sql" />
   ```

4. Change every SQL object definition from `[dbo]` to `[Dad]`.

5. Explicitly qualify all permanent table references inside the modules. For
   example, change `FROM Joke j` to `FROM [Dad].[Joke] j`, and change
   `INNER JOIN JokeCategory c` to `INNER JOIN [Dad].[JokeCategory] c`.

6. Update seed and patch scripts under `src/sql.database/Patch` so any script run
   through the pipeline targets `[Dad]` explicitly.

### Phase 2: Add An Offline Cleanup Pre-Deploy Script

The first deployment against an existing database needs a pre-deploy script that
drops the old `[dbo]` objects before the DACPAC creates `[Dad]` objects. This is
safe only because the migration assumes the export file is the source of truth
for restoring joke data.

Recommended behavior:

* Drop old `[dbo]` views and procedures first.
* Disable change tracking on old `[dbo]` tables if it is enabled, because SQL
  Server blocks database-level change tracking changes and table drops until
  tracked tables are disabled.
* Drop old `[dbo]` child tables before parent tables.
* Do not drop `[Dad]` tables in this script. Future deployments should preserve
  data already imported into the new schema.
* Keep the script idempotent so empty databases and repeat deployments are safe.

Example pattern:

```sql
IF OBJECT_ID(N'[dbo].[vw_Jokes]', N'V') IS NOT NULL
    DROP VIEW [dbo].[vw_Jokes];

IF OBJECT_ID(N'[dbo].[usp_Get_Random_Joke]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Get_Random_Joke];

IF OBJECT_ID(N'[dbo].[usp_Joke_Search]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Joke_Search];

IF OBJECT_ID(N'[dbo].[usp_Joke_Import]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Joke_Import];

IF OBJECT_ID(N'[dbo].[usp_Joke_Reseed_Identities]', N'P') IS NOT NULL
  DROP PROCEDURE [dbo].[usp_Joke_Reseed_Identities];

IF OBJECT_ID(N'[dbo].[usp_Joke_Update_ImageTxt]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Joke_Update_ImageTxt];

IF OBJECT_ID(N'[dbo].[JokeRating]', N'U') IS NOT NULL
  DROP TABLE [dbo].[JokeRating];

IF OBJECT_ID(N'[dbo].[JokeJokeCategory]', N'U') IS NOT NULL
  DROP TABLE [dbo].[JokeJokeCategory];

IF OBJECT_ID(N'[dbo].[Joke]', N'U') IS NOT NULL
  DROP TABLE [dbo].[Joke];

IF OBJECT_ID(N'[dbo].[JokeCategory]', N'U') IS NOT NULL
  DROP TABLE [dbo].[JokeCategory];
```

This script is the only SQL project script that should reference `[dbo]`, because
its job is to remove legacy objects from the old schema.

### Phase 3: Update Application Code

Update every runtime path that can read or write SQL Server data.

| Project | Change |
|---------|--------|
| `src/web/Data` | Add `Schema = "Dad"` to `Joke`, `JokeCategory`, `JokeJokeCategory`, and `JokeRating` table attributes, or map those entities explicitly in `DadABaseDbContext`. |
| `src/web/Data/Repositories/JokeSQLRepository.cs` | Change all `EXEC [dbo]` calls to `EXEC [Dad]`, and qualify raw SQL table names as `[Dad].[...]`. |
| `src/web/Data/Repositories/JokeSQLRepository.cs` | Update `ExportToSql` output so generated scripts target `[Dad]` and use `DBCC CHECKIDENT('[Dad].[Joke]', ...)`. |
| `src/web/Data/Repositories/JokeJsonRepository.cs` | Update generated SQL export output for the same `[Dad]` schema target, even though the repository itself is JSON-backed. |
| `src/web/Tests/RepositoryTests/Export_Repository_Tests.cs` | Update string assertions that currently expect unqualified table names. |
| `src/function/Entities/DatabaseTables` | Add schema metadata to the entity attributes so future SQL-backed Function data access maps to `[Dad]`. |
| `src/analyzer/Models` | Add schema metadata to the analyzer entity attributes or configure `JokeDbContext` mappings. |
| `src/web/Website/Models/DbContext/ProjectEntities.cs` and `src/function/Entities/DbContext/ProjectEntities.cs` | Update comments and grant examples from `SCHEMA::[dbo]` to `SCHEMA::[Dad]`. |

Prefer explicit mappings over relying on the database user's default schema. A
default schema is useful for ad hoc SQL, but it does not fix hardcoded `[dbo]`
references and it makes behavior depend on the connected principal.

### Phase 4: Update Permissions

The deployment principal can remain `db_owner` for DACPAC publishing if that is
the current operational model. The application identity should not use
`db_datareader` or `db_datawriter`, because those roles grant access across all
schemas in the database. Grant only the permissions needed on `SCHEMA::[Dad]`:

```sql
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Dad] TO [yourAppManagedIdentityName];
GRANT EXECUTE ON SCHEMA::[Dad] TO [yourAppManagedIdentityName];
```

If any legacy script remains unqualified during the transition, optionally set
the default schema for the application user:

```sql
ALTER USER [yourAppManagedIdentityName] WITH DEFAULT_SCHEMA = [Dad];
```

Do not use `DEFAULT_SCHEMA` as the main migration mechanism. It does not correct
hardcoded `[dbo]` object names, and different principals can resolve unqualified
names differently.

### Phase 5: Update Documentation

Update these documentation files after the code and SQL project changes are made:

| File | Required documentation change |
|------|-------------------------------|
| `Docs/Application-Architecture.md` | Change the SQL object table and SQL project schema from `dbo` to `Dad`. |
| `.github/instructions/sql-database-dacpac-instructions.md` | Change guidance that refers to the `dbo` folder and add a note about schema migrations requiring pre-deploy review. |
| `Docs/sql/README.md` | Replace the project structure and setup examples with `Dad` schema references. |
| `Docs/sql/CreateDatabase.sql` | If kept as a manual setup script, change all `[dbo]` references to `[Dad]` and create the schema first. |
| `Docs/sql/CreateJokeView.sql` | Change the view and table references to `[Dad]`. |
| `Docs/SQL-Permissions-Queries.md` | Add schema permission examples for `Dad`. |
| `.github/workflows-readme.md` | Update the app managed identity grant from `SCHEMA::[dbo]` to `SCHEMA::[Dad]`. |
| `src/sql.database/README.md` | Replace `dbo` folder and object examples with `Dad`. |

## Deployment Order

### Existing Database Migration Runbook (In-Place Transfer)

Run during a maintenance window after taking a full backup.

1. Ensure the target schema exists.

```sql
IF SCHEMA_ID(N'Dad') IS NULL
    EXEC(N'CREATE SCHEMA [Dad]');
```

2. Optional preflight inventory for all expected DadABase objects.

```sql
SELECT
    s.name AS SchemaName,
    o.type_desc,
    o.name AS ObjectName
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE o.name IN (
    N'Joke',
    N'JokeCategory',
    N'JokeJokeCategory',
    N'JokeRating',
    N'usp_Get_Random_Joke',
    N'usp_Joke_Reseed_Identities',
    N'usp_Joke_Search',
    N'usp_Joke_Import',
    N'usp_Joke_Update_ImageTxt',
    N'vw_Jokes')
ORDER BY o.type_desc, o.name, s.name;
```

3. Transfer application tables to the new schema.

```sql
ALTER SCHEMA [Dad] TRANSFER [dbo].[Joke];
ALTER SCHEMA [Dad] TRANSFER [dbo].[JokeCategory];
ALTER SCHEMA [Dad] TRANSFER [dbo].[JokeJokeCategory];
ALTER SCHEMA [Dad] TRANSFER [dbo].[JokeRating];
```

4. Transfer stored procedures used by the application.

```sql
ALTER SCHEMA [Dad] TRANSFER [dbo].[usp_Get_Random_Joke];
ALTER SCHEMA [Dad] TRANSFER [dbo].[usp_Joke_Reseed_Identities];
ALTER SCHEMA [Dad] TRANSFER [dbo].[usp_Joke_Search];
ALTER SCHEMA [Dad] TRANSFER [dbo].[usp_Joke_Import];
ALTER SCHEMA [Dad] TRANSFER [dbo].[usp_Joke_Update_ImageTxt];
```

5. Transfer the DadABase view.

```sql
ALTER SCHEMA [Dad] TRANSFER [dbo].[vw_Jokes];
```

6. Regrant runtime permissions on the target schema.

```sql
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Dad] TO [<app-role-or-user>];
GRANT EXECUTE ON SCHEMA::[Dad] TO [<app-role-or-user>];
```

7. Validate nothing from the DadABase object set remains in `dbo`.

```sql
SELECT
    s.name AS SchemaName,
    o.type_desc,
    o.name AS ObjectName
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE s.name = N'dbo'
  AND o.name IN (
      N'Joke',
      N'JokeCategory',
      N'JokeJokeCategory',
      N'JokeRating',
      N'usp_Get_Random_Joke',
      N'usp_Joke_Reseed_Identities',
      N'usp_Joke_Search',
      N'usp_Joke_Import',
      N'usp_Joke_Update_ImageTxt',
      N'vw_Jokes');
```

8. Rebuild the vs_Jokes.sql View so it points to the new table

9. Validate dependencies and runtime behavior.

```sql
SELECT OBJECT_SCHEMA_NAME(referencing_id) AS referencing_schema,
       OBJECT_NAME(referencing_id) AS referencing_object,
       referenced_schema_name,
       referenced_entity_name
FROM sys.sql_expression_dependencies
WHERE referenced_schema_name = N'dbo';

SELECT COUNT(*) AS JokeCount FROM [Dad].[Joke];
SELECT COUNT(*) AS CategoryCount FROM [Dad].[JokeCategory];
SELECT COUNT(*) AS JokeCategoryLinkCount FROM [Dad].[JokeJokeCategory];
SELECT COUNT(*) AS RatingCount FROM [Dad].[JokeRating];

EXEC [Dad].[usp_Get_Random_Joke];
EXEC [Dad].[usp_Joke_Search] @searchTxt = N'sun';
SELECT TOP 10 * FROM [Dad].[vw_Jokes];
```

#### Exact Object Coverage Notes

The command list above is generated from the current SQL project content in
`src/sql.database`.

* Tables transferred: `Joke`, `JokeCategory`, `JokeJokeCategory`, `JokeRating`
* Procedures transferred: `usp_Get_Random_Joke`, `usp_Joke_Search`,
  `usp_Joke_Import`, `usp_Joke_Reseed_Identities`, `usp_Joke_Update_ImageTxt`
* View transferred: `vw_Jokes`
* Suggested but not present in this app: any ASP.NET Identity tables (for
  example, `AspNetUsers`, `AspNetRoles`) and related Identity procedures.

Use the fresh environment path for a new database. Use the existing database
path only when the target already contains the old `dbo` DadABase objects.

## Fresh Environment Deployment Order

For an entirely new environment, there are no legacy `[dbo]` DadABase objects to
migrate. The same DACPAC can be used, and the pre-deploy cleanup script is safe
because it checks whether each legacy object exists before dropping it.

Use this shorter order for a fresh environment:

1. Provision the infrastructure and empty SQL database.

2. Build the DACPAC.

  ```powershell
  dotnet build .\src\sql.database\sql.database.sqlproj
  ```

3. Publish the DACPAC to the empty database.

  ```powershell
  SqlPackage /Action:Publish `
    /SourceFile:.\src\sql.database\bin\Debug\sql.database.dacpac `
    /TargetConnectionString:"Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;" `
    /p:ScriptDatabaseOptions=False
  ```

4. Load data if the environment needs starter or restored joke data. Use the app
  import flow, the generated export script, or the import stored procedure.

  ```sql
  EXEC [Dad].[usp_Joke_Import]
    @RemovePreviousJokes = 1,
    @tsvData = N'<exported TSV payload>';
  ```

5. Grant the runtime application identity schema-scoped access.

  ```sql
  CREATE USER [yourAppManagedIdentityName] FROM EXTERNAL PROVIDER;
  GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Dad] TO [yourAppManagedIdentityName];
  GRANT EXECUTE ON SCHEMA::[Dad] TO [yourAppManagedIdentityName];
  ```

6. Point the app connection string at the new database and run the app smoke
  tests.

You do not need a database copy, downtime window, export backup, or manual
`dbo` cleanup for a fresh environment. You only need data import if the new
environment should start with jokes instead of an empty schema.

## Existing Database Deployment Order

Use this order for the first deployment against an existing `dbo` database.

1. Create a backup or Azure SQL database copy.

  ```sql
  CREATE DATABASE DadABase_PreDadSchema AS COPY OF DadABase;
  ```

2. Run inventory queries in the target database.

  ```sql
  SELECT s.name AS SchemaName, o.type_desc, o.name AS ObjectName
  FROM sys.objects o
  INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
  WHERE o.name IN (
     N'Joke', N'JokeCategory', N'JokeJokeCategory', N'JokeRating',
     N'vw_Jokes', N'usp_Get_Random_Joke', N'usp_Joke_Search',
     N'usp_Joke_Import', N'usp_Joke_Update_ImageTxt')
  ORDER BY s.name, o.type_desc, o.name;
  ```

3. Build the updated DACPAC locally or in CI.

   ```powershell
   dotnet build .\src\sql.database\sql.database.sqlproj
   ```

4. Generate and review a deployment script before publishing to a shared
   environment.

   ```powershell
   SqlPackage /Action:Script `
     /SourceFile:.\src\sql.database\bin\Debug\sql.database.dacpac `
     /TargetConnectionString:"Server=tcp:<server>.database.windows.net,1433;Initial Catalog=DadABase;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;" `
     /p:ScriptDatabaseOptions=False `
     /DeployScriptPath:.\artifacts\dad-schema-deploy.sql `
     /DeployReportPath:.\artifacts\dad-schema-deploy-report.xml
   ```

5. Stop or drain the application. Downtime is acceptable for this migration.

6. Confirm the export file is accessible and contains the expected joke count.

7. Publish the DACPAC with the pre-deploy cleanup script included. The cleanup
  script intentionally drops legacy `[dbo]` objects, and the DACPAC creates the
  fresh `[Dad]` schema objects.

  ```powershell
  SqlPackage /Action:Publish `
    /SourceFile:.\src\sql.database\bin\Debug\sql.database.dacpac `
    /TargetConnectionString:"Server=localhost;Database=DadABase;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;" `
    /p:ScriptDatabaseOptions=False
  ```

8. Run the import using the exported backup file and the new import procedure.

  ```sql
  EXEC [Dad].[usp_Joke_Import]
     @RemovePreviousJokes = 1,
     @tsvData = N'<exported TSV payload>';
  ```

9. Grant the application identity least-privilege schema access.

  ```sql
  CREATE USER [yourAppManagedIdentityName] FROM EXTERNAL PROVIDER;
  GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Dad] TO [yourAppManagedIdentityName];
  GRANT EXECUTE ON SCHEMA::[Dad] TO [yourAppManagedIdentityName];
  ```

10. Run post-deployment validation queries.

   ```sql
   SELECT COUNT(*) AS JokeCount FROM [Dad].[Joke];
   SELECT COUNT(*) AS CategoryCount FROM [Dad].[JokeCategory];
   SELECT COUNT(*) AS JokeCategoryLinkCount FROM [Dad].[JokeJokeCategory];
   SELECT COUNT(*) AS RatingCount FROM [Dad].[JokeRating];

   EXEC [Dad].[usp_Get_Random_Joke];
   EXEC [Dad].[usp_Joke_Search] @searchTxt = 'sun';
   SELECT TOP 10 * FROM [Dad].[vw_Jokes];
   ```

11. Deploy the application changes that call `[Dad]`.

12. Run application smoke tests with `DataSource=SQL` and a real database
   connection string.

## Downtime Cutover

Use a coordinated offline release:

1. Stop or drain the application.
2. Deploy the database schema migration.
3. Import the exported data into `[Dad]`.
4. Deploy the application changes.
5. Run smoke tests and reopen traffic.

No rolling compatibility bridge is required because downtime is acceptable. Do
not create temporary `dbo` wrappers or synonyms for this path.

## Validation Checklist

Use this checklist before merging the implementation.

* The SQL project builds and includes `Dad` schema objects in the DACPAC.
* The export file has been validated before the legacy tables are dropped.
* The generated deployment script drops legacy `[dbo]` objects only through the
  planned pre-deploy cleanup path.
* `sys.objects` shows application-owned objects under `Dad` after deployment.
* The application managed identity has DML and execute permissions on `Dad` and
  is not a member of `db_datareader` or `db_datawriter`.
* `JokeSQLRepository` no longer contains `[dbo]` or unqualified permanent table
  names in raw SQL.
* Entity attributes or fluent mappings explicitly target `Dad`.
* Generated export SQL targets `[Dad]` and tests assert that output.
* The analyzer can connect and read/write the same `Dad` tables.
* Documentation and permission snippets no longer instruct users to grant execute
  on `SCHEMA::[dbo]` for DadABase objects.

Recommended local checks:

```powershell
dotnet build .\src\sql.database\sql.database.sqlproj
dotnet build .\src\web\Website\DadABase.Web.csproj
dotnet test .\src\web\Tests\DadABase.Tests.csproj
```

## Rollback Plan

The safest rollback is to restore or copy back from the pre-migration database
backup. This migration intentionally drops legacy tables, so rollback should not
depend on transferring tables back from `[Dad]` to `[dbo]`.

High-level rollback actions:

1. Stop the application or disable writes.
2. Restore the pre-migration database backup or copy.
3. Redeploy the previous DACPAC and application version.
4. Reapply previous permissions if the restore target requires them.
5. Validate row counts and stored procedure behavior.

## Open Decisions

Resolve these before implementation starts.

* Confirm whether any deployed database contains ASP.NET Identity tables and
  whether those tables should remain in `dbo` or move under a separate schema.
* Confirm the exported backup file format, location, and validation procedure for
  the production cutover.
* Decide whether old historical patch scripts should remain runnable against
  `[Dad]` or be archived as pre-`Dad` migration history.

## References

* [ALTER SCHEMA (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-schema-transact-sql)
* [CREATE SCHEMA (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-schema-transact-sql)
* [GRANT Schema Permissions (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/grant-schema-permissions-transact-sql)
* [SqlPackage Publish parameters and properties](https://learn.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage-publish)
* [EF Core entity type schema mapping](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types#table-schema)