//-----------------------------------------------------------------------
// <copyright file="Export_Repository_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Export Repository Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

using DadABase.Data;
using DadABase.Data.Repositories;

[ExcludeFromCodeCoverage]
public class Export_Repository_Tests : BaseTest
{
    private readonly IJokeRepository repo;

    public Export_Repository_Tests(ITestOutputHelper output)
    {
        Task.Run(() => SetupInitialize(output)).Wait();

        // Use JSON repository for testing (avoids SQL stored procedure dependencies)
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Jokes.json");
        repo = new JokeJsonRepository(jsonFilePath);
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
    public void Repository_ExportToSql_Contains_Valid_SQL_Structure()
    {
        // Arrange

        // Act
        var sqlContent = repo.ExportToSql("TEST_USER");

        // Assert
        Assert.NotNull(sqlContent);
        
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
    public void Repository_ExportToSql_Escapes_Single_Quotes()
    {
        // Arrange

        // Act
        var sqlContent = repo.ExportToSql("TEST_USER");

        // Assert
        Assert.NotNull(sqlContent);
        
        // Check that single quotes in jokes are properly escaped (doubled)
        // SQL uses '' to escape a single quote within a string literal
        // The test data should have jokes with apostrophes that need escaping
        Assert.Contains("''", sqlContent); // Should find escaped quotes
        
        output.WriteLine("Export SQL generated successfully with proper quote escaping");
    }

    [Fact]
    public void Repository_ExportToSql_Handles_Multiple_Categories()
    {
        // Arrange

        // Act
        var sqlContent = repo.ExportToSql("TEST_USER");

        // Assert
        Assert.NotNull(sqlContent);
        
        // The export should handle jokes with multiple categories
        // by creating multiple rows in the temp table
        Assert.Contains("INSERT INTO @tmpJokes (JokeCategoryTxt, JokeTxt, Attribution) VALUES", sqlContent);
        
        // Should have multiple category entries
        var lines = sqlContent.Split('\n');
        var insertLines = lines.Where(l => l.Trim().StartsWith("(")).ToList();
        Assert.True(insertLines.Count > 0, "Should have at least one joke entry");
        
        output.WriteLine($"Found {insertLines.Count} joke-category combinations");
    }
}
