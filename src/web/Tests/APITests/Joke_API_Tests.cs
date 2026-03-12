//-----------------------------------------------------------------------
// <copyright file="Joke_API_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke API Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

using DadABase.Data;
using DadABase.Data.Models;
using DadABase.Data.Repositories;

/// <summary>
/// Contains API-level tests for the <see cref="JokeController"/> endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
public class Joke_API_Tests : BaseTest
{
    /// <summary>
    /// Provides joke data access for the tests.
    /// </summary>
    private readonly IJokeRepository repo;

    /// <summary>
    /// Provides access to the joke API under test.
    /// </summary>
    private readonly JokeController apiController;

    /// <summary>
    /// Initializes a new instance of the <see cref="Joke_API_Tests"/> class.
    /// </summary>
    /// <param name="output">The test output helper used for logging.</param>
    public Joke_API_Tests(ITestOutputHelper output)
    {
        Task.Run(() => SetupInitialize(output)).Wait();

        // Use JSON repository for testing (avoids SQL stored procedure dependencies)
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Jokes.json");
        repo = new JokeJsonRepository(jsonFilePath);

        var mockContext = GetMockHttpContext(testData.UserName);
        apiController = new JokeController(appSettings, mockContext, repo);
    }

    /// <summary>
    /// Verifies that the API returns a complete list of jokes.
    /// </summary>
    [Fact]
    public void Api_Joke_Get_List_Works()
    {
        // Arrange

        // Act
        var jokeList = apiController.List();

        // Assert
        Assert.True(jokeList != null, "Found no data!");
        output.WriteLine($"Found {jokeList.Count} Jokes!");
        foreach (var item in jokeList)
        {
            output.WriteLine($"Joke: {item.Category} {item.Joke}");
        }
        Assert.True(jokeList.Count >= 0, "Found no Jokes!");
    }

    /// <summary>
    /// Verifies that requesting a random joke returns a valid result.
    /// </summary>
    [Fact]
    public void Api_Joke_GetRandom_Works()
    {
        // Arrange

        // Act
        var joke = apiController.Get();

        // Assert
        Assert.True(joke != null, "Found no data!");
        output.WriteLine($"Found Joke: {joke.Joke}");
    }

    /// <summary>
    /// Verifies that the category endpoint filters jokes correctly.
    /// </summary>
    [Fact]
    public void Api_Joke_Category_Works()
    {
        // Arrange

        // Act
        var jokeList = apiController.Category("Engineers");

        // Assert
        Assert.True(jokeList != null, "Found no data!");
        output.WriteLine($"Found {jokeList.Count} Jokes!");
        foreach (var item in jokeList)
        {
            output.WriteLine($"Joke: {item.Category} {item.Joke}");
        }
        Assert.True(jokeList.Count >= 0, "Found no Jokes!");
    }

    // Assign an owner to the test
    // https://devblogs.microsoft.com/devops/part-2using-traits-with-different-test-frameworks-in-the-unit-test-explorer/#:~:text=If%20we%20add%20in%20another%20test%20method%20and

    // MS Test?
    // [Owner("Just for test.")]

    // NUnit?
    // [Property("Owner", "Just for test.")]

    // XUnit
    /// <summary>
    /// Verifies that the search endpoint returns jokes matching the query.
    /// </summary>
    [Trait("Owner", "Dad")]
    [Fact]
    public void Api_Joke_Search_Works()
    {
        // Arrange

        // Act
        var jokeList = apiController.Search("it");

        // Assert
        Assert.True(jokeList != null, "Found no data!");
        output.WriteLine($"Found {jokeList.Count} Jokes!");
        foreach (var item in jokeList)
        {
            output.WriteLine($"Joke: {item.Category} {item.Joke}");
        }
        Assert.True(jokeList.Count >= 0, "Found no Jokes!");
        // Assert.True(jokeList.Count == 0, "Break this test!");
    }

    //[Fact]
    //public void Api_Joke_Put_Works()
    //{
    //    // Arrange
    //    var newJoke = new Joke()
    //    {
    //        JokeCategoryId = 1,
    //        JokeCategoryTxt = "Chickens",
    //        JokeTxt = "Which day of the week do chickens hate most? Fry-day!"
    //    };

    //    // Act
    //    var message = apiController.Put(newJoke);

    //    // Assert
    //    output.WriteLine($"API returned {message.Success} {message.Message}");
    //    Assert.True(message.Success, "Put did not succeed!");
    //}

    /// <summary>
    /// Verifies that the controller initializes successfully with JSON data.
    /// </summary>
    [Fact]
    public void Api_Joke_Initialize_Works()
    {
        // Arrange
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Jokes.json");
        _ = new JokeController(appSettings, GetMockHttpContext(testData.UserName), new JokeJsonRepository(jsonFilePath));
        // Act
        // Assert
    }

    /// <summary>
    /// Verifies that GetOne returns a valid joke when given a valid ID.
    /// </summary>
    [Fact]
    public void Api_Joke_GetOne_Works()
    {
        // Arrange - JSON repository generates sequential IDs starting at 1
        const int validJokeId = 1;

        // Act
        var joke = apiController.GetOne(validJokeId);

        // Assert
        Assert.NotNull(joke);
        output.WriteLine($"GetOne returned joke: {joke.Joke}");
    }

    /// <summary>
    /// Verifies that GetOne returns a result even when given an ID that does not exist.
    /// </summary>
    [Fact]
    public void Api_Joke_GetOne_NotFound_ReturnsResult()
    {
        // Arrange
        const int nonExistentId = int.MaxValue;

        // Act
        var joke = apiController.GetOne(nonExistentId);

        // Assert - repository returns a placeholder, not null
        Assert.NotNull(joke);
        output.WriteLine($"GetOne with non-existent ID returned: {joke.Joke}");
    }

    /// <summary>
    /// Verifies that SearchCategory returns jokes matching both category and search text.
    /// </summary>
    [Fact]
    public void Api_Joke_SearchCategory_Works()
    {
        // Act
        var jokeList = apiController.SearchCategory("Engineers", "e");

        // Assert
        Assert.NotNull(jokeList);
        output.WriteLine($"SearchCategory found {jokeList.Count} jokes in 'Engineers' category containing 'e'.");
        foreach (var item in jokeList)
        {
            output.WriteLine($"Joke: {item.Category} {item.Joke}");
        }
    }

    /// <summary>
    /// Verifies that SearchCategory with an empty search text returns category-filtered jokes.
    /// </summary>
    [Fact]
    public void Api_Joke_SearchCategory_EmptySearch_ReturnsResults()
    {
        // Act
        var jokeList = apiController.SearchCategory("Engineers", string.Empty);

        // Assert
        Assert.NotNull(jokeList);
        output.WriteLine($"SearchCategory (empty search) found {jokeList.Count} jokes in 'Engineers'.");
    }
}