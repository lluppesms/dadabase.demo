---
title: DadABase SQL Database Project
description: SQL database project structure, deployment, and schema notes for DadABase
author: DadABase maintainers
ms.date: 2026-05-14
ms.topic: reference
keywords:
  - dadabase
  - sqlproj
  - dacpac
estimated_reading_time: 7
---

## DadABase SQL Database Project

This is a SQL Server Database Project for the DadABase application. It contains the schema definition for storing dad jokes, following the structure and deployment patterns from the reference repository.

## Project Structure

```text
sql.database/
├── Dad/
│   ├── Tables/
│   │   ├── Joke.sql              # Main jokes table
│   │   ├── JokeCategory.sql      # Joke categories
│   │   ├── JokeJokeCategory.sql  # Joke/category relationships
│   │   └── JokeRating.sql        # User ratings for jokes
│   ├── Views/
│   │   └── vw_Jokes.sql          # Simplified view of active jokes
│   ├── Pre.Deployment.sql        # Drops legacy dbo objects for offline migration
│   └── Post.Deployment.sql       # Post-deployment script
├── Schemas/
│   └── Dad.sql                   # Dad schema definition
├── Patch/                         # Patch scripts for updates
└── sql.database.sqlproj          # SQL Server Database Project file
```

## Database Schema

### Tables

**Joke**
- Primary table storing joke text, rating, and metadata
- Fields: JokeId, JokeTxt, Attribution, ImageTxt, SortOrderNbr, Rating, VoteCount, ActiveInd, CreateDateTime, CreateUserName, ChangeDateTime, ChangeUserName
- CHECK constraint: ActiveInd IN ('Y', 'N')

**JokeCategory**
- Stores joke category definitions
- Fields: JokeCategoryId, JokeCategoryTxt, SortOrderNbr, ActiveInd, CreateDateTime, CreateUserName, ChangeDateTime, ChangeUserName
- CHECK constraint: ActiveInd IN ('Y', 'N')

**JokeJokeCategory**
- Stores many-to-many relationships between jokes and categories
- Fields: JokeId, JokeCategoryId, CreateDateTime, CreateUserName
- Foreign keys: Dad.Joke and Dad.JokeCategory

**JokeRating**
- Stores individual user ratings for jokes
- Fields: JokeRatingId, JokeId, UserRating, CreateDateTime, CreateUserName
- CHECK constraint: UserRating BETWEEN 1 AND 5

### Views

**vw_Jokes**
- Simplified view showing active jokes with key information
- Filters to ActiveInd = 'Y'

## Building the Project

### Using Visual Studio

1. Open the solution in Visual Studio with SQL Server Data Tools (SSDT)
2. Build the sql.database project
3. This will create a DACPAC file in the bin folder

### Using MSBuild

```bash
msbuild sql.database.sqlproj /p:Configuration=Release
```

### Using SQL Server Data Tools Build

```bash
# Navigate to project directory
cd src/sql.database

# Build the project
SqlPackage.exe /Action:Build /SourceFile:sql.database.sqlproj
```

## Deploying the Database

### Option 1: Using SqlPackage CLI

```bash
SqlPackage.exe /Action:Publish \
  /SourceFile:bin/Release/sql.database.dacpac \
  /TargetConnectionString:"Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=DadABase;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;" \
  /p:ScriptDatabaseOptions=False
```

For a fresh environment, publish the DACPAC to an empty database. The pre-deploy
script is idempotent and does nothing when the legacy `dbo` DadABase objects are
not present. Load starter data only if the environment should begin with jokes.

### Option 2: Using Visual Studio SSDT

1. Right-click the project in Solution Explorer
2. Select "Publish"
3. Configure your target database connection
4. Click "Publish"

### Option 3: Using Azure DevOps Pipeline

The project includes pipeline templates for automated deployment:
- Build DACPAC artifact
- Deploy to Azure SQL Database
- Support for multiple environments (DEV, QA, PROD)

See the `.azdo/pipelines/` folder for deployment pipeline examples.

## Populating with Data

After deploying the database schema, populate it with jokes:

1. Navigate to the `/Docs/sql` folder
2. Run `InsertDefaultData.sql` to populate the database with sample jokes

The script includes approximately 3,000+ dad jokes across various categories.

## Bicep Infrastructure

The database infrastructure is defined in Bicep templates at `/infra/Bicep/`:

- `sqlserver.bicep` - Azure SQL Server and Database configuration
- `main.bicep` - Main deployment orchestration

Deploy infrastructure before deploying schema:

```bash
az deployment group create \
  --resource-group rg-dadabase-dev \
  --template-file infra/Bicep/main.bicep \
  --parameters sqlDatabaseName=DadABase
```

## Reusability

This database project is designed to be reusable across different applications:

- **Web Application**: Used by the main web app in `src/web`
- **Function App**: Can be used by Azure Functions in `src/function`  
- **Console App**: Can be used by console applications in `src/console`
- **MCP Services**: Can be used by Model Context Protocol services in `src/mcp`

## Connection Strings

Example connection strings for different environments:

**Azure SQL Database**

``` sql
data source=tcp:<databaseServerName>.database.windows.net,1433;Database=YourBase;Authentication=Active Directory Default;Connection Timeout=120;
```

**LocalDB (Development)**

``` sql
Server=(localdb)\\mssqllocaldb;Database=DadABase;Trusted_Connection=True;MultipleActiveResultSets=true
```

## Entity Framework Integration

This database schema is designed to work with Entity Framework Core. The corresponding C# models are located in:
- `src/web/Website/Models/DatabaseTables/`

The models include:
- `Joke.cs`
- `JokeCategory.cs`
- `JokeRating.cs`

## Patch Scripts

The `Patch/` folder is for database update scripts that need to be run manually or as part of a specific deployment:
- Use naming convention: `Patch-YYYYMMDD.sql`
- These are not automatically executed during DACPAC deployment
- Useful for data migrations, one-time updates, or breaking changes

## Version History

- **v1.0** - Initial database schema with Joke, JokeCategory, and JokeRating tables
- Support for active/inactive jokes via ActiveInd flag
- Foreign key relationships with cascade rules
- Default constraints for audit fields
- CHECK constraints for data integrity

## Support

For issues or questions about the database schema, please refer to:
- Main repository documentation at `/Docs/sql/README.md`
- Bicep infrastructure templates at `/infra/Bicep/`
