//-----------------------------------------------------------------------
// <copyright file="Export_Repository_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
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

    [Fact]
    public void Repository_ExportToJson_Returns_Valid_Json_Array()
    {
        // Arrange

        // Act
        var jsonContent = repo.ExportToJson("TEST_USER");

        // Assert
        Assert.NotNull(jsonContent);
        Assert.NotEmpty(jsonContent);

        // Verify it's valid JSON by parsing it
        var jokes = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);
        Assert.Equal(System.Text.Json.JsonValueKind.Array, jokes.ValueKind);
        Assert.True(jokes.GetArrayLength() > 0, "Should have at least one joke in the export");

        output.WriteLine($"Generated JSON length: {jsonContent.Length} characters");
        output.WriteLine($"Total jokes exported: {jokes.GetArrayLength()}");
    }

    [Fact]
    public void Repository_ExportToJson_Contains_Expected_Fields()
    {
        // Arrange

        // Act
        var jsonContent = repo.ExportToJson("TEST_USER");

        // Assert
        Assert.NotNull(jsonContent);

        var jokes = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);
        var firstJoke = jokes[0];

        // Verify all expected fields are present
        Assert.True(firstJoke.TryGetProperty("JokeId", out _), "Should have JokeId field");
        Assert.True(firstJoke.TryGetProperty("Categories", out _), "Should have Categories field");
        Assert.True(firstJoke.TryGetProperty("JokeTxt", out _), "Should have JokeTxt field");
        Assert.True(firstJoke.TryGetProperty("ImageTxt", out _), "Should have ImageTxt field");
        Assert.True(firstJoke.TryGetProperty("Attribution", out _), "Should have Attribution field");
        Assert.True(firstJoke.TryGetProperty("ActiveInd", out _), "Should have ActiveInd field");
        Assert.True(firstJoke.TryGetProperty("SortOrderNbr", out _), "Should have SortOrderNbr field");
        Assert.True(firstJoke.TryGetProperty("Rating", out _), "Should have Rating field");
        Assert.True(firstJoke.TryGetProperty("VoteCount", out _), "Should have VoteCount field");

        output.WriteLine("First joke in export:");
        output.WriteLine(System.Text.Json.JsonSerializer.Serialize(firstJoke, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    [Fact]
    public void Repository_ConvertJsonToTabDelimited_Produces_Valid_Tsv()
    {
        // Arrange - export to JSON first
        var jsonContent = repo.ExportToJson("TEST_USER");

        // Act - convert JSON to TSV
        var tsvContent = DadABase.Data.Repositories.JokeSQLRepository.ConvertJsonToTabDelimited(jsonContent);

        // Assert
        Assert.NotNull(tsvContent);
        Assert.NotEmpty(tsvContent);

        // Verify header row
        var lines = tsvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length > 1, "Should have header and at least one data row");
        Assert.StartsWith("JokeId\t", lines[0]);

        // Verify data rows have the right number of tab-separated fields (9)
        var dataLine = lines[1].Split('\t');
        Assert.Equal(9, dataLine.Length);

        output.WriteLine($"TSV has {lines.Length - 1} data rows");
        output.WriteLine($"Header: {lines[0]}");
        output.WriteLine($"First data row: {lines[1]}");
    }
}
