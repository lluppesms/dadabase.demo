//-----------------------------------------------------------------------
// <copyright file="JokeRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Repository
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;
using System.Threading;

namespace DadABase.Data.Repositories;

/// <summary>
/// Represents the Entity Framework Core implementation of the joke repository.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="JokeSQLRepository"/> class.
/// </remarks>
/// <param name="context">The database context to utilize.</param>
[ExcludeFromCodeCoverage]
public class JokeSQLRepository(DadABaseDbContext context) : IJokeRepository
{
    private readonly DadABaseDbContext _context = context;

    /// <summary>
    /// Gets a random joke from the database.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the joke.</param>
    /// <returns>A random <see cref="Joke"/> record.</returns>
    public Joke GetRandomJoke(string requestingUserName = "ANON")
    {
        var joke = _context.Jokes
            .FromSqlRaw("EXEC [dbo].[usp_Get_Random_Joke]")
            .AsEnumerable()
            .FirstOrDefault();

        return joke ?? new Joke("No jokes here!");
    }

    /// <summary>
    /// Finds matching jokes by search text and category.
    /// </summary>
    /// <param name="searchTxt">The partial text to search for.</param>
    /// <param name="jokeCategoryTxt">The specified category text filter.</param>
    /// <param name="requestingUserName">The username of the user requesting the search.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of matching <see cref="Joke"/> records.</returns>
    public IQueryable<Joke> SearchJokes(string searchTxt = "", string jokeCategoryTxt = "", string requestingUserName = "ANON")
    {
        jokeCategoryTxt = jokeCategoryTxt.Equals("All", StringComparison.OrdinalIgnoreCase) ? string.Empty : jokeCategoryTxt;

        // If user supplied NEITHER category NOR search term - get a random joke
        if (string.IsNullOrEmpty(jokeCategoryTxt) && string.IsNullOrEmpty(searchTxt))
        {
            var randomJoke = GetRandomJoke();
            return new List<Joke> { randomJoke }.AsQueryable();
        }

        // Use stored procedure for search with optional category and search text parameters
        // See: https://learn.microsoft.com/en-us/ef/core/querying/sql-queries?tabs=sqlserver
        //   While this syntax may look like regular C# string interpolation, the supplied value is wrapped in a
        //   DbParameter and the generated parameter name inserted where the {0} placeholder was specified.
        //   This makes FromSql safe from SQL injection attacks, and sends the value efficiently and correctly to the database.
        var categoryParam = string.IsNullOrEmpty(jokeCategoryTxt) ? null : jokeCategoryTxt;
        var searchParam = string.IsNullOrEmpty(searchTxt) ? null : searchTxt;
        var jokes = _context.Jokes
            .FromSqlInterpolated($"EXEC [dbo].[usp_Joke_Search] @category = {categoryParam}, @searchTxt = {searchParam}")
            .AsEnumerable()
            .AsQueryable();

        return jokes;
    }

    /// <summary>
    /// Lists all jokes with their categories populated.
    /// </summary>
    /// <param name="activeInd">The active indicator, typically "Y" or "N".</param>
    /// <param name="requestingUserName">The username of the user requesting the list.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of jokes.</returns>
    public IQueryable<Joke> ListAll(string activeInd = "Y", string requestingUserName = "ANON")
    {
        // Use raw SQL to include Categories from the many-to-many relationship
        var jokes = _context.Jokes
            .FromSqlInterpolated($@"
                SELECT j.JokeId, 
                    STUFF((SELECT ', ' + c.JokeCategoryTxt
                           FROM JokeJokeCategory jjc
                           INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
                           WHERE jjc.JokeId = j.JokeId
                           ORDER BY c.JokeCategoryTxt
                           FOR XML PATH('')), 1, 2, '') AS Categories,
                    j.JokeTxt, j.ImageTxt, j.Rating, j.ActiveInd, j.Attribution, j.VoteCount, j.SortOrderNbr,
                    j.CreateDateTime, j.CreateUserName, j.ChangeDateTime, j.ChangeUserName
                FROM Joke j
                WHERE j.ActiveInd = {activeInd}")
            .AsEnumerable()
            .AsQueryable();

        return jokes;
    }

    /// <summary>
    /// Returns the most recently added active jokes, ordered by creation date descending and limited to <paramref name="count"/> records.
    /// </summary>
    /// <param name="count">The maximum number of jokes to return. The default is 100.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of the most recent <see cref="Joke"/> records.</returns>
    public IQueryable<Joke> GetRecentAdditions(int count = 100)
    {
        var jokes = _context.Jokes
            .FromSqlInterpolated($@"
                SELECT TOP ({count}) j.JokeId,
                    STUFF((SELECT ', ' + c.JokeCategoryTxt
                           FROM JokeJokeCategory jjc
                           INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
                           WHERE jjc.JokeId = j.JokeId
                           ORDER BY c.JokeCategoryTxt
                           FOR XML PATH('')), 1, 2, '') AS Categories,
                    j.JokeTxt, j.ImageTxt, j.Rating, j.ActiveInd, j.Attribution, j.VoteCount, j.SortOrderNbr,
                    j.CreateDateTime, j.CreateUserName, j.ChangeDateTime, j.ChangeUserName
                FROM Joke j
                WHERE j.ActiveInd = 'Y'
                ORDER BY j.CreateDateTime DESC")
            .AsEnumerable()
            .AsQueryable();

        return jokes;
    }

    /// <summary>
    /// Updates the image text field for a specific joke.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke.</param>
    /// <param name="imageTxt">The image description text.</param>
    /// <param name="requestingUserName">The username performing the update.</param>
    /// <returns><see langword="true" /> if the update succeeded; otherwise, <see langword="false" />.</returns>
    public bool UpdateImageTxt(int jokeId, string imageTxt, string requestingUserName = "ANON")
    {
        try
        {
            _context.Database.ExecuteSqlInterpolated($"EXEC [dbo].[usp_Joke_Update_ImageTxt] @jokeId = {jokeId}, @imageTxt = {imageTxt}");
            return true;
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error updating ImageTxt for JokeId {jokeId}: {msg}");
            return false;
        }
    }

    /// <summary>
    /// Gets all joke categories from the database.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the categories.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of category names.</returns>
    public IQueryable<string> GetJokeCategories(string requestingUserName)
    {
        return _context.JokeCategories
            .Where(c => c.JokeCategoryTxt != null)
            .Select(c => c.JokeCategoryTxt!)
            .OrderBy(c => c);
    }

    /// <summary>
    /// Gets one specific joke record with its categories populated.
    /// </summary>
    /// <param name="id">The identifier of the joke.</param>
    /// <param name="requestingUserName">The username of the user requesting the joke.</param>
    /// <returns>The specified <see cref="Joke"/> object.</returns>
    public Joke GetOne(int id, string requestingUserName = "ANON")
    {
        // Use raw SQL to include Categories from the many-to-many relationship
        var joke = _context.Jokes
            .FromSqlInterpolated($@"
                SELECT j.JokeId, 
                    STUFF((SELECT ', ' + c.JokeCategoryTxt
                           FROM JokeJokeCategory jjc
                           INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
                           WHERE jjc.JokeId = j.JokeId
                           ORDER BY c.JokeCategoryTxt
                           FOR XML PATH('')), 1, 2, '') AS Categories,
                    j.JokeTxt, j.ImageTxt, j.Rating, j.ActiveInd, j.Attribution, j.VoteCount, j.SortOrderNbr,
                    j.CreateDateTime, j.CreateUserName, j.ChangeDateTime, j.ChangeUserName
                FROM Joke j
                WHERE j.JokeId = {id}")
            .AsEnumerable()
            .FirstOrDefault();

        return joke ?? new Joke("Joke not found!");
    }

    //// --------------------------------------------------------------------------------------------------------------
    ////  NOT IMPLEMENTED YET!
    //// --------------------------------------------------------------------------------------------------------------
    //public bool DupCheck(int keyValue, string dscr, ref string fieldName, ref string errorMessage)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Add(Joke Joke, string requestingUserName = "ANON")
    //{
    //    throw new NotImplementedException();
    //}

    //public bool DeleteCheck(int id, ref string errorMessage, string requestingUserName = "ANON")
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Delete(int id, string requestingUserName = "ANON")
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Save(int id, Joke joke, string requestingUserName = "ANON")
    //{
    //    throw new NotImplementedException();
    //}

    //public decimal AddRating(JokeRating jokeRating, string requestingUserName = "ANON")
    //{
    //    throw new NotImplementedException();
    //}

    ///// <summary>
    ///// Export Data
    ///// </summary>
    ///// <returns>Success</returns>
    //public bool ExportData(string fileName)
    //{
    //    using (var r = new StreamReader(sourceFileName))
    //    {
    //        var json = r.ReadToEnd();
    //        using (var w = new StreamWriter(fileName))
    //        {
    //            w.Write(json);
    //        }
    //    }
    //    return true;
    //}

    ///// <summary>
    ///// Import Data
    ///// </summary>
    ///// <returns>Success</returns>
    //public bool ImportData(string data)
    //{
    //    throw new NotImplementedException();
    //    // -- this *should* work, but hasn't been tested and we'd had to put in a file upload capability...
    //    //using (var w = new StreamWriter(sourceFileName))
    //    //{
    //    //    w.Write(data);
    //    //}
    //    //return true;
    //}

    /// <summary>
    /// Exports all jokes and categories to an SQL script format.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the export.</param>
    /// <returns>A string containing the SQL script content.</returns>
    public string ExportToSql(string requestingUserName = "ANON")
    {
        var sb = new System.Text.StringBuilder();
        const int maxRowsPerInsert = 1000;

        // Header
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("-- Exported Joke Data");
        sb.AppendLine($"-- Export Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("Declare @RemovePreviousJokes varchar(1) = 'Y'");
        sb.AppendLine();
        sb.AppendLine("-- Temp table for unique jokes");
        sb.AppendLine("Declare @tmpJokes Table (");
        sb.AppendLine("  JokeId int,");
        sb.AppendLine("  Categories nvarchar(500),  -- For documentation/cross-reference only");
        sb.AppendLine("  JokeTxt nvarchar(max),");
        sb.AppendLine("  Attribution nvarchar(500),");
        sb.AppendLine("  ImageTxt nvarchar(max)");
        sb.AppendLine(")");
        sb.AppendLine();
        sb.AppendLine("-- Temp table for joke-category relationships");
        sb.AppendLine("Declare @tmpJokeCategories Table (");
        sb.AppendLine("  JokeId int,");
        sb.AppendLine("  JokeCategoryTxt nvarchar(500)");
        sb.AppendLine(")");
        sb.AppendLine();
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("-- Insert Jokes (each joke appears once)");
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");

        // Get all jokes with their categories
        var jokesQuery = _context.Jokes
            .FromSqlInterpolated($@"
                SELECT j.JokeId, 
                    STUFF((SELECT ', ' + c.JokeCategoryTxt
                           FROM JokeJokeCategory jjc
                           INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
                           WHERE jjc.JokeId = j.JokeId
                           ORDER BY c.JokeCategoryTxt
                           FOR XML PATH('')), 1, 2, '') AS Categories,
                    j.JokeTxt, j.ImageTxt, j.Rating, j.ActiveInd, j.Attribution, j.VoteCount, j.SortOrderNbr,
                    j.CreateDateTime, j.CreateUserName, j.ChangeDateTime, j.ChangeUserName
                FROM Joke j
                WHERE j.ActiveInd = 'Y'
                ORDER BY Categories, j.JokeTxt")
            .AsNoTracking()
            .AsEnumerable()
            .ToList();

        // Insert unique jokes into @tmpJokes (with 1000 row limit per INSERT)
        var rowCount = 0;
        var isFirst = true;
        foreach (var joke in jokesQuery)
        {
            if (rowCount % maxRowsPerInsert == 0)
            {
                if (rowCount > 0)
                {
                    sb.AppendLine();
                }
                sb.AppendLine("INSERT INTO @tmpJokes (JokeId, Categories, JokeTxt, Attribution, ImageTxt) VALUES");
                isFirst = true;
            }

            if (!isFirst)
            {
                sb.AppendLine(",");
            }
            isFirst = false;

            var jokeTxt = EscapeSqlString(joke.JokeTxt ?? string.Empty).Trim();
            var categories = joke.Categories != null ? $"'{EscapeSqlString(joke.Categories)}'".Trim() : "'Random'";
            var attribution = joke.Attribution != null ? $"'{EscapeSqlString(joke.Attribution)}'".Trim() : "NULL";
            var imageTxt = joke.ImageTxt != null ? $"'{EscapeSqlString(joke.ImageTxt)}'".Trim().Replace("\n", "") : "NULL";

            sb.Append($" ({joke.JokeId}, {categories}, '{jokeTxt}', {attribution}, {imageTxt})");
            rowCount++;
        }

        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("-- Insert Joke-Category relationships (one row per joke-category pair)");
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");

        // Build joke-category relationships by splitting categories for each joke (with 1000 row limit per INSERT)
        rowCount = 0;
        isFirst = true;
        foreach (var joke in jokesQuery)
        {
            var categories = (joke.Categories ?? "Unknown").Split(',').Select(c => c.Trim()).Distinct();

            foreach (var category in categories)
            {
                if (rowCount % maxRowsPerInsert == 0)
                {
                    if (rowCount > 0)
                    {
                        sb.AppendLine();
                    }
                    sb.AppendLine("INSERT INTO @tmpJokeCategories (JokeId, JokeCategoryTxt) VALUES");
                    isFirst = true;
                }

                if (!isFirst)
                {
                    sb.AppendLine(",");
                }
                isFirst = false;

                var categoryTxt = category != null ? EscapeSqlString(category) : "'Random'";
                sb.Append($" ({joke.JokeId}, '{categoryTxt}')");
                rowCount++;
            }
        }

        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("-- END Data Inserts");
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine();
        sb.AppendLine("IF @RemovePreviousJokes = 'Y'");
        sb.AppendLine("BEGIN");
        sb.AppendLine("  PRINT ''");
        sb.AppendLine("  PRINT 'Removing previous set of jokes...'");
        sb.AppendLine("  DELETE FROM JokeRating");
        sb.AppendLine("  DELETE FROM JokeJokeCategory");
        sb.AppendLine("  DELETE FROM JokeCategory");
        sb.AppendLine("  DELETE FROM Joke");
        sb.AppendLine("  DBCC CHECKIDENT('JokeRating', RESEED, 0)");
        sb.AppendLine("  DBCC CHECKIDENT('JokeCategory', RESEED, 0)");
        sb.AppendLine("  DBCC CHECKIDENT('Joke', RESEED, 0)");
        sb.AppendLine("END");
        sb.AppendLine();
        sb.AppendLine("DECLARE @CategoryCount int");
        sb.AppendLine("SELECT @CategoryCount = Count(DISTINCT JokeCategoryTxt) From @tmpJokeCategories");
        sb.AppendLine("PRINT ''");
        sb.AppendLine("PRINT 'Inserting ' + CAST(@CategoryCount as varchar) + ' fresh categories...'");
        sb.AppendLine("INSERT INTO JokeCategory (JokeCategoryTxt) ");
        sb.AppendLine("  SELECT DISTINCT JokeCategoryTxt From @tmpJokeCategories Where JokeCategoryTxt NOT IN (Select JokeCategoryTxt From JokeCategory)");
        sb.AppendLine();
        sb.AppendLine("DECLARE @JokeCount int");
        sb.AppendLine("SELECT @JokeCount = Count(*) From @tmpJokes");
        sb.AppendLine("PRINT ''");
        sb.AppendLine("PRINT 'Inserting ' + CAST(@JokeCount as varchar) + ' fresh jokes...'");
        sb.AppendLine("INSERT INTO Joke (JokeTxt, Attribution, ImageTxt, Rating, VoteCount) ");
        sb.AppendLine("  SELECT j.JokeTxt, j.Attribution, j.ImageTxt, 0, 0");
        sb.AppendLine("  FROM @tmpJokes j");
        sb.AppendLine("  WHERE j.JokeTxt NOT IN (Select JokeTxt From Joke)");
        sb.AppendLine("  ORDER BY j.Categories, j.JokeTxt");
        sb.AppendLine();
        sb.AppendLine("PRINT ''");
        sb.AppendLine("PRINT 'Populating JokeJokeCategory junction table...'");
        sb.AppendLine("INSERT INTO JokeJokeCategory (JokeId, JokeCategoryId)");
        sb.AppendLine("  SELECT DISTINCT jk.JokeId, c.JokeCategoryId");
        sb.AppendLine("  FROM Joke jk");
        sb.AppendLine("  INNER JOIN @tmpJokes tj ON jk.JokeTxt = tj.JokeTxt");
        sb.AppendLine("  INNER JOIN @tmpJokeCategories tjc ON tj.JokeId = tjc.JokeId");
        sb.AppendLine("  INNER JOIN JokeCategory c ON tjc.JokeCategoryTxt = c.JokeCategoryTxt");
        sb.AppendLine("  WHERE NOT EXISTS (");
        sb.AppendLine("    SELECT 1 FROM JokeJokeCategory jjc ");
        sb.AppendLine("    WHERE jjc.JokeId = jk.JokeId AND jjc.JokeCategoryId = c.JokeCategoryId");
        sb.AppendLine("  )");
        sb.AppendLine();
        sb.AppendLine("PRINT ''");
        sb.AppendLine("PRINT 'Displaying All Jokes...'");
        sb.AppendLine("SELECT j.JokeId, ");
        sb.AppendLine("  STUFF((SELECT ', ' + c.JokeCategoryTxt");
        sb.AppendLine("         FROM JokeJokeCategory jjc");
        sb.AppendLine("         INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId");
        sb.AppendLine("         WHERE jjc.JokeId = j.JokeId");
        sb.AppendLine("         ORDER BY c.JokeCategoryTxt");
        sb.AppendLine("         FOR XML PATH('')), 1, 2, '') AS Categories,");
        sb.AppendLine("  j.JokeTxt, j.Attribution, j.ImageTxt, j.Rating, j.CreateDateTime ");
        sb.AppendLine("FROM Joke j ");
        sb.AppendLine("ORDER BY j.JokeId, Categories, j.JokeTxt");

        return sb.ToString();
    }

    /// <summary>
    /// Escapes SQL string literals by handling single quotes.
    /// </summary>
    /// <param name="input">The input string to escape.</param>
    /// <returns>The escaped SQL string.</returns>
    private static string EscapeSqlString(string input)
    {
        return input?.Replace("'", "''") ?? string.Empty;
    }

    /// <summary>
    /// Sanitizes a field value for inclusion in a tab-separated file by replacing tabs and newlines.
    /// </summary>
    /// <param name="input">The raw field value.</param>
    /// <returns>A sanitized string safe for use in a TSV field.</returns>
    private static string EscapeTabField(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("\t", " ").Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
    }

    /// <summary>
    /// Exports all active jokes to a tab-delimited text format with fields:
    /// JokeId, Categories, JokeTxt, ImageTxt, Attribution, ActiveInd, SortOrderNbr, Rating, VoteCount.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the export.</param>
    /// <returns>A string containing the tab-delimited content with a header row.</returns>
    public string ExportToTabDelimited(string requestingUserName = "ANON")
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine("JokeId\tCategories\tJokeTxt\tImageTxt\tAttribution\tActiveInd\tSortOrderNbr\tRating\tVoteCount");

        // Get all active jokes with their comma-separated categories via the search stored proc
        // Calling with NULL params returns all jokes; the proc's correlated subquery aggregates ALL categories per joke
        var categoryParam = (string?)null;
        var searchParam = (string?)null;
        var jokes = _context.Jokes
            .FromSqlInterpolated($"EXEC [dbo].[usp_Joke_Search] @category = {categoryParam}, @searchTxt = {searchParam}")
            .AsNoTracking()
            .AsEnumerable()
            .ToList();

        foreach (var joke in jokes)
        {
            sb.AppendLine($"{joke.JokeId}\t{EscapeTabField(joke.Categories)}\t{EscapeTabField(joke.JokeTxt)}\t{EscapeTabField(joke.ImageTxt)}\t{EscapeTabField(joke.Attribution)}\t{EscapeTabField(joke.ActiveInd)}\t{joke.SortOrderNbr}\t{joke.Rating ?? 0}\t{joke.VoteCount ?? 0}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports all active jokes to a JSON string. Each joke object contains the fields
    /// JokeId, Categories, JokeTxt, ImageTxt, Attribution, ActiveInd, SortOrderNbr, Rating, VoteCount.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the export.</param>
    /// <returns>A JSON array string with indented formatting.</returns>
    public string ExportToJson(string requestingUserName = "ANON")
    {
        var categoryParam = (string?)null;
        var searchParam = (string?)null;
        var jokes = _context.Jokes
            .FromSqlInterpolated($"EXEC [dbo].[usp_Joke_Search] @category = {categoryParam}, @searchTxt = {searchParam}")
            .AsNoTracking()
            .AsEnumerable()
            .ToList();

        var projected = jokes.Select(joke => new
        {
            joke.JokeId,
            Categories = joke.Categories ?? string.Empty,
            JokeTxt = joke.JokeTxt ?? string.Empty,
            ImageTxt = joke.ImageTxt ?? string.Empty,
            Attribution = joke.Attribution ?? string.Empty,
            ActiveInd = joke.ActiveInd ?? "Y",
            joke.SortOrderNbr,
            Rating = joke.Rating ?? 0,
            VoteCount = joke.VoteCount ?? 0
        });

        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        return System.Text.Json.JsonSerializer.Serialize(projected, options);
    }

    /// <summary>
    /// Imports jokes from a tab-delimited text string. Parses the rows in C#, ensures all
    /// referenced categories exist, then inserts new jokes and their category associations
    /// using the existing repository methods.
    /// </summary>
    /// <param name="tabData">The tab-delimited content including a header row.</param>
    /// <param name="requestingUserName">The username of the user performing the import.</param>
    /// <returns>A tuple with success flag, count of newly inserted jokes, and a status message.</returns>
    /// <remarks>Deprecated: use <see cref="ImportFromTabDelimitedViaSproc"/> instead.</remarks>
    [Obsolete("Use ImportFromTabDelimitedViaSproc instead, which delegates batch import to usp_Joke_Import.")]
    public (bool Success, int ImportedCount, string Message) ImportFromTabDelimited(string tabData, string requestingUserName = "ANON")
    {
        try
        {
            var jokeRows = ParseTabDelimitedData(tabData);
            if (jokeRows.Count == 0)
            {
                return (false, 0, "No valid joke data found in the import file.");
            }

            var importedCount = 0;
            var now = DateTime.UtcNow;

            // Load existing joke texts once to check for duplicates
            var existingJokeTexts = _context.Jokes
                .Where(j => j.JokeTxt != null)
                .Select(j => j.JokeTxt)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Load existing categories (name -> id) once; filter out any entries with null keys
            var existingCategories = _context.JokeCategories
                .Where(c => c.JokeCategoryTxt != null)
                .ToDictionary(c => c.JokeCategoryTxt!, c => c.JokeCategoryId, StringComparer.OrdinalIgnoreCase);

            // Collect all new category names referenced in the import that do not yet exist,
            // then insert them in a single SaveChanges call to minimize round-trips.
            var allImportCategoryNames = jokeRows
                .SelectMany(r => (r.Categories ?? string.Empty).Split(','))
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c) && !existingCategories.ContainsKey(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (allImportCategoryNames.Count > 0)
            {
                foreach (var catName in allImportCategoryNames)
                {
                    var newCategory = new JokeCategory
                    {
                        JokeCategoryTxt = catName,
                        ActiveInd = "Y",
                        SortOrderNbr = 50,
                        CreateDateTime = now,
                        CreateUserName = requestingUserName,
                        ChangeDateTime = now,
                        ChangeUserName = requestingUserName
                    };
                    _context.JokeCategories.Add(newCategory);
                }
                _context.SaveChanges();

                // Refresh the category dictionary to include the newly inserted IDs
                foreach (var cat in _context.JokeCategories.Where(c => c.JokeCategoryTxt != null && allImportCategoryNames.Contains(c.JokeCategoryTxt)))
                {
                    existingCategories[cat.JokeCategoryTxt!] = cat.JokeCategoryId;
                }
            }

            foreach (var row in jokeRows)
            {
                if (string.IsNullOrWhiteSpace(row.JokeTxt)) continue;
                if (existingJokeTexts.Contains(row.JokeTxt)) continue;

                // Resolve category IDs for this joke
                var categoryIds = (row.Categories ?? string.Empty)
                    .Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c) && existingCategories.ContainsKey(c))
                    .Select(c => existingCategories[c])
                    .Distinct()
                    .ToList();

                var joke = new Joke
                {
                    JokeTxt = row.JokeTxt,
                    Attribution = row.Attribution,
                    ImageTxt = row.ImageTxt,
                    SortOrderNbr = 50,
                    ActiveInd = "Y",
                    Rating = row.Rating ?? 0,
                    VoteCount = row.VoteCount ?? 0,
                    CreateDateTime = now,
                    CreateUserName = requestingUserName,
                    ChangeDateTime = now,
                    ChangeUserName = requestingUserName
                };

                var newJokeId = AddJoke(joke, requestingUserName);
                if (newJokeId > 0)
                {
                    if (categoryIds.Count > 0)
                    {
                        UpdateJokeCategories(newJokeId, categoryIds, requestingUserName);
                    }

                    existingJokeTexts.Add(row.JokeTxt);
                    importedCount++;
                }
            }

            return (true, importedCount, $"Successfully imported {importedCount} new joke(s) from {jokeRows.Count} record(s) in the file.");
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error importing jokes: {msg}");
            return (false, 0, $"Error importing jokes: {msg}");
        }
    }

    /// <summary>
    /// Represents a single row parsed from a tab-delimited joke import file.
    /// </summary>
    private sealed record JokeImportRow(
        string JokeTxt,
        string Categories,
        string Attribution,
        string ImageTxt,
        decimal? Rating,
        int? VoteCount);

    /// <summary>
    /// Parses tab-delimited joke data into a list of <see cref="JokeImportRow"/> records.
    /// The expected column order is: JokeId, Categories, JokeTxt, ImageTxt, Attribution, Rating, VoteCount.
    /// A leading header row (starting with "JokeId") is automatically skipped.
    /// </summary>
    /// <param name="tabData">The raw tab-delimited text.</param>
    /// <returns>A list of parsed <see cref="JokeImportRow"/> records with non-empty joke text.</returns>
    private static List<JokeImportRow> ParseTabDelimitedData(string tabData)
    {
        var rows = new List<JokeImportRow>();
        var lines = tabData.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        var headerSkipped = false;

        foreach (var line in lines)
        {
            // Skip the header row (identified by starting with "JokeId")
            if (!headerSkipped)
            {
                headerSkipped = true;
                if (line.TrimStart().StartsWith("JokeId", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            var fields = line.Split('\t');
            if (fields.Length < 3) continue;

            // Column order: JokeId(0), Categories(1), JokeTxt(2), ImageTxt(3), Attribution(4), Rating(5), VoteCount(6)
            var jokeTxt = fields[2].Trim();
            if (string.IsNullOrWhiteSpace(jokeTxt)) continue;

            rows.Add(new JokeImportRow(
                JokeTxt: jokeTxt,
                Categories: fields.Length > 1 ? fields[1].Trim() : string.Empty,
                Attribution: fields.Length > 4 && !string.IsNullOrWhiteSpace(fields[4]) ? fields[4].Trim() : string.Empty,
                ImageTxt: fields.Length > 3 && !string.IsNullOrWhiteSpace(fields[3]) ? fields[3].Trim() : string.Empty,
                Rating: fields.Length > 5 && decimal.TryParse(fields[5].Trim(), out var rating) ? rating : null,
                VoteCount: fields.Length > 6 && int.TryParse(fields[6].Trim(), out var voteCount) ? voteCount : null));
        }

        return rows;
    }

    /// <summary>
    /// Converts a JSON array of joke objects into a 9-column tab-delimited string compatible with
    /// <c>usp_Joke_Import</c>: JokeId, Categories, JokeTxt, ImageTxt, Attribution, ActiveInd,
    /// SortOrderNbr, Rating, VoteCount.
    /// </summary>
    /// <param name="jsonData">A JSON array string produced by <see cref="ExportToJson"/>.</param>
    /// <returns>A 9-column tab-delimited string for import, or an empty string if input is empty or malformed.</returns>
    public static string ConvertJsonToTabDelimited(string jsonData)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            return string.Empty;
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(jsonData);
            var sb = new StringBuilder();
            sb.AppendLine("JokeId\tCategories\tJokeTxt\tImageTxt\tAttribution\tActiveInd\tSortOrderNbr\tRating\tVoteCount");

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var jokeId = element.TryGetProperty("JokeId", out var idProp) ? idProp.GetInt32() : 0;
                var categories = element.TryGetProperty("Categories", out var catProp) ? catProp.GetString() : string.Empty;
                var jokeTxt = element.TryGetProperty("JokeTxt", out var txtProp) ? txtProp.GetString() : string.Empty;
                var imageTxt = element.TryGetProperty("ImageTxt", out var imgProp) ? imgProp.GetString() : string.Empty;
                var attribution = element.TryGetProperty("Attribution", out var attrProp) ? attrProp.GetString() : string.Empty;
                var activeInd = element.TryGetProperty("ActiveInd", out var aiProp) ? aiProp.GetString() : "Y";
                var sortOrderNbr = element.TryGetProperty("SortOrderNbr", out var soProp) ? soProp.GetInt32() : 50;
                var rating = element.TryGetProperty("Rating", out var ratProp) ? ratProp.GetDecimal() : 0;
                var voteCount = element.TryGetProperty("VoteCount", out var vcProp) ? vcProp.GetInt32() : 0;

                sb.AppendLine($"{jokeId}\t{EscapeTabField(categories)}\t{EscapeTabField(jokeTxt)}\t{EscapeTabField(imageTxt)}\t{EscapeTabField(attribution)}\t{EscapeTabField(activeInd)}\t{sortOrderNbr}\t{rating}\t{voteCount}");
            }

            return sb.ToString();
        }
        catch (System.Text.Json.JsonException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Normalizes a tab-delimited string for import by upgrading old 7-column exports
    /// (JokeId, Categories, JokeTxt, ImageTxt, Attribution, Rating, VoteCount) to the
    /// 9-column format expected by <c>usp_Joke_Import</c> (adds ActiveInd = "Y" and
    /// SortOrderNbr = 50 as defaults). Files already in 9-column format are returned as-is.
    /// </summary>
    /// <param name="tabData">The raw tab-delimited content (7 or 9 columns).</param>
    /// <returns>A 9-column tab-delimited string safe for the import stored procedure.</returns>
    public static string NormalizeTabDelimitedForImport(string tabData)
    {
        if (string.IsNullOrWhiteSpace(tabData))
        {
            return tabData;
        }

        var lines = tabData.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        if (lines.Length == 0)
        {
            return tabData;
        }

        // Check if the header already has 9 columns (contains ActiveInd) — no upgrade needed
        var headerFields = lines[0].Split('\t');
        if (headerFields.Any(f => f.Trim().Equals("ActiveInd", StringComparison.OrdinalIgnoreCase)))
        {
            return tabData;
        }

        // Old 7-column format: insert ActiveInd and SortOrderNbr after Attribution (index 5)
        var sb = new StringBuilder();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Preserve trailing empty lines
            if (string.IsNullOrEmpty(line))
            {
                if (i < lines.Length - 1) sb.AppendLine();
                continue;
            }

            var fields = line.Split('\t');
            if (fields.Length >= 7)
            {
                // Insert "ActiveInd"/"SortOrderNbr" header or "Y"/"50" defaults at position 5
                var upgraded = new List<string>(fields.Take(5));
                upgraded.Add(i == 0 && fields[0].TrimStart().StartsWith("JokeId", StringComparison.OrdinalIgnoreCase) ? "ActiveInd" : "Y");
                upgraded.Add(i == 0 && fields[0].TrimStart().StartsWith("JokeId", StringComparison.OrdinalIgnoreCase) ? "SortOrderNbr" : "50");
                upgraded.AddRange(fields.Skip(5));
                sb.AppendLine(string.Join("\t", upgraded));
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Imports jokes from a tab-delimited text string by passing the raw TSV to the
    /// <c>usp_Joke_Import</c> stored procedure, which parses and batch-inserts data entirely in SQL.
    /// New categories are inserted, duplicate jokes (by text) are skipped, and category associations
    /// are created — all in a single database round-trip.
    /// </summary>
    /// <param name="tabData">The tab-delimited content including a header row.</param>
    /// <param name="removePreviousJokes">When <see langword="true"/>, all existing jokes, categories, and ratings are deleted and identity columns are reseeded before importing.</param>
    /// <param name="requestingUserName">The username of the user performing the import.</param>
    /// <returns>A tuple with success flag, count of newly inserted jokes, and a status message.</returns>
    public (bool Success, int ImportedCount, string Message) ImportFromTabDelimitedViaSproc(string tabData, bool removePreviousJokes = false, string requestingUserName = "ANON")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tabData))
            {
                return (false, 0, "No data provided for import.");
            }

            // Count non-header, non-empty lines so we can include the total in the message
            var totalLines = tabData
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Count(l => !string.IsNullOrWhiteSpace(l)
                    && !l.TrimStart().StartsWith("JokeId", StringComparison.OrdinalIgnoreCase));

            if (totalLines == 0)
            {
                return (false, 0, "No valid joke data found in the import file.");
            }

            // Normalise line endings to LF so STRING_SPLIT(…, CHAR(10)) works consistently
            var normalised = tabData.Replace("\r\n", "\n").Replace("\r", "\n");

            var importedCount = 0;
            using var command = _context.Database.GetDbConnection().CreateCommand();
            if (command.Connection.State != System.Data.ConnectionState.Open)
            {
                _context.Database.OpenConnection();
            }
            command.CommandText = "EXEC [dbo].[usp_Joke_Import] @tsvData, @RemovePreviousJokes";
            command.CommandType = System.Data.CommandType.Text;

            var paramTsv = command.CreateParameter();
            paramTsv.ParameterName = "@tsvData";
            paramTsv.Value = normalised;
            command.Parameters.Add(paramTsv);

            var paramReplace = command.CreateParameter();
            paramReplace.ParameterName = "@RemovePreviousJokes";
            paramReplace.Value = removePreviousJokes ? 1 : 0;
            command.Parameters.Add(paramReplace);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                importedCount = reader.GetInt32(0);
            }

            var actionMsg = removePreviousJokes
                ? $"Successfully replaced all existing jokes with {importedCount} joke(s) from the file."
                : $"Successfully imported {importedCount} new joke(s) from {totalLines} record(s) in the file.";
            return (true, importedCount, actionMsg);
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error importing jokes via stored procedure: {msg}");
            return (false, 0, $"Error importing jokes: {msg}");
        }
    }

    /// <summary>
    /// Updates an existing joke.
    /// </summary>
    /// <param name="joke">The joke entity containing the updated information.</param>
    /// <param name="requestingUserName">The username of the user requesting the update.</param>
    /// <returns><see langword="true"/> if the update was successful; otherwise, <see langword="false"/>.</returns>
    public bool UpdateJoke(Joke joke, string requestingUserName = "ANON")
    {
        try
        {
            var existingJoke = _context.Jokes.Find(joke.JokeId);
            if (existingJoke == null)
            {
                return false;
            }

            existingJoke.JokeTxt = joke.JokeTxt;
            existingJoke.Attribution = joke.Attribution;
            existingJoke.ImageTxt = joke.ImageTxt;
            existingJoke.ActiveInd = "Y";
            existingJoke.SortOrderNbr = joke.SortOrderNbr;
            existingJoke.ChangeDateTime = DateTime.UtcNow;
            existingJoke.ChangeUserName = requestingUserName;

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error updating joke {joke.JokeId}: {msg}");
            return false;
        }
    }

    /// <summary>
    /// Gets all active joke categories.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the categories.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of <see cref="JokeCategory"/> entities.</returns>
    public IQueryable<JokeCategory> GetAllCategories(string requestingUserName = "ANON")
    {
        return _context.JokeCategories
            .Where(c => c.ActiveInd == "Y")
            .OrderBy(c => c.JokeCategoryTxt);
    }

    /// <summary>
    /// Updates the categories associated with a joke.
    /// </summary>
    /// <param name="jokeId">The ID of the joke to update.</param>
    /// <param name="categoryIds">The list of category IDs to associate with the joke.</param>
    /// <param name="requestingUserName">The username of the user requesting the update.</param>
    /// <returns><see langword="true"/> if the update was successful; otherwise, <see langword="false"/>.</returns>
    public bool UpdateJokeCategories(int jokeId, List<int> categoryIds, string requestingUserName = "ANON")
    {
        try
        {
            // Remove existing categories
            var existingCategories = _context.JokeJokeCategories.Where(jjc => jjc.JokeId == jokeId);
            _context.JokeJokeCategories.RemoveRange(existingCategories);

            // Add new categories
            foreach (var categoryId in categoryIds)
            {
                _context.JokeJokeCategories.Add(new JokeJokeCategory
                {
                    JokeId = jokeId,
                    JokeCategoryId = categoryId,
                    CreateDateTime = DateTime.UtcNow,
                    CreateUserName = requestingUserName
                });
            }

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error updating categories for joke {jokeId}: {msg}");
            return false;
        }
    }

    /// <summary>
    /// Adds a new joke.
    /// </summary>
    /// <param name="joke">The joke entity to add.</param>
    /// <param name="requestingUserName">The username of the user requesting the add operation.</param>
    /// <returns>The identifier of the newly created joke; otherwise, -1 if the operation fails.</returns>
    public int AddJoke(Joke joke, string requestingUserName = "ANON")
    {
        try
        {
            var now = DateTime.UtcNow;

            // Use raw SQL to insert the joke and return the new ID
            // This avoids the Categories column issue (Categories is a computed property, not a real column)
            var result = _context.Jokes
                .FromSqlInterpolated($@"
                    INSERT INTO Joke (JokeTxt, Attribution, ImageTxt, ActiveInd, SortOrderNbr, Rating, VoteCount,
                                      CreateDateTime, CreateUserName, ChangeDateTime, ChangeUserName)
                    OUTPUT INSERTED.JokeId, INSERTED.JokeTxt, INSERTED.Attribution, INSERTED.ImageTxt, 
                           INSERTED.ActiveInd, INSERTED.SortOrderNbr, INSERTED.Rating, INSERTED.VoteCount,
                           INSERTED.CreateDateTime, INSERTED.CreateUserName, INSERTED.ChangeDateTime, INSERTED.ChangeUserName,
                           NULL AS Categories
                    VALUES ({joke.JokeTxt}, {joke.Attribution}, {joke.ImageTxt}, 'Y', {joke.SortOrderNbr}, 0, 0,
                            {now}, {requestingUserName}, {now}, {requestingUserName})")
                .AsEnumerable()
                .FirstOrDefault();

            return result?.JokeId ?? -1;
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error adding joke: {msg}");
            return -1;
        }
    }

    /// <summary>
    /// Deletes an existing joke.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke to delete.</param>
    /// <param name="requestingUserName">The username of the user requesting the delete operation.</param>
    /// <returns><see langword="true"/> if the delete was successful; otherwise, <see langword="false"/>.</returns>
    public bool DeleteJoke(int jokeId, string requestingUserName = "ANON")
    {
        try
        {
            // Then, find and remove the joke (JokeJokeCategories will be cascade deleted)
            var joke = _context.Jokes.Find(jokeId);
            if (joke == null)
            {
                return false;
            }
            _context.Jokes.Remove(joke);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error deleting joke {jokeId}: {msg}");
            return false;
        }
    }

    /// <summary>
    /// Disposes the repository and its underlying resources.
    /// </summary>
    public void Dispose()
    {
        _context?.Dispose();
    }
}