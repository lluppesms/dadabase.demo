//-----------------------------------------------------------------------
// <copyright file="JsonJoke.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// JSON Joke Model - for deserializing jokes from JSON files
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data.Models;

/// <summary>
/// Represents a JSON joke model with a flat structure matching the JSON file format.
/// </summary>
/// <remarks>
/// This class is used by the <see cref="Repositories.JokeJsonRepository"/> to deserialize jokes from JSON files.
/// </remarks>
[ExcludeFromCodeCoverage]
public class JsonJoke
{
    /// <summary>
    /// Gets or sets the text of the joke's category.
    /// </summary>
    /// <value>
    /// A string that maps to the categories in the <see cref="Joke"/> model.
    /// </value>
    public string? JokeCategoryTxt { get; set; }

    /// <summary>
    /// Gets or sets the text of the joke.
    /// </summary>
    /// <value>
    /// A string containing the text of the joke.
    /// </value>
    public string? JokeTxt { get; set; }

    /// <summary>
    /// Gets or sets the attribution for the joke.
    /// </summary>
    /// <value>
    /// A string representing the attribution information.
    /// </value>
    public string? Attribution { get; set; }

    /// <summary>
    /// Converts the current JSON joke model to a <see cref="Joke"/> entity.
    /// </summary>
    /// <param name="jokeId">The optional joke identifier to assign.</param>
    /// <returns>A new <see cref="Joke"/> entity with the mapped properties.</returns>
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
/// Represents a container for deserializing the JSON jokes file.
/// </summary>
[ExcludeFromCodeCoverage]
public class JsonJokeList
{
    /// <summary>
    /// Gets or sets the list of JSON jokes.
    /// </summary>
    /// <value>
    /// A list of <see cref="JsonJoke"/> instances. The default is an empty list.
    /// </value>
    public List<JsonJoke> Jokes { get; set; } = new List<JsonJoke>();
}
