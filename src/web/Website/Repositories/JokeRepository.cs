//-----------------------------------------------------------------------
// <copyright file="JokeRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Repository
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data;

/// <summary>
/// Joke Repository
/// </summary>
[ExcludeFromCodeCoverage]
public class JokeRepository : BaseRepository, IJokeRepository
{
    /// <summary>
    /// Application Database Context
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Joke Repository
    /// </summary>
    /// <param name="context">Database Context</param>
    public JokeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a random joke
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Record</returns>
    public Joke GetRandomJoke(string requestingUserName = "ANON" )
    {
        var activeJokes = _context.Jokes
            .Where(j => j.ActiveInd == "Y")
            .ToList();
        
        if (activeJokes.Count == 0)
        {
            return new Joke("No jokes here!");
        }
        
        var joke = activeJokes[Random.Shared.Next(0, activeJokes.Count)];
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

        var query = _context.Jokes.Where(j => j.ActiveInd == "Y");

        // user supplied both category and search term
        if (!string.IsNullOrEmpty(jokeCategoryTxt) && !string.IsNullOrEmpty(searchTxt))
        {
            var categories = jokeCategoryTxt.Split(',').Select(c => c.Trim()).ToList();
            return query
                .Where(joke => categories.Contains(joke.JokeCategoryTxt)
                    && EF.Functions.Like(joke.JokeTxt, $"%{searchTxt}%"));
        }

        // user supplied ONLY category and NOT search term
        if (!string.IsNullOrEmpty(jokeCategoryTxt) && string.IsNullOrEmpty(searchTxt))
        {
            var categories = jokeCategoryTxt.Split(',').Select(c => c.Trim()).ToList();
            return query.Where(joke => categories.Contains(joke.JokeCategoryTxt));
        }

        // user supplied NOT category and ONLY search term
        if (string.IsNullOrEmpty(jokeCategoryTxt) && !string.IsNullOrEmpty(searchTxt))
        {
            return query.Where(joke => EF.Functions.Like(joke.JokeTxt, $"%{searchTxt}%"));
        }

        // user supplied NEITHER category NOR search term - get a random joke
        var randomJoke = GetRandomJoke();
        return new List<Joke> { randomJoke }.AsQueryable();
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
    public IQueryable<string> GetJokeCategories(string activeInd, string requestingUserName)
    {
        return _context.Jokes
            .Where(j => j.ActiveInd == activeInd)
            .Select(j => j.JokeCategoryTxt)
            .Distinct()
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