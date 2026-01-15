//-----------------------------------------------------------------------
// <copyright file="Export_API_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Export API Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

using DadABase.API;
using DadABase.Data;
using DadABase.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

[ExcludeFromCodeCoverage]
public class Export_API_Tests : BaseTest
{
    private readonly IJokeRepository repo;
    private readonly ExportController apiController;

    public Export_API_Tests(ITestOutputHelper output)
    {
        Task.Run(() => SetupInitialize(output)).Wait();

        // Use JSON repository for testing (avoids SQL stored procedure dependencies)
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Jokes.json");
        repo = new JokeJsonRepository(jsonFilePath);

        var mockContext = GetMockHttpContext(testData.UserName);
        apiController = new ExportController(appSettings, mockContext, repo);
    }

    [Fact]
    public void Api_Export_SQL_Returns_FileResult()
    {
        // Arrange

        // Act
        var result = apiController.ExportSql();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileContentResult>(result);
        
        var fileResult = result as FileContentResult;
        Assert.NotNull(fileResult);
        Assert.Equal("text/plain", fileResult.ContentType);
        Assert.StartsWith("JokeExport_", fileResult.FileDownloadName);
        Assert.EndsWith(".sql", fileResult.FileDownloadName);
        
        output.WriteLine($"Export file name: {fileResult.FileDownloadName}");
        output.WriteLine($"Export file size: {fileResult.FileContents.Length} bytes");
    }

    [Fact]
    public void Api_Export_SQL_Contains_Valid_SQL()
    {
        // Arrange

        // Act
        var result = apiController.ExportSql();

        // Assert
        Assert.NotNull(result);
        var fileResult = result as FileContentResult;
        Assert.NotNull(fileResult);
        
        var sqlContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
        
        // Check for key SQL elements
        Assert.Contains("Declare @RemovePreviousJokes", sqlContent);
        Assert.Contains("INSERT INTO @tmpJokes", sqlContent);
        Assert.Contains("DELETE FROM JokeRating", sqlContent);
        Assert.Contains("DELETE FROM JokeJokeCategory", sqlContent);
        Assert.Contains("DELETE FROM JokeCategory", sqlContent);
        Assert.Contains("DELETE FROM Joke", sqlContent);
        Assert.Contains("INSERT INTO JokeCategory", sqlContent);
        Assert.Contains("INSERT INTO Joke", sqlContent);
        Assert.Contains("INSERT INTO JokeJokeCategory", sqlContent);
        
        output.WriteLine($"SQL content length: {sqlContent.Length} characters");
        output.WriteLine("First 500 characters:");
        output.WriteLine(sqlContent.Substring(0, Math.Min(500, sqlContent.Length)));
    }

    [Fact]
    public void Repository_ExportToSql_Returns_NonEmpty_String()
    {
        // Arrange

        // Act
        var sqlContent = repo.ExportToSql("TEST_USER");

        // Assert
        Assert.NotNull(sqlContent);
        Assert.NotEmpty(sqlContent);
        Assert.Contains("INSERT INTO @tmpJokes", sqlContent);
        
        output.WriteLine($"Generated SQL length: {sqlContent.Length} characters");
    }

    [Fact]
    public void Repository_ExportToSql_Escapes_Single_Quotes()
    {
        // Arrange
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Jokes.json");
        var testRepo = new JokeJsonRepository(jsonFilePath);

        // Act
        var sqlContent = testRepo.ExportToSql("TEST_USER");

        // Assert
        Assert.NotNull(sqlContent);
        
        // Check that single quotes in jokes are properly escaped (doubled)
        // SQL uses '' to escape a single quote within a string literal
        // The test data should have jokes with apostrophes that need escaping
        Assert.Contains("''", sqlContent); // Should find escaped quotes
        
        output.WriteLine("Export SQL generated successfully with proper quote escaping");
    }
}
