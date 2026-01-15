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
            .Select(c => c.JokeCategoryTxt)
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
        
        // Header
        sb.AppendLine("------------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("-- Exported Joke Data");
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

        // Group jokes by category for better organization
        var jokesByCategory = jokesQuery
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
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _context?.Dispose();
    }
}