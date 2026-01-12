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
    /// List All Jokes
    /// </summary>
    /// <returns>List of Category Names</returns>
    public IQueryable<Joke> ListAll(string activeInd = "Y", string requestingUserName = "ANON")
    {
        return _context.Jokes.Where(j => j.ActiveInd == activeInd);
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
    /// Get One Record
    /// </summary>
    /// <param name="id"></param>
    /// <param name="requestingUserName"></param>
    /// <returns></returns>
    public Joke GetOne(int id, string requestingUserName = "ANON")
    {
        var joke = _context.Jokes.FirstOrDefault(j => j.JokeId == id);
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
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _context?.Dispose();
    }
}