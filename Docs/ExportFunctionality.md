# Export Functionality

## Overview
The export functionality allows users to download all jokes and categories from the database as a SQL file. The exported file is in the same format as the `InsertDefaultData.sql` file and can be used to restore or transfer the joke collection.

## How to Use

### Via Web UI
1. Navigate to the **Export** page using the navigation menu
2. Click the **Download SQL Export** button
3. The SQL file will be downloaded with a timestamped filename (e.g., `JokeExport_20250115_150530.sql`)

## Technical Details

### Implementation Approach
The export functionality uses a direct repository call pattern, similar to the [travel.tracker](https://github.com/lluppesms/travel.tracker) application:
- The `Export.razor` page directly injects and calls `IJokeRepository.ExportToSql()`
- The generated SQL content is streamed to the browser using `DotNetStreamReference`
- Uses the existing `downloadFileFromStream` JavaScript function for file downloads
- No API endpoint is required

### File Format
The exported SQL file includes:
- Header with export date/time
- Temporary table declaration (`@tmpJokes`)
- INSERT statements for all jokes with their categories
- Cleanup statements (optional - controlled by `@RemovePreviousJokes` variable)
- INSERT statements for categories, jokes, and junction table mappings

### Multi-Category Support
Jokes can belong to multiple categories. The export properly handles this by:
1. Exporting each joke-category combination as a separate row
2. Using the `JokeJokeCategory` junction table for mapping
3. Generating INSERT statements that recreate these relationships

### SQL Quote Escaping
Single quotes in joke text and attributions are properly escaped using SQL's double-quote convention (`''`).

### Data Source Support
The export functionality works with both:
- **SQL Database**: Uses stored procedures to fetch jokes with categories
- **JSON File**: Reads from the Jokes.json file when database is unavailable

## Example Export Structure
```sql
------------------------------------------------------------------------------------------------------------------------
-- Exported Joke Data
-- Export Date: 2025-01-15 15:05:30 UTC
------------------------------------------------------------------------------------------------------------------------
Declare @RemovePreviousJokes varchar(1) = 'Y'
Declare @tmpJokes Table (
  JokeId int identity(1,1),
  JokeTxt nvarchar(max),
  JokeCategoryTxt nvarchar(500),
  Attribution nvarchar(500),
  ImageTxt nvarchar(max)
)
...
INSERT INTO @tmpJokes (JokeCategoryTxt, JokeTxt, Attribution) VALUES
 ('Bad Puns', 'A bicycle can''t stand on its own because it''s two-tired.', NULL),
 ('Bad Puns', 'A boiled egg is hard to beat.', NULL),
 ...
```

## Excluded Data
The export **does not include** data from the `JokeRatings` table, as this is user-specific rating data that should not be transferred between environments.

## Testing
The export functionality includes comprehensive unit tests that verify:
- Repository generates non-empty SQL content
- Valid SQL structure with all required elements
- Proper quote escaping
- Support for multiple categories per joke
