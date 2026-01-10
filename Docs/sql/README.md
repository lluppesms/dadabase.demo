# SQL Database Setup for DadABase

## Overview

The DadABase web application has been refactored to use a SQL Server database instead of JSON files for storing jokes.

## Database Schema

The database consists of three main tables:

1. **Joke** - Stores individual jokes with their text, category, rating, and metadata
2. **JokeCategory** - Stores joke category definitions
3. **JokeRating** - Stores individual user ratings for jokes

Additionally, there is a view:
- **vw_Jokes** - Provides a simplified view of jokes ordered by category and text

## Setup Instructions

### 1. Create the Database

Run the SQL scripts in the following order:

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
- SQL Server (default)
- Azure SQL Database
- LocalDB (for development)

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

1. Create the database using the scripts provided
2. The `InsertDefaultData.sql` script already contains all jokes from the original JSON file
3. Simply run the script to populate your database

## Troubleshooting

### Connection Issues

If you encounter connection issues:
- Verify SQL Server is running
- Check the connection string format
- Ensure LocalDB is installed (for development)
- Verify network connectivity (for remote databases)

### Missing Data

If no jokes appear:
- Verify the database was created successfully
- Check that `InsertDefaultData.sql` was executed
- Verify the ActiveInd field is set to 'Y' for active jokes

## Performance Considerations

- The application uses Entity Framework Core for database access
- Queries are optimized using IQueryable for deferred execution
- Consider adding indexes on frequently queried columns (JokeCategoryTxt, ActiveInd)
- For production, consider using a connection pooling strategy

## Future Enhancements

Potential areas for future development:
- Implement Add/Update/Delete operations for jokes (currently commented out)
- Add database migrations for schema versioning
- Implement caching layer for frequently accessed jokes
- Add full-text search capabilities
