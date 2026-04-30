//-----------------------------------------------------------------------
// <copyright file="JokeJsonRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Repository - JSON File Based
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;
using Newtonsoft.Json;

namespace DadABase.Data.Repositories;

/// <summary>
/// Provides a JSON-backed fallback implementation of <see cref="IJokeRepository"/> when a database is unavailable.
/// </summary>
[ExcludeFromCodeCoverage]
public class JokeJsonRepository : IJokeRepository
{
    /// <summary>
    /// Collection of jokes that are materialized from the JSON data source.
    /// </summary>
    private readonly List<Joke> _jokes;

    /// <summary>
    /// Collection of joke categories derived from the JSON data source.
    /// </summary>
    private readonly List<string> _jokeCategories;

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeJsonRepository"/> class.
    /// </summary>
    /// <param name="jsonFilePath">The path to the JSON file that contains the jokes payload.</param>
    public JokeJsonRepository(string jsonFilePath)
    {
        // Load jokes from JSON file
        JsonJokeList jsonData;
        using (var r = new StreamReader(jsonFilePath))
        {
            var json = r.ReadToEnd();
            jsonData = JsonConvert.DeserializeObject<JsonJokeList>(json) ?? new JsonJokeList();
        }

        // Map JSON jokes to Joke entities with auto-generated IDs
        _jokes = jsonData.Jokes
            .Select((jsonJoke, index) => jsonJoke.ToJoke(index + 1))
            .ToList();

        // Extract distinct categories from all jokes' Categories field (comma-separated)
        var allCategories = new HashSet<string>();
        foreach (var joke in _jokes)
        {
            if (!string.IsNullOrEmpty(joke.Categories))
            {
                var categories = joke.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var cat in categories)
                {
                    allCategories.Add(cat);
                }
            }
        }
        _jokeCategories = allCategories.Order().ToList();
    }

    /// <summary>
    /// Gets a random joke from the in-memory collection.
    /// </summary>
    /// <param name="requestingUserName">The username of the caller requesting the joke.</param>
    /// <returns>A <see cref="Joke"/> selected at random.</returns>
    public Joke GetRandomJoke(string requestingUserName = "ANON")
    {
        if (_jokes == null || _jokes.Count == 0)
        {
            return new Joke { JokeTxt = "No jokes here!", Categories = "None" };
        }

        var joke = _jokes[Random.Shared.Next(0, _jokes.Count)];
        return joke ?? new Joke { JokeTxt = "No jokes here!", Categories = "None" };
    }

    /// <summary>
    /// Gets a specific joke by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the joke.</param>
    /// <param name="requestingUserName">The username of the caller requesting the joke.</param>
    /// <returns>The matching <see cref="Joke"/> if found; otherwise, a placeholder joke.</returns>
    public Joke GetOne(int id, string requestingUserName = "ANON")
    {
        var joke = _jokes.FirstOrDefault(j => j.JokeId == id);
        return joke ?? new Joke { JokeTxt = "Joke not found", Categories = "None" };
    }

    /// <summary>
    /// Searches for jokes that match the provided text and optional category filters.
    /// </summary>
    /// <param name="searchTxt">The free-form text used to match against joke content.</param>
    /// <param name="jokeCategoryTxt">The comma-separated list of categories to filter by.</param>
    /// <param name="requestingUserName">The username of the caller requesting the search operation.</param>
    /// <returns>An <see cref="IQueryable{T}"/> representing the filtered set of jokes.</returns>
    public IQueryable<Joke> SearchJokes(string searchTxt = "", string jokeCategoryTxt = "", string requestingUserName = "ANON")
    {
        List<string>? jokeCategoryList = null;
        jokeCategoryTxt = jokeCategoryTxt.Equals("All", StringComparison.OrdinalIgnoreCase) ? string.Empty : jokeCategoryTxt;

        if (!string.IsNullOrEmpty(jokeCategoryTxt))
        {
            // Split the jokeCategoryTxt into a list of categories by comma
            var jokeCategoryArray = jokeCategoryTxt.Split(',').ToList();
            jokeCategoryList = _jokeCategories.Where(category => jokeCategoryArray.Contains(category)).Select(c => c).ToList();
        }

        // User supplied both category and search term
        if (!string.IsNullOrEmpty(jokeCategoryTxt) && !string.IsNullOrEmpty(searchTxt))
        {
            var jokesByTermAndCategory = _jokes
                .Where(joke =>
                {
                    if (string.IsNullOrEmpty(joke.Categories)) return false;
                    var jokeCategories = joke.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return jokeCategoryList!.Any(category => jokeCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
                        && (joke.JokeTxt ?? string.Empty).Contains(searchTxt, StringComparison.InvariantCultureIgnoreCase);
                })
                .ToList();
            return jokesByTermAndCategory.AsQueryable();
        }

        // User supplied ONLY category and NOT search term
        if (!string.IsNullOrEmpty(jokeCategoryTxt) && string.IsNullOrEmpty(searchTxt))
        {
            var jokesInCategory = _jokes
                .Where(joke =>
                {
                    if (string.IsNullOrEmpty(joke.Categories)) return false;
                    var jokeCategories = joke.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return jokeCategoryList!.Any(category => jokeCategories.Contains(category, StringComparer.OrdinalIgnoreCase));
                })
                .ToList();
            return jokesInCategory.AsQueryable();
        }

        // User supplied NOT category and ONLY search term
        if (string.IsNullOrEmpty(jokeCategoryTxt) && !string.IsNullOrEmpty(searchTxt))
        {
            var jokesByTerm = _jokes
                .Where(joke => (joke.JokeTxt ?? string.Empty).Contains(searchTxt, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            return jokesByTerm.AsQueryable();
        }

        // User supplied neither category nor search term - return all
        return _jokes.AsQueryable();
    }

    /// <summary>
    /// Gets the collection of available joke category names.
    /// </summary>
    /// <param name="requestingUserName">The username of the caller requesting the categories.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of category names.</returns>
    public IQueryable<string> GetJokeCategories(string requestingUserName = "ANON")
    {
        return _jokeCategories.AsQueryable();
    }

    /// <summary>
    /// Lists all jokes contained in the JSON repository.
    /// </summary>
    /// <param name="activeInd">The active indicator filter, ignored for the JSON repository.</param>
    /// <param name="requestingUserName">The username of the caller requesting the list.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of <see cref="Joke"/> items.</returns>
    public IQueryable<Joke> ListAll(string activeInd = "Y", string requestingUserName = "ANON")
    {
        return _jokes.AsQueryable();
    }

    /// <summary>
    /// Returns the most recently modified jokes from the JSON repository, ordered by last-modified date descending and limited to <paramref name="count"/> records.
    /// </summary>
    /// <param name="count">The maximum number of jokes to return. The default is 100.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of <see cref="Joke"/> records.</returns>
    public IQueryable<Joke> GetRecentAdditions(int count = 100)
    {
        return _jokes
            .OrderByDescending(j => j.ChangeDateTime)
            .Take(count)
            .AsQueryable();
    }

    /// <summary>
    /// Updates the <see cref="Joke.ImageTxt"/> field for the specified joke.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke to update.</param>
    /// <param name="imageTxt">The replacement descriptive text for the image.</param>
    /// <param name="requestingUserName">The username of the caller requesting the update.</param>
    /// <returns><see langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
    public bool UpdateImageTxt(int jokeId, string imageTxt, string requestingUserName = "ANON")
    {
        var joke = _jokes.FirstOrDefault(j => j.JokeId == jokeId);
        if (joke != null)
        {
            joke.ImageTxt = imageTxt;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Exports all jokes and categories to a SQL script representation.
    /// </summary>
    /// <param name="requestingUserName">The username of the caller requesting the export.</param>
    /// <returns>A formatted SQL script that can recreate the joke data.</returns>
    public string ExportToSql(string requestingUserName = "ANON")
    {
        var sb = new System.Text.StringBuilder();

        // Header
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("-- Exported Joke Data (from JSON source)");
        sb.AppendLine($"-- Export Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("Declare @RemovePreviousJokes varchar(1) = 'Y'");
        sb.AppendLine("Declare @tmpJokes Table (");
        sb.AppendLine("  JokeId int identity(1,1),");
        sb.AppendLine("  JokeTxt nvarchar(max),");
        sb.AppendLine("  JokeCategoryTxt nvarchar(500),");
        sb.AppendLine("  Attribution nvarchar(500),");
        sb.AppendLine("  ImageTxt nvarchar(max)");
        sb.AppendLine(")");
        sb.AppendLine("DELETE FROM @tmpJokes");
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("-- Insert Joke SQL From Excel Spreadsheet here...  (and remove the last trailing comma...)");
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");

        // Group jokes by category for better organization
        var jokesByCategory = _jokes
            .SelectMany(j => (j.Categories ?? "Unknown").Split(',').Select(c => c.Trim()),
                       (j, c) => new { Category = c, Joke = j })
            .GroupBy(x => x.Category)
            .OrderBy(g => g.Key)
            .ToList();

        sb.AppendLine("INSERT INTO @tmpJokes (JokeCategoryTxt, JokeTxt, Attribution) VALUES");

        var isFirst = true;
        foreach (var categoryGroup in jokesByCategory)
        {
            foreach (var item in categoryGroup)
            {
                if (!isFirst)
                {
                    sb.AppendLine(",");
                }
                isFirst = false;

                var category = EscapeSqlString(item.Category);
                var jokeTxt = EscapeSqlString(item.Joke.JokeTxt ?? string.Empty);
                var attribution = item.Joke.Attribution != null ? $"'{EscapeSqlString(item.Joke.Attribution)}'" : "NULL";

                sb.Append($" ('{category}', '{jokeTxt}', {attribution})");
            }
        }

        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("-- END -- Insert Joke SQL From Excel Spreadsheet here...  (and remove the last trailing comma...)");
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
        sb.AppendLine("SELECT @CategoryCount = Count(DISTINCT JokeCategoryTxt) From @TmpJokes");
        sb.AppendLine("PRINT ''");
        sb.AppendLine("PRINT 'Inserting ' + CAST(@CategoryCount as varchar) + ' fresh categories...'");
        sb.AppendLine("INSERT INTO JokeCategory (JokeCategoryTxt) ");
        sb.AppendLine("  SELECT DISTINCT JokeCategoryTxt From @tmpJokes Where JokeCategoryTxt NOT IN (Select JokeCategoryTxt From JokeCategory)");
        sb.AppendLine();
        sb.AppendLine("DECLARE @JokeCount int");
        sb.AppendLine("SELECT @JokeCount = Count(*) From @TmpJokes");
        sb.AppendLine("PRINT ''");
        sb.AppendLine("PRINT 'Inserting ' + CAST(@JokeCount as varchar) + ' fresh jokes...'");
        sb.AppendLine("INSERT INTO Joke (JokeTxt, Attribution, ImageTxt, Rating, VoteCount) ");
        sb.AppendLine("  SELECT j.JokeTxt, j.Attribution, j.ImageTxt, 0, 0");
        sb.AppendLine("  FROM @tmpJokes j");
        sb.AppendLine("  WHERE j.JokeTxt NOT IN (Select JokeTxt From Joke)");
        sb.AppendLine();
        sb.AppendLine("PRINT ''");
        sb.AppendLine("PRINT 'Populating JokeJokeCategory junction table...'");
        sb.AppendLine("INSERT INTO JokeJokeCategory (JokeId, JokeCategoryId)");
        sb.AppendLine("  SELECT DISTINCT jk.JokeId, c.JokeCategoryId");
        sb.AppendLine("  FROM Joke jk");
        sb.AppendLine("  INNER JOIN @tmpJokes tj ON jk.JokeTxt = tj.JokeTxt");
        sb.AppendLine("  INNER JOIN JokeCategory c ON tj.JokeCategoryTxt = c.JokeCategoryTxt");
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
        sb.AppendLine("  j.JokeTxt, j.ImageTxt, j.Rating, j.CreateDateTime ");
        sb.AppendLine("FROM Joke j ");
        sb.AppendLine("ORDER BY Categories, j.JokeTxt");

        return sb.ToString();
    }

    /// <summary>
    /// Escapes SQL string literals by handling single quotes.
    /// </summary>
    /// <param name="input">The input string that requires escaping.</param>
    /// <returns>The escaped SQL-safe string.</returns>
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
    /// Exports all jokes from the JSON repository to a tab-delimited text format with fields:
    /// JokeId, Categories, JokeTxt, ImageTxt, Attribution, ActiveInd, SortOrderNbr, Rating, VoteCount.
    /// </summary>
    /// <param name="requestingUserName">The username of the caller requesting the export.</param>
    /// <returns>A string containing the tab-delimited content with a header row.</returns>
    public string ExportToTabDelimited(string requestingUserName = "ANON")
    {
        var sb = new System.Text.StringBuilder();

        // Header row
        sb.AppendLine("JokeId\tCategories\tJokeTxt\tImageTxt\tAttribution\tActiveInd\tSortOrderNbr\tRating\tVoteCount");

        foreach (var joke in _jokes.OrderBy(j => j.Categories).ThenBy(j => j.JokeTxt))
        {
            sb.AppendLine($"{joke.JokeId}\t{EscapeTabField(joke.Categories)}\t{EscapeTabField(joke.JokeTxt)}\t{EscapeTabField(joke.ImageTxt)}\t{EscapeTabField(joke.Attribution)}\t{EscapeTabField(joke.ActiveInd)}\t{joke.SortOrderNbr}\t{joke.Rating ?? 0}\t{joke.VoteCount ?? 0}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports all jokes from the JSON repository to a JSON-formatted string with fields:
    /// JokeId, Categories, JokeTxt, ImageTxt, Attribution, ActiveInd, SortOrderNbr, Rating, VoteCount.
    /// </summary>
    /// <param name="requestingUserName">The username of the caller requesting the export.</param>
    /// <returns>A JSON string containing an array of joke objects with indented formatting.</returns>
    public string ExportToJson(string requestingUserName = "ANON")
    {
        var exportList = _jokes
            .OrderBy(j => j.Categories)
            .ThenBy(j => j.JokeTxt)
            .Select(joke => new
            {
                joke.JokeId,
                joke.Categories,
                joke.JokeTxt,
                joke.ImageTxt,
                joke.Attribution,
                ActiveInd = joke.ActiveInd ?? "Y",
                joke.SortOrderNbr,
                Rating = joke.Rating ?? 0,
                VoteCount = joke.VoteCount ?? 0
            })
            .ToList();

        return System.Text.Json.JsonSerializer.Serialize(exportList, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Exports all jokes from the JSON repository as a plain-text bulleted list grouped by category.
    /// </summary>
    /// <param name="requestingUserName">The username of the caller requesting the export.</param>
    /// <returns>A plain-text bulleted list suitable for copying into OneNote or similar tools.</returns>
    public string ExportToBulletedList(string requestingUserName = "ANON")
    {
        var jokes = _jokes.OrderBy(j => j.Categories).ThenBy(j => j.JokeTxt).ToList();
        return Helpers.Utilities.BuildBulletedList(jokes);
    }

    /// <summary>
    /// Import from tab-delimited is not supported for the JSON-based repository.
    /// </summary>
    /// <param name="tabData">Not used.</param>
    /// <param name="requestingUserName">Not used.</param>
    /// <returns>This method always throws because the JSON repository is read-only.</returns>
    [Obsolete("Use ImportFromTabDelimitedViaSproc instead, which delegates batch import to usp_Joke_Import.")]
    public (bool Success, int ImportedCount, string Message) ImportFromTabDelimited(string tabData, string requestingUserName = "ANON")
    {
        throw new NotSupportedException("ImportFromTabDelimited is not supported for the JSON-based repository.");
    }

    /// <summary>
    /// Import via stored procedure is not supported for the JSON-based repository.
    /// </summary>
    /// <param name="tabData">Not used.</param>
    /// <param name="requestingUserName">Not used.</param>
    /// <returns>This method always throws because the JSON repository is read-only.</returns>
    public (bool Success, int ImportedCount, string Message) ImportFromTabDelimitedViaSproc(string tabData, bool removePreviousJokes = false, string requestingUserName = "ANON")
    {
        throw new NotSupportedException("ImportFromTabDelimitedViaSproc is not supported for the JSON-based repository.");
    }

    /// <summary>
    /// Updates an existing joke.
    /// </summary>
    /// <param name="joke">The joke to update.</param>
    /// <param name="requestingUserName">The username of the caller requesting the update.</param>
    /// <returns>Always throws because updates are not supported.</returns>
    public bool UpdateJoke(Joke joke, string requestingUserName = "ANON")
    {
        // Not supported for JSON-based repository
        throw new NotSupportedException("UpdateJoke is not supported for JSON-based repository");
    }

    /// <summary>
    /// Gets the list of joke categories represented as <see cref="JokeCategory"/> entities.
    /// </summary>
    /// <param name="requestingUserName">The username of the caller requesting the categories.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of <see cref="JokeCategory"/> entities.</returns>
    public IQueryable<JokeCategory> GetAllCategories(string requestingUserName = "ANON")
    {
        // Convert category strings to JokeCategory entities
        var categories = _jokeCategories
            .Select((cat, index) => new JokeCategory(index + 1, cat))
            .AsQueryable();
        return categories;
    }

    /// <summary>
    /// Updates the categories assigned to a joke.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke to update.</param>
    /// <param name="categoryIds">The category identifiers to associate with the joke.</param>
    /// <param name="requestingUserName">The username of the caller requesting the update.</param>
    /// <returns>This method always throws because the JSON repository is read-only.</returns>
    public bool UpdateJokeCategories(int jokeId, List<int> categoryIds, string requestingUserName = "ANON")
    {
        throw new NotSupportedException("UpdateJokeCategories is not supported for JSON-based repository");
    }

    /// <summary>
    /// Adds a new joke to the repository.
    /// </summary>
    /// <param name="joke">The joke to add.</param>
    /// <param name="requestingUserName">The username of the caller requesting the add operation.</param>
    /// <returns>This method always throws because the JSON repository is read-only.</returns>
    public int AddJoke(Joke joke, string requestingUserName = "ANON")
    {
        throw new NotSupportedException("AddJoke is not supported for JSON-based repository");
    }

    /// <summary>
    /// Deletes a joke from the repository.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke to delete.</param>
    /// <param name="requestingUserName">The username of the caller requesting the delete operation.</param>
    /// <returns>This method always throws because the JSON repository is read-only.</returns>
    public bool DeleteJoke(int jokeId, string requestingUserName = "ANON")
    {
        throw new NotSupportedException("DeleteJoke is not supported for JSON-based repository");
    }

    /// <summary>
    /// Disposes the repository. No resources require explicit cleanup.
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose for JSON-based repository
    }
}
