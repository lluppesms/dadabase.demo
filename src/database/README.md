# DadABase Database Project

This is a SQL Server Database Project for the DadABase application. It contains the schema definition for storing dad jokes.

## Project Structure

```
DadABase.Database/
├── Tables/
│   ├── Joke.sql              # Main jokes table
│   ├── JokeCategory.sql      # Joke categories
│   └── JokeRating.sql        # User ratings for jokes
├── Views/
│   └── vw_Jokes.sql          # Simplified view of active jokes
├── Scripts/
│   └── PostDeployment/
│       └── Script.PostDeployment.sql  # Post-deployment script
└── DadABase.Database.sqlproj # SQL Server Database Project file
```

## Database Schema

### Tables

**Joke**
- Primary table storing joke text, category, rating, and metadata
- Fields: JokeId, JokeTxt, JokeCategoryId, JokeCategoryTxt, Attribution, ImageTxt, SortOrderNbr, Rating, VoteCount, ActiveInd, CreateDateTime, CreateUserName, ChangeDateTime, ChangeUserName

**JokeCategory**
- Stores joke category definitions
- Fields: JokeCategoryId, JokeCategoryTxt, SortOrderNbr, ActiveInd, CreateDateTime, CreateUserName, ChangeDateTime, ChangeUserName

**JokeRating**
- Stores individual user ratings for jokes
- Fields: JokeRatingId, JokeId, UserRating, CreateDateTime, CreateUserName

### Views

**vw_Jokes**
- Simplified view showing active jokes with key information
- Ordered by category and joke text

## Building the Project

### Using SQL Server Data Tools (SSDT)

1. Open the solution in Visual Studio
2. Build the DadABase.Database project
3. This will create a DACPAC file in the bin folder

### Using SqlPackage.exe

```bash
# Build the project (creates .dacpac)
dotnet build src/database/DadABase.Database.sqlproj

# Publish to a database
SqlPackage.exe /Action:Publish \
  /SourceFile:src/database/bin/Debug/DadABase.Database.dacpac \
  /TargetConnectionString:"Server=yourserver;Database=DadABase;Trusted_Connection=True;"
```

### Using SQL Database Projects SDK

This project uses the Microsoft.Build.Sql SDK which allows building with .NET tooling:

```bash
dotnet build src/database/DadABase.Database.sqlproj
```

## Deploying the Database

### Option 1: Using SSDT in Visual Studio
1. Right-click the project
2. Select "Publish"
3. Configure your target database connection
4. Click "Publish"

### Option 2: Using SqlPackage CLI

```bash
SqlPackage /Action:Publish \
  /SourceFile:DadABase.Database.dacpac \
  /TargetServerName:yourserver \
  /TargetDatabaseName:DadABase
```

### Option 3: Using Azure DevOps or GitHub Actions

The project can be integrated into CI/CD pipelines using SqlPackage tasks.

## Populating with Data

After deploying the database schema, populate it with jokes:

1. Navigate to the `Docs/sql` folder
2. Run `InsertDefaultData.sql` to populate the database with sample jokes

The script includes approximately 3,000+ dad jokes across various categories.

## Reusability

This database project is designed to be reusable across different applications:

- **Web Application**: Used by the main web app in `src/web`
- **Function App**: Can be used by Azure Functions in `src/function`  
- **Console App**: Can be used by console applications in `src/console`
- **MCP Services**: Can be used by Model Context Protocol services in `src/mcp`

## Connection Strings

Example connection strings for different environments:

**LocalDB (Development)**
```
Server=(localdb)\\mssqllocaldb;Database=DadABase;Trusted_Connection=True;MultipleActiveResultSets=true
```

**Azure SQL Database**
```
Server=tcp:yourserver.database.windows.net,1433;Database=DadABase;User ID=yourusername;Password=yourpassword;Encrypt=True;
```

**SQL Server (Windows Auth)**
```
Server=yourserver;Database=DadABase;Trusted_Connection=True;MultipleActiveResultSets=true
```

## Entity Framework Integration

This database schema is designed to work with Entity Framework Core. The corresponding C# models are located in:
- `src/web/Website/Models/DatabaseTables/`

The models include:
- `Joke.cs`
- `JokeCategory.cs`
- `JokeRating.cs`

## Version History

- **v1.0** - Initial database schema with Joke, JokeCategory, and JokeRating tables
- Support for active/inactive jokes via ActiveInd flag
- Foreign key relationships with cascade rules
- Default constraints for audit fields

## Support

For issues or questions about the database schema, please refer to the main repository documentation at `/Docs/sql/README.md`.
