//-----------------------------------------------------------------------
// <copyright file="RecordProcessor.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Analyzer - Record processing logic for batch joke analysis
// </summary>
//-----------------------------------------------------------------------
using System.Diagnostics;
using JokeAnalyzer.Models;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace JokeAnalyzer;

/// <summary>
/// Handles batch processing of jokes using AI-powered analysis.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RecordProcessor"/> class.
/// </remarks>
/// <param name="dbContextOptions">Database context options.</param>
/// <param name="chatCompletionService">Chat completion service for AI processing.</param>
public class RecordProcessor(DbContextOptions<JokeDbContext> dbContextOptions, IChatCompletionService chatCompletionService)
{
    /// <summary>
    /// Helper class to hold mutable category state during processing.
    /// </summary>
    private class CategoryState
    {
        public List<JokeCategory> ExistingCategories { get; set; } = [];
        public string CategoryNames { get; set; } = string.Empty;
    }
    private readonly DbContextOptions<JokeDbContext> _dbContextOptions = dbContextOptions;
    private readonly IChatCompletionService _chatCompletionService = chatCompletionService;

    private const int MaxBatchSize = 10;

    // Prompt template for image description generation
    private const string ImageDescriptionPrompt =
        "You are going to be told a funny joke or a humorous line or an insightful quote. " +
        "It is your responsibility to describe that joke so that an artist can draw a picture of the mental image that this joke creates. " +
        "Give clear instructions on how the scene should look and what objects should be included in the scene. " +
        "Instruct the artist to draw it in a humorous cartoon format. " +
        "Make sure the description does not ask for anything violent, sexual, or political so that it does not violate safety rules. " +
        "Keep the scene description under 250 words or less.\n\n" +
        "Joke: {0}\n\n" +
        "Image Description:";

    // Prompt template for category evaluation
    private const string CategoryPrompt =
        "Given the following joke, identify the two or three most appropriate categories it belongs to. " +
        "Choose from existing categories if they fit, or suggest new categories if needed. " +
        "Return only the category names, separated by commas.\n\n" +
        "Existing categories: {0}\n\n" +
        "Joke: {1}\n\n" +
        "Categories:";

    /// <summary>
    /// Processes all active jokes, generating image descriptions and assigning categories.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessJokesAsync()
    {
        await using var context = new JokeDbContext(_dbContextOptions);

        // Get all active jokes (limited to batch size)
        var jokes = await context.Jokes
            .Where(j => j.ActiveInd == "Y")
            .OrderBy(j => j.JokeId)
            .Take(MaxBatchSize)
            .ToListAsync();

        var totalJokes = jokes.Count;
        AnsiConsole.MarkupLine($"[cyan]Found {totalJokes} jokes to process[/]\n");

        if (totalJokes == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No jokes found to process[/]");
            return;
        }

        // Get existing categories
        var existingCategories = await context.JokeCategories
            .Where(c => c.ActiveInd == "Y")
            .ToListAsync();

        var categoryState = new CategoryState
        {
            ExistingCategories = existingCategories,
            CategoryNames = string.Join(", ", existingCategories.Select(c => c.JokeCategoryTxt))
        };

        var processedCount = 0;
        var updatedCount = 0;
        var errorCount = 0;
        var totalStopwatch = Stopwatch.StartNew();

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Processing jokes[/]", maxValue: totalJokes);

                foreach (var joke in jokes)
                {
                    processedCount++;
                    var jokeStopwatch = Stopwatch.StartNew();
                    AnsiConsole.MarkupLine($"[cyan]Processing record {processedCount} of {totalJokes}[/] - Joke ID: {joke.JokeId}");
                    AnsiConsole.MarkupLine($"  [grey]Joke: {Markup.Escape(joke.JokeTxt ?? string.Empty)}[/]");

                    try
                    {
                        await ProcessSingleJokeAsync(context, joke, categoryState);
                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        AnsiConsole.MarkupLine($"  [red]✗ Error processing joke {joke.JokeId}: {ex.Message}[/]");
                        break;
                    }
                    finally
                    {
                        jokeStopwatch.Stop();
                        AnsiConsole.MarkupLine($"  [blue]⏱ Processing time: {jokeStopwatch.Elapsed.TotalSeconds:F2} seconds[/]\n");
                    }

                    task.Increment(1);
                }
            });

        totalStopwatch.Stop();

        // Display summary
        DisplaySummary(totalJokes, updatedCount, errorCount, totalStopwatch.Elapsed);
    }

    private async Task ProcessSingleJokeAsync(JokeDbContext context, Joke joke, CategoryState categoryState)
    {
        // Generate image description only if ImageTxt is empty
        if (string.IsNullOrWhiteSpace(joke.ImageTxt))
        {
            AnsiConsole.MarkupLine($"  [blue] Analyzing joke to create a mental image...[/]");
            var imagePrompt = string.Format(ImageDescriptionPrompt, joke.JokeTxt);
            var imageHistory = new ChatHistory();
            imageHistory.AddUserMessage(imagePrompt);

            var imageResponse = await _chatCompletionService.GetChatMessageContentAsync(
                imageHistory,
                new OpenAIPromptExecutionSettings { MaxTokens = 300, Temperature = 0.7 }
            );

            joke.ImageTxt = imageResponse.Content?.Trim();
            joke.ChangeDateTime = DateTime.UtcNow;
            joke.ChangeUserName = "JokeAnalyzer";

            AnsiConsole.MarkupLine($"  [green]✓ Generated image description[/]");
            AnsiConsole.MarkupLine($"  [lightgray]✓ {joke.ImageTxt}[/]");

            AnsiConsole.MarkupLine($"  [blue] Evaluating categories...[/]");
            // Evaluate categories
            var catPrompt = string.Format(CategoryPrompt, categoryState.CategoryNames, joke.JokeTxt);
            var catHistory = new ChatHistory();
            catHistory.AddUserMessage(catPrompt);

            var categoryResponse = await _chatCompletionService.GetChatMessageContentAsync(
                catHistory,
                new OpenAIPromptExecutionSettings { MaxTokens = 100, Temperature = 0.5 }
            );

            var suggestedCategories = categoryResponse.Content?
                .Split(',')
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList() ?? [];

            // Process categories
            foreach (var categoryName in suggestedCategories)
            {
                await ProcessCategoryAsync(context, joke, categoryName, categoryState);
            }
            // Save changes
            await context.SaveChangesAsync();
        }
        else
        {
            AnsiConsole.MarkupLine($"  [yellow]⊘ Image description already exists, skipping[/]");
        }

    }

    private async Task ProcessCategoryAsync(JokeDbContext context, Joke joke, string categoryName, CategoryState categoryState)
    {
        // Check if category exists
        AnsiConsole.MarkupLine($"  [cyan] Checking category {categoryName}...[/]");
        var category = categoryState.ExistingCategories.FirstOrDefault(c =>
            c.JokeCategoryTxt.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

        if (category == null)
        {
            // Create new category
            category = new JokeCategory
            {
                JokeCategoryTxt = categoryName,
                ActiveInd = "Y",
                SortOrderNbr = 50,
                CreateDateTime = DateTime.UtcNow,
                CreateUserName = "JokeAnalyzer",
                ChangeDateTime = DateTime.UtcNow,
                ChangeUserName = "JokeAnalyzer"
            };
            context.JokeCategories.Add(category);
            await context.SaveChangesAsync();
            categoryState.ExistingCategories.Add(category);
            categoryState.CategoryNames = string.Join(", ", categoryState.ExistingCategories.Select(c => c.JokeCategoryTxt));
            AnsiConsole.MarkupLine($"  [green]✓ Created new category: {categoryName}[/]");
        }

        // Check if joke-category relationship exists
        var jokeCategory = await context.JokeJokeCategories
            .FirstOrDefaultAsync(jjc => jjc.JokeId == joke.JokeId && jjc.JokeCategoryId == category.JokeCategoryId);

        if (jokeCategory == null)
        {
            // Create relationship
            jokeCategory = new JokeJokeCategory
            {
                JokeId = joke.JokeId,
                JokeCategoryId = category.JokeCategoryId,
                CreateDateTime = DateTime.UtcNow,
                CreateUserName = "JokeAnalyzer"
            };
            context.JokeJokeCategories.Add(jokeCategory);
            AnsiConsole.MarkupLine($"  [green]✓ Assigned to category: {categoryName}[/]");
        }
    }

    private static void DisplaySummary(int totalJokes, int updatedCount, int errorCount, TimeSpan totalElapsed)
    {
        AnsiConsole.WriteLine();
        var summaryTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .AddColumn(new TableColumn("[yellow]Summary[/]").Centered());

        summaryTable.AddRow($"[cyan]Total Records:[/] {totalJokes}");
        summaryTable.AddRow($"[green]Successfully Processed:[/] {updatedCount}");
        summaryTable.AddRow($"[red]Errors:[/] {errorCount}");
        summaryTable.AddRow($"[blue]Total Processing Time:[/] {FormatElapsedTime(totalElapsed)}");

        AnsiConsole.Write(summaryTable);
    }

    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalHours >= 1)
        {
            return $"{elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s";
        }
        else if (elapsed.TotalMinutes >= 1)
        {
            return $"{elapsed.Minutes}m {elapsed.Seconds}s";
        }
        else
        {
            return $"{elapsed.TotalSeconds:F2} seconds";
        }
    }
}
