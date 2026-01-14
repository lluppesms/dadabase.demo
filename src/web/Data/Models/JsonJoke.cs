//-----------------------------------------------------------------------
// <copyright file="JsonJoke.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// JSON Joke Model - for deserializing jokes from JSON files
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data.Models;

/// <summary>
/// JSON Joke Model - flat structure matching the JSON file format.
/// Used by JokeJsonRepository to deserialize jokes from JSON files.
/// </summary>
[ExcludeFromCodeCoverage]
public class JsonJoke
{
    /// <summary>
    /// Joke Category Text (maps to Categories in the Joke model)
    /// </summary>
    public string? JokeCategoryTxt { get; set; }

    /// <summary>
    /// Joke Text
    /// </summary>
    public string? JokeTxt { get; set; }

    /// <summary>
    /// Attribution
    /// </summary>
    public string? Attribution { get; set; }

    /// <summary>
    /// Convert this JSON joke to a Joke entity
    /// </summary>
    /// <param name="jokeId">Optional joke ID to assign</param>
    /// <returns>Joke entity with mapped properties</returns>
    public Joke ToJoke(int jokeId = 0)
    {
        return new Joke
        {
            JokeId = jokeId,
            JokeTxt = JokeTxt ?? string.Empty,
            Categories = JokeCategoryTxt ?? string.Empty,
            Attribution = Attribution ?? string.Empty,
            SortOrderNbr = 50,
            ActiveInd = "Y",
            CreateUserName = "JSON",
            CreateDateTime = DateTime.UtcNow,
            ChangeUserName = "JSON",
            ChangeDateTime = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Container for deserializing the JSON jokes file
/// </summary>
[ExcludeFromCodeCoverage]
public class JsonJokeList
{
    /// <summary>
    /// List of JSON Jokes
    /// </summary>
    public List<JsonJoke> Jokes { get; set; } = new List<JsonJoke>();
}
