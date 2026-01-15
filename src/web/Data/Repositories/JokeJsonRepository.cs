//-----------------------------------------------------------------------
// <copyright file="JokeJsonRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Repository - JSON File Based
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;
using Newtonsoft.Json;

namespace DadABase.Data.Repositories;

/// <summary>
/// Joke Repository - JSON File Based (Fallback when no database connection)
/// </summary>
[ExcludeFromCodeCoverage]
public class JokeJsonRepository : IJokeRepository
{
    /// <summary>
    /// List of Jokes (mapped from JSON)
    /// </summary>
    private readonly List<Joke> _jokes;

    /// <summary>
    /// List of Categories
    /// </summary>
    private readonly List<string> _jokeCategories;

    /// <summary>
    /// Joke Repository
    /// </summary>
    /// <param name="jsonFilePath">Path to the JSON file containing jokes</param>
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
    /// Get a random joke
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Record</returns>
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
    /// Find One Specific Joke
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Record</returns>
    public Joke GetOne(int id, string requestingUserName = "ANON")
    {
        var joke = _jokes.FirstOrDefault(j => j.JokeId == id);
        return joke ?? new Joke { JokeTxt = "Joke not found", Categories = "None" };
    }

    /// <summary>
    /// Find Matching Jokes by Search Text and Category
    /// </summary>
    /// <param name="searchTxt">Search Text</param>
    /// <param name="jokeCategoryTxt">Category</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Records</returns>
    public IQueryable<Joke> SearchJokes(string searchTxt = "", string jokeCategoryTxt = "", string requestingUserName = "ANON")
    {
        List<string> jokeCategoryList = null;
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
                    return jokeCategoryList.Any(category => jokeCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
                        && joke.JokeTxt.Contains(searchTxt, StringComparison.InvariantCultureIgnoreCase);
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
                    return jokeCategoryList.Any(category => jokeCategories.Contains(category, StringComparer.OrdinalIgnoreCase));
                })
                .ToList();
            return jokesInCategory.AsQueryable();
        }

        // User supplied NOT category and ONLY search term
        if (string.IsNullOrEmpty(jokeCategoryTxt) && !string.IsNullOrEmpty(searchTxt))
        {
            var jokesByTerm = _jokes
                .Where(joke => joke.JokeTxt.Contains(searchTxt, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            return jokesByTerm.AsQueryable();
        }

        // User supplied neither category nor search term - return all
        return _jokes.AsQueryable();
    }

    /// <summary>
    /// Get Joke Categories
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>List of Category Names</returns>
    public IQueryable<string> GetJokeCategories(string requestingUserName = "ANON")
    {
        return _jokeCategories.AsQueryable();
    }

    /// <summary>
    /// Find All Records
    /// </summary>
    /// <param name="activeInd">Active?</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Records</returns>
    public IQueryable<Joke> ListAll(string activeInd = "Y", string requestingUserName = "ANON")
    {
        return _jokes.AsQueryable();
    }

    /// <summary>
    /// Update ImageTxt field for a specific joke
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
    /// <param name="imageTxt">Image description text</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Success</returns>
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
    /// Export all jokes and categories to SQL format
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>SQL script content</returns>
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
    /// Escape SQL string literals (handle single quotes)
    /// </summary>
    private static string EscapeSqlString(string input)
    {
        return input?.Replace("'", "''") ?? string.Empty;
    }

    /// <summary>
    /// Disposal
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose for JSON-based repository
    }
}
