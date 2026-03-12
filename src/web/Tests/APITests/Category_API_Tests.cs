//-----------------------------------------------------------------------
// <copyright file="Category_API_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Category API Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

using DadABase.Data;
using DadABase.Data.Repositories;

/// <summary>
/// Contains API-level tests for the <see cref="CategoryController"/> endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
public class Category_API_Tests : BaseTest
{
    private readonly IJokeRepository repo;
    private readonly CategoryController apiController;

    /// <summary>
    /// Initializes a new instance of the <see cref="Category_API_Tests"/> class.
    /// </summary>
    public Category_API_Tests(ITestOutputHelper output)
    {
        Task.Run(() => SetupInitialize(output)).Wait();
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Jokes.json");
        repo = new JokeJsonRepository(jsonFilePath);
        var mockContext = GetMockHttpContext(testData.UserName);
        apiController = new CategoryController(appSettings, mockContext, repo);
    }

    /// <summary>
    /// Verifies that the category list endpoint returns categories.
    /// </summary>
    [Fact]
    public void Api_Category_List_Works()
    {
        // Act
        var categoryList = apiController.List();

        // Assert
        Assert.NotNull(categoryList);
        Assert.True(categoryList.Count > 0, "Found no categories!");
        output.WriteLine($"Found {categoryList.Count} Categories!");
        foreach (var category in categoryList)
        {
            output.WriteLine($"Category: {category}");
        }
    }

    /// <summary>
    /// Verifies that the controller initializes successfully.
    /// </summary>
    [Fact]
    public void Api_Category_Initialize_Works()
    {
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Jokes.json");
        _ = new CategoryController(appSettings, GetMockHttpContext(testData.UserName), new JokeJsonRepository(jsonFilePath));
    }
}
