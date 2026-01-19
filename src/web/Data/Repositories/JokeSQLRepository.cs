//-----------------------------------------------------------------------
// <copyright file="JokeRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Repository
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;
using System.Threading;

namespace DadABase.Data.Repositories;

/// <summary>
/// Joke Repository
/// </summary>
/// <remarks>
/// Joke Repository
/// </remarks>
/// <param name="context">Database Context</param>
[ExcludeFromCodeCoverage]
public class JokeSQLRepository(DadABaseDbContext context) : IJokeRepository
{
    /// <summary>
    /// DadABase Database Context
    /// </summary>
    private readonly DadABaseDbContext _context = context;

    /// <summary>
    /// Get a random joke
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Record</returns>
    public Joke GetRandomJoke(string requestingUserName = "ANON")
    {
        var joke = _context.Jokes
            .FromSqlRaw("EXEC [dbo].[usp_Get_Random_Joke]")
            .AsEnumerable()
            .FirstOrDefault();

        return joke ?? new Joke("No jokes here!");
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
    /// List All Jokes with Categories populated
    /// </summary>
    /// <returns>List of Jokes</returns>
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
    /// Update ImageTxt field for a specific joke
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
    /// <param name="imageTxt">Image description text</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Success</returns>
    public bool UpdateImageTxt(int jokeId, string imageTxt, string requestingUserName = "ANON")
    {
        try
        {
            _context.Database.ExecuteSqlInterpolated($"EXEC [dbo].[usp_Joke_Update_ImageTxt] @jokeId = {jokeId}, @imageTxt = {imageTxt}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating ImageTxt for JokeId {jokeId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get Joke Categories
    /// </summary>
    /// <returns>List of Category Names</returns>
    public IQueryable<string> GetJokeCategories(string requestingUserName)
    {
        return _context.JokeCategories
            .Where(c => c.JokeCategoryTxt != null)
            .Select(c => c.JokeCategoryTxt!)
            .OrderBy(c => c);
    }

    /// <summary>
    /// Get One Record with Categories populated
    /// </summary>
    /// <param name="id"></param>
    /// <param name="requestingUserName"></param>
    /// <returns></returns>
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
    /// Export all jokes and categories to SQL format
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>SQL script content</returns>
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
    /// Escape SQL string literals (handle single quotes)
    /// </summary>
    private static string EscapeSqlString(string input)
    {
        return input?.Replace("'", "''") ?? string.Empty;
    }

    /// <summary>
    /// Update a joke
    /// </summary>
    /// <param name="joke">Joke to update</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Success</returns>
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
            Console.WriteLine($"Error updating joke {joke.JokeId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get all joke categories (entities, not just names)
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>List of JokeCategory entities</returns>
    public IQueryable<JokeCategory> GetAllCategories(string requestingUserName = "ANON")
    {
        return _context.JokeCategories
            .Where(c => c.ActiveInd == "Y")
            .OrderBy(c => c.JokeCategoryTxt);
    }

    /// <summary>
    /// Update joke categories
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
    /// <param name="categoryIds">List of category IDs</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Success</returns>
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
                        Console.WriteLine($"Error updating categories for joke {jokeId}: {ex.Message}");
                        return false;
                    }
                }


    /// <summary>
    /// Add a new joke
    /// </summary>
    /// <param name="joke">Joke to add</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>The ID of the newly created joke, or -1 if failed</returns>
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
            Console.WriteLine($"Error adding joke: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _context?.Dispose();
    }
}