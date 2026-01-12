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
    /// List of Jokes
    /// </summary>
    private readonly JsonJokeList _jokeData;

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
        using (var r = new StreamReader(jsonFilePath))
        {
            var json = r.ReadToEnd();
            _jokeData = JsonConvert.DeserializeObject<JsonJokeList>(json) ?? new JsonJokeList();
        }

        // Extract distinct categories
        _jokeCategories = _jokeData.Jokes.Select(joke => joke.JokeCategoryTxt).Distinct().Order().ToList();
    }

    /// <summary>
    /// Get a random joke
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Record</returns>
    public Joke GetRandomJoke(string requestingUserName = "ANON")
    {
        if (_jokeData.Jokes == null || _jokeData.Jokes.Count == 0)
        {
            return new Joke { JokeTxt = "No jokes here!", JokeCategoryTxt = "None" };
        }

        var joke = _jokeData.Jokes[Random.Shared.Next(0, _jokeData.Jokes.Count)];
        return joke ?? new Joke { JokeTxt = "No jokes here!", JokeCategoryTxt = "None" };
    }

    /// <summary>
    /// Find One Specific Joke
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Record</returns>
    public Joke GetOne(int id, string requestingUserName = "ANON")
    {
        var joke = _jokeData.Jokes.FirstOrDefault(j => j.JokeId == id);
        return joke ?? new Joke { JokeTxt = "Joke not found", JokeCategoryTxt = "None" };
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
            var jokesByTermAndCategory = _jokeData.Jokes
                .Where(joke => jokeCategoryList.Any(category => category == joke.JokeCategoryTxt)
                    && joke.JokeTxt.Contains(searchTxt, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            return jokesByTermAndCategory.AsQueryable();
        }

        // User supplied ONLY category and NOT search term
        if (!string.IsNullOrEmpty(jokeCategoryTxt) && string.IsNullOrEmpty(searchTxt))
        {
            var jokesInCategory = _jokeData.Jokes
                .Where(joke => jokeCategoryList.Any(category => category == joke.JokeCategoryTxt))
                .ToList();
            return jokesInCategory.AsQueryable();
        }

        // User supplied NOT category and ONLY search term
        if (string.IsNullOrEmpty(jokeCategoryTxt) && !string.IsNullOrEmpty(searchTxt))
        {
            var jokesByTerm = _jokeData.Jokes
                .Where(joke => joke.JokeTxt.Contains(searchTxt, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            return jokesByTerm.AsQueryable();
        }

        // User supplied neither category nor search term - return all
        return _jokeData.Jokes.AsQueryable();
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
        return _jokeData.Jokes.AsQueryable();
    }

    /// <summary>
    /// Disposal
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose for JSON-based repository
    }
}

/// <summary>
/// Helper class for deserializing JSON
/// </summary>
[ExcludeFromCodeCoverage]
public class JsonJokeList
{
    /// <summary>
    /// List of Jokes
    /// </summary>
    public List<Joke> Jokes { get; set; } = new List<Joke>();
}
