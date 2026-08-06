# Database Fallback Configuration

## Overview

The application supports two modes of operation for joke data storage:

1. **SQL Server Database** (Primary mode)
2. **JSON File** (Fallback mode)

## Configuration

The application automatically determines which mode to use based on the presence of a connection string in the application settings.

### SQL Server Mode (Default)

To use SQL Server database for joke storage, provide a connection string in `applicationSettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=DadABase;Authentication=Active Directory Interactive;"
  }
}
```

Or via environment variable:
```bash
export ConnectionStrings__DefaultConnection="Server=.;Database=DadABase;Integrated Security=true;"
```

When a connection string is configured:
- The application uses `DadABaseDbContext` with Entity Framework Core
- Jokes are stored in SQL Server database
- Uses `JokeRepository` for data access
- Supports all SQL Server features (stored procedures, views, etc.)

### JSON File Mode (Fallback)

To use JSON file-based joke storage, either:
- Remove the `DefaultConnection` from `ConnectionStrings` section
- Set it to an empty string or null
- Don't define the `ConnectionStrings` section at all

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

Or:
```json
{
  "ConnectionStrings": {}
}
```

When no connection string is configured:
- The application uses `JokeJsonRepository`
- Jokes are read from `Data/Jokes.json` file
- No database required
- Read-only access to jokes
- Ideal for development, testing, or offline scenarios

## Benefits of Fallback Mode

1. **No Database Required**: Run the application without setting up SQL Server
2. **Offline Development**: Work without network access to database
3. **Quick Testing**: Rapid application startup without database overhead
4. **Portability**: Easily deploy without database dependencies
5. **Demos**: Perfect for demonstrations or proof-of-concepts

## Switching Between Modes

Simply modify the connection string in `applicationSettings.json` and restart the application. No code changes required.

### Example: Switch to JSON Mode

Comment out or remove the connection string:
```json
{
  "ConnectionStrings": {
    // "DefaultConnection": "Server=.;Database=DadABase;..."
  }
}
```

### Example: Switch to SQL Mode

Add the connection string back:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=DadABase;Authentication=Active Directory Interactive;"
  }
}
```

## Data Location

- **SQL Mode**: Data stored in database tables (`Joke`, `JokeCategory`, `JokeRating`)
- **JSON Mode**: Data read from `/Website/Data/Jokes.json`

## Implementation Details

The mode selection is handled in `Program.cs`:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useDatabase = !string.IsNullOrEmpty(connectionString);

if (useDatabase)
{
    // SQL Server mode
    builder.Services.AddDbContext<DadABaseDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddScoped<IJokeRepository, JokeRepository>();
}
else
{
    // JSON file mode
    var jsonFilePath = Path.Combine(Path.GetDirectoryName(
        System.Reflection.Assembly.GetExecutingAssembly().Location), 
        "Data/Jokes.json");
    builder.Services.AddSingleton<IJokeRepository>(sp => 
        new JokeJsonRepository(jsonFilePath));
}
```

Both implementations use the same `IJokeRepository` interface, ensuring seamless operation regardless of the data source.
