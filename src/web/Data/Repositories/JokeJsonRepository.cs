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
    /// Disposal
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose for JSON-based repository
    }
}
