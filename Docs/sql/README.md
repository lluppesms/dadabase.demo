# SQL Database Setup for DadABase

## Overview

The DadABase web application has been refactored to use a SQL Server database instead of JSON files for storing jokes. The application supports both SQL Server database mode and a JSON file fallback mode for scenarios where a database is not available.

## Configuration Modes

The application automatically determines which data source to use based on the presence of a connection string:

- **SQL Server Mode** (Primary): When a connection string is configured, jokes are stored in a SQL Server database
- **JSON File Mode** (Fallback): When no connection string is configured, jokes are read from a JSON file

For details on how to configure and switch between modes, see: [Database Fallback Configuration](../DATABASE-FALLBACK.md)

## Database Project

The database schema is now maintained as a separate, reusable SQL Server Database Project following industry best practices and the pattern from the reference repository.

**Location**: `/src/sql.database/`

This project can be used by any application in the repository (web app, functions, console, MCP services).

### Database Project Structure

```
src/sql.database/
├── sql.database/
│   ├── dbo/
│   │   ├── Tables/
│   │   │   ├── Joke.sql              # Main jokes table
│   │   │   ├── JokeCategory.sql      # Joke categories
│   │   │   └── JokeRating.sql        # User ratings
│   │   ├── Views/
│   │   │   └── vw_Jokes.sql          # Simplified view of active jokes
│   │   └── Post.Deployment.sql       # Post-deployment script
│   ├── Patch/                         # Patch scripts for updates
│   ├── sql.database.sqlproj          # SQL Server Database Project
│   └── README.md                      # Detailed project documentation
├── sql.database.sln                   # Visual Studio solution
└── .gitignore                         # Version control exclusions
```

For detailed information about the database project, see: [`/src/sql.database/sql.database/README.md`](../../src/sql.database/sql.database/README.md)

## Bicep Infrastructure

SQL Server infrastructure is defined in Bicep modules at `/infra/Bicep/modules/database/`:

- **sqlserver.bicep** - Azure SQL Server and Database configuration with:
  - Serverless compute tier for cost optimization
  - Azure AD authentication support
  - Diagnostic logging integration
  - Firewall rules for Azure services
  - Auto-pause configuration

Deploy infrastructure before deploying schema:

```bash
az deployment group create \
  --resource-group rg-dadabase-dev \
  --template-file infra/Bicep/main.bicep \
  --parameters sqlDatabaseName=DadABase
```

## Database Schema

The database consists of three main tables:

1. **Joke** - Stores individual jokes with their text, category, rating, and metadata
2. **JokeCategory** - Stores joke category definitions
3. **JokeRating** - Stores individual user ratings for jokes

Additionally, there is a view:
- **vw_Jokes** - Provides a simplified view of active jokes

## Setup Instructions

### Option 1: Using the Database Project (Recommended)

#### Build DACPAC

```bash
# Using Visual Studio
# Open sql.database.sln and build the project

# OR using MSBuild
cd src/sql.database/sql.database
msbuild sql.database.sqlproj /p:Configuration=Release
```

#### Deploy DACPAC

```bash
SqlPackage.exe /Action:Publish \
  /SourceFile:bin/Release/sql.database.dacpac \
  /TargetServerName:yourserver.database.windows.net \
  /TargetDatabaseName:DadABase \
  /TargetUser:yourusername \
  /TargetPassword:yourpassword
```

For Visual Studio deployment:
1. Right-click on `sql.database.sqlproj` in Solution Explorer
2. Select "Publish"
3. Configure your target database connection
4. Click "Publish"

### Option 2: Using SQL Scripts Directly

Run the SQL scripts in this folder in the following order:

1. `CreateDatabase.sql` - Creates the tables and their relationships
2. `CreateJokeView.sql` - Creates the vw_Jokes view
3. `InsertDefaultData.sql` - Populates the database with default jokes

### 2. Configure Connection String

Update the connection string in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DadABase;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

For production environments, you can also set the connection string via:
- Environment variable: `ConnectionStrings__DefaultConnection`
- Azure App Configuration
- Azure Key Vault

### 3. Database Providers Supported

The application uses Entity Framework Core and supports:
- Azure SQL Database (recommended for production)
- SQL Server (on-premises or VM)
- LocalDB (for development)

## Deployment Patterns

The project follows the SQL DACPAC deployment pattern from the reference repository:

1. **Build Phase**: Compile the SQL project into a DACPAC file
2. **Infrastructure Phase**: Deploy Azure SQL Server using Bicep templates
3. **Schema Phase**: Deploy DACPAC to the database
4. **Data Phase**: Run post-deployment scripts to populate data

This pattern supports:
- Multiple environments (DEV, QA, PROD)
- Automated CI/CD pipelines
- Schema version control
- Rollback capabilities

## Code Changes

### Key Changes Made

1. **Added SQL Server EF Core Package**
   - `Microsoft.EntityFrameworkCore.SqlServer` v10.0.1

2. **Updated ApplicationDbContext**
   - Added DbSets for Joke, JokeCategory, and JokeRating tables
   - Location: `src/web/Website/Models/DbContext/ApplicationDbContext.cs`

3. **Refactored JokeRepository**
   - Replaced JSON file reading with Entity Framework queries
   - Changed from Singleton to Scoped service (required for EF Core)
   - Implemented previously missing GetOne() method
   - Uses EF.Functions.Like for case-insensitive searches
   - Location: `src/web/Website/Repositories/JokeRepository.cs`

4. **Updated Program.cs**
   - Added DbContext configuration with connection string
   - Changed IJokeRepository registration to Scoped

5. **Updated Tests**
   - Tests now use in-memory database for isolated testing
   - No external database required for running tests

## Migration from JSON to SQL

If you have existing jokes in JSON format that you want to migrate to SQL:

1. Create the database using the scripts provided or deploy the DACPAC
2. The `InsertDefaultData.sql` script already contains all jokes from the original JSON file
3. Simply run the script to populate your database

## Reusability

The database project in `/src/sql.database/` is designed to be reused across different applications:

- **Web Application**: Main web app (`src/web`)
- **Function App**: Azure Functions (`src/function`)
- **Console App**: Console applications (`src/console`)
- **MCP Services**: Model Context Protocol services (`src/mcp`)

All applications can reference the same database schema definition and DACPAC for deployment.

## Troubleshooting

### Connection Issues

If you encounter connection issues:
- Verify SQL Server is running
- Check the connection string format
- Ensure LocalDB is installed (for development)
- Verify network connectivity (for remote databases)
- Check firewall rules on Azure SQL Server

### Missing Data

If no jokes appear:
- Verify the database was created successfully
- Check that `InsertDefaultData.sql` was executed or run the post-deployment data population
- Verify the ActiveInd field is set to 'Y' for active jokes

### DACPAC Deployment Issues

If DACPAC deployment fails:
- Verify SqlPackage.exe is installed
- Check target database permissions
- Review deployment logs for specific errors
- Ensure target server allows connections from your IP

## Performance Considerations

- The application uses Entity Framework Core for database access
- Queries are optimized using IQueryable for deferred execution
- Azure SQL Database serverless tier provides cost-effective scaling
- Consider adding indexes on frequently queried columns (JokeCategoryTxt, ActiveInd)
- For production, configure appropriate DTU/vCore sizing based on load

## Future Enhancements

Potential areas for future development:
- Implement Add/Update/Delete operations for jokes (currently commented out)
- Add database migrations for schema versioning using EF Core migrations
- Implement caching layer for frequently accessed jokes
- Add full-text search capabilities
- Implement stored procedures for complex queries
- Add database-level audit logging
