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
/// <param name="trackTokens">Whether to track and display token usage (for cloud models).</param>
/// <param name="maxBatchSize">Maximum number of records to process (0 = no limit).</param>
public class RecordProcessor(DbContextOptions<JokeDbContext> dbContextOptions, IChatCompletionService chatCompletionService, bool trackTokens = false, int maxBatchSize = 100)
{
    /// <summary>
    /// Helper class to hold mutable category state during processing.
    /// </summary>
    private class CategoryState
    {
        public List<JokeCategory> ExistingCategories { get; set; } = [];
        public string CategoryNames { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tracks token usage for cloud model calls.
    /// </summary>
    private class TokenUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens => PromptTokens + CompletionTokens;
    }

    private readonly DbContextOptions<JokeDbContext> _dbContextOptions = dbContextOptions;
    private readonly IChatCompletionService _chatCompletionService = chatCompletionService;
    private readonly bool _trackTokens = trackTokens;
    private readonly int _maxBatchSize = maxBatchSize;

    // Running totals for token tracking
    private int _totalPromptTokens;
    private int _totalCompletionTokens;

    // Combined prompt template for image description and category evaluation
    private const string CombinedAnalysisPrompt =
        """
        Analyze the following joke and provide two things:

        1. IMAGE DESCRIPTION: Describe this joke so an artist can draw a picture of the mental image it creates.
           - Give clear instructions on how the scene should look and what objects should be included.
           - Draw it in a humorous cartoon format.
           - Make sure the description does not ask for anything violent, sexual, or political.
           - Keep the scene description under 250 words.

        2. CATEGORIES: Identify the one, two, or three of the most appropriate categories this joke belongs to.
           - Choose from existing categories if they fit, or suggest a new category if needed.
           - Try not to put everything into the Dad category - only use that for jokes clearly referencing Dad
           - If it is clearly of one type, only suggest one category. If it matches several categories, suggest two or three.
           - Existing categories: {0}

        Joke: {1}

        Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
        {{"imageDescription": "your description here", "categories": ["category1", "category2"]}}
        """;

    /// <summary>
    /// Response model for combined joke analysis.
    /// </summary>
    private class JokeAnalysisResponse
    {
        public string ImageDescription { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = [];
    }



    /// <summary>
    /// Processes all active jokes, generating image descriptions and assigning categories.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessJokesAsync()
    {
        await using var context = new JokeDbContext(_dbContextOptions);

        // Get active jokes that need image descriptions (limited to batch size if > 0)
        var jokesQuery = context.Jokes
            .Where(j => j.ActiveInd == "Y" && (j.ImageTxt == null || j.ImageTxt == ""))
            .OrderBy(j => j.JokeId);

        var jokes = _maxBatchSize > 0
            ? await jokesQuery.Take(_maxBatchSize).ToListAsync()
            : await jokesQuery.ToListAsync();

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
                var task = ctx.AddTask("[green]    Processing joke - Percent Complete: [/]", maxValue: totalJokes);

                foreach (var joke in jokes)
                {
                    processedCount++;
                    var jokeStopwatch = Stopwatch.StartNew();
                    AnsiConsole.MarkupLine($"\n[cyan] --------------------------------------------------------------------------------[/]");
                    AnsiConsole.MarkupLine($"[cyan] {DateTime.Now:HH:mm:ss}: Processing record {processedCount} of {totalJokes}[/] - Joke ID: {joke.JokeId}");
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
                        AnsiConsole.MarkupLine($"  [magenta]⏱ Processing time: {jokeStopwatch.Elapsed.TotalSeconds:F2} seconds[/]\n");
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
        AnsiConsole.MarkupLine($"  [blue]Analyzing joke (image + categories)...[/]");

        var combinedPrompt = string.Format(CombinedAnalysisPrompt, categoryState.CategoryNames, joke.JokeTxt);
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(combinedPrompt);

        var response = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            new OpenAIPromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object> { ["max_completion_tokens"] = 400 }
            }
        );

        // Track and display tokens for combined analysis call
        DisplayTokenUsage(response, "Combined Analysis");

        // Parse the JSON response
        var analysisResult = ParseAnalysisResponse(response.Content);

        joke.ImageTxt = analysisResult.ImageDescription;
        joke.ChangeDateTime = DateTime.UtcNow;
        joke.ChangeUserName = "JokeAnalyzer";

        AnsiConsole.MarkupLine($"  [green]✓ Generated image description[/]");
        AnsiConsole.MarkupLine($"  [gray]  {Markup.Escape(joke.ImageTxt ?? string.Empty)}[/]");

        AnsiConsole.MarkupLine($"  [green]✓ Identified categories: {string.Join(", ", analysisResult.Categories)}[/]");

        // Process categories
        foreach (var categoryName in analysisResult.Categories)
        {
            await ProcessCategoryAsync(context, joke, categoryName, categoryState);
        }
        // Save changes
        await context.SaveChangesAsync();
    }

    private static JokeAnalysisResponse ParseAnalysisResponse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new JokeAnalysisResponse();
        }

        try
        {
            // Clean up the response - remove any markdown code blocks if present
            var jsonContent = content.Trim();
            if (jsonContent.StartsWith("```"))
            {
                var lines = jsonContent.Split('\n');
                jsonContent = string.Join('\n', lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<JokeAnalysisResponse>(
                jsonContent,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result ?? new JokeAnalysisResponse();
        }
        catch (System.Text.Json.JsonException ex)
        {
            AnsiConsole.MarkupLine($"  [yellow]⚠ Failed to parse JSON response: {Markup.Escape(ex.Message)}[/]");
            AnsiConsole.MarkupLine($"  [gray]  Raw response: {Markup.Escape(content)}[/]");
            return new JokeAnalysisResponse();
        }
    }

    private void DisplayTokenUsage(Microsoft.SemanticKernel.ChatMessageContent response, string callDescription)
    {
        if (!_trackTokens) return;

        var metadata = response.Metadata;
        if (metadata != null && metadata.TryGetValue("Usage", out var usageObj))
        {
            // Azure OpenAI returns usage information in metadata
            if (usageObj is OpenAI.Chat.ChatTokenUsage usage)
            {
                var promptTokens = usage.InputTokenCount;
                var completionTokens = usage.OutputTokenCount;
                var totalTokens = usage.TotalTokenCount;

                _totalPromptTokens += promptTokens;
                _totalCompletionTokens += completionTokens;

                AnsiConsole.MarkupLine($"  [magenta]📊 {callDescription} tokens: {totalTokens} (prompt: {promptTokens}, completion: {completionTokens})[/]");
            }
        }
    }

    private async Task ProcessCategoryAsync(JokeDbContext context, Joke joke, string categoryName, CategoryState categoryState)
    {
        // Check if category exists
        AnsiConsole.MarkupLine($"  [cyan]  Checking category {categoryName}...[/]");
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
            AnsiConsole.MarkupLine($"  [green]  ✓ Assigned to category: {categoryName}[/]");
        }
    }

    private void DisplaySummary(int totalJokes, int updatedCount, int errorCount, TimeSpan totalElapsed)
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

        if (_trackTokens)
        {
            var totalTokens = _totalPromptTokens + _totalCompletionTokens;
            summaryTable.AddRow($"[magenta]Total Tokens Used:[/] {totalTokens:N0}");
            summaryTable.AddRow($"[magenta]  Prompt Tokens:[/] {_totalPromptTokens:N0}");
            summaryTable.AddRow($"[magenta]  Completion Tokens:[/] {_totalCompletionTokens:N0}");
        }

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
