//-----------------------------------------------------------------------
// <copyright file="RecordProcessor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
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
/// <param name="createImages">Whether to generate image descriptions for jokes that do not have one.</param>
/// <param name="evaluateCategories">Whether to evaluate and simplify joke category assignments.</param>
/// <param name="allowedCategories">
/// Explicit list of allowed categories for evaluation mode.
/// When null or empty, defaults to <see cref="DefaultCategoryList"/>.
/// </param>
public class RecordProcessor(
    DbContextOptions<JokeDbContext> dbContextOptions,
    IChatCompletionService chatCompletionService,
    bool trackTokens = false,
    int maxBatchSize = 100,
    bool createImages = false,
    bool evaluateCategories = false,
    IList<string>? allowedCategories = null)
{
    /// <summary>
    /// Default comma-separated list of joke categories used when evaluating categories without a custom list.
    /// </summary>
    public const string DefaultCategoryList =
        "Aging,Animals,Bad Puns,Bars,Camping,Chickens,Christmas,Chuck Norris,Compliments,Corona,Corporate," +
        "Dad,Did'ya Hear,Don't Say It!,Dyslexics,Education,Engineers,Fun Facts,Family,Farmers,Good Question!," +
        "Government,Halloween,Hipsters,Insults,Jobs,Knock Knock,Lawyers,Love and Marriage,Money,Pirates,Politics,Programmers," +
        "Quotes,Random,Religion,Riddles,Science,Space,Sports,Star Wars,Star Trek,Steven Wright,Technology,Thanksgiving," +
        "Weather,What do you call...,Words of Wisdom,Zombies,";

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
    private readonly bool _createImages = createImages;
    private readonly bool _evaluateCategories = evaluateCategories;
    private readonly IList<string> _allowedCategories = allowedCategories ?? [];

    // Running totals for token tracking
    private int _totalPromptTokens;
    private int _totalCompletionTokens;

    // Prompt template for generating an image description only
    private const string ImageOnlyPrompt =
        """
        Describe this joke so an artist can draw a picture of the mental image it creates.
        - Give clear instructions on how the scene should look and what objects should be included.
        - Draw it in a humorous cartoon format.
        - Make sure the description does not ask for anything violent, sexual, or political.
        - Keep the scene description under 250 words.

        Joke: {0}

        Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
        {{"imageDescription": "your description here"}}
        """;

    // Prompt template for evaluating categories only (restricted to the allowed list)
    private const string CategoryEvaluationPrompt =
        """
        Analyze the following joke and identify the most appropriate category from the provided list.

        Rules:
        - Choose the SINGLE BEST category that most accurately describes this joke.
        - Only assign a second category if there is a STRONG and clear correlation (not just a weak connection).
        - ONLY use categories from the provided list. Do NOT suggest new categories.
        - Try not to use the "Dad" category unless the joke clearly references a father figure.
        - Available categories: {0}

        Joke: {1}

        Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
        {{"primaryCategory": "best matching category", "secondaryCategory": "second category only if strongly relevant, otherwise empty string"}}
        """;

    // Combined prompt for image description and free-form category assignment (when evaluateCategories is off)
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

    // Combined prompt for image description with restricted category evaluation
    private const string CombinedEvaluationPrompt =
        """
        Analyze the following joke and provide two things:

        1. IMAGE DESCRIPTION: Describe this joke so an artist can draw a picture of the mental image it creates.
           - Give clear instructions on how the scene should look and what objects should be included.
           - Draw it in a humorous cartoon format.
           - Make sure the description does not ask for anything violent, sexual, or political.
           - Keep the scene description under 250 words.

        2. CATEGORIES: Identify the most appropriate category from the provided list only.
           - Choose the SINGLE BEST category that most accurately describes this joke.
           - Only assign a second category if there is a STRONG and clear correlation (not just a weak connection).
           - ONLY use categories from the provided list. Do NOT suggest new categories.
           - Try not to use the "Dad" category unless the joke clearly references a father figure.
           - Available categories: {0}

        Joke: {1}

        Respond ONLY with valid JSON in this exact format (no markdown, no code blocks):
        {{"imageDescription": "your description here", "primaryCategory": "best matching category", "secondaryCategory": "second category only if strongly relevant, otherwise empty string"}}
        """;

    // Response model for image-only analysis
    private class ImageOnlyResponse
    {
        public string ImageDescription { get; set; } = string.Empty;
    }

    // Response model for category evaluation
    private class CategoryEvaluationResponse
    {
        public string PrimaryCategory { get; set; } = string.Empty;
        public string SecondaryCategory { get; set; } = string.Empty;
    }

    // Response model for combined analysis (createImages && !evaluateCategories)
    private class JokeAnalysisResponse
    {
        public string ImageDescription { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = [];
    }

    // Response model for combined analysis with restricted category evaluation
    private class CombinedEvaluationResponse
    {
        public string ImageDescription { get; set; } = string.Empty;
        public string PrimaryCategory { get; set; } = string.Empty;
        public string SecondaryCategory { get; set; } = string.Empty;
    }

    /// <summary>
    /// Processes active jokes according to the enabled processing modes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessJokesAsync()
    {
        if (!_createImages && !_evaluateCategories)
        {
            AnsiConsole.MarkupLine("[yellow]No processing modes enabled. Use --images and/or --categories to enable processing.[/]");
            return;
        }

        await using var context = new JokeDbContext(_dbContextOptions);

        // Build query based on processing modes
        IQueryable<Joke> jokesQuery;

        if (_evaluateCategories && _createImages)
        {
            // Both modes: process jokes missing images OR missing categories
            jokesQuery = context.Jokes
                .Where(j => j.ActiveInd == "Y")
                .Where(j => (j.ImageTxt == null || j.ImageTxt == "") || 
                            !context.JokeJokeCategories.Any(jjc => jjc.JokeId == j.JokeId))
                .OrderBy(j => j.JokeId);
        }
        else if (_evaluateCategories)
        {
            // Categories only: process jokes without assigned categories
            jokesQuery = context.Jokes
                .Where(j => j.ActiveInd == "Y")
                .Where(j => !context.JokeJokeCategories.Any(jjc => jjc.JokeId == j.JokeId))
                .OrderBy(j => j.JokeId);
        }
        else
        {
            // Images only: process jokes without images
            jokesQuery = context.Jokes
                .Where(j => j.ActiveInd == "Y" && (j.ImageTxt == null || j.ImageTxt == ""))
                .OrderBy(j => j.JokeId);
        }

        var jokes = _maxBatchSize > 0
            ? await jokesQuery.Take(_maxBatchSize).ToListAsync()
            : await jokesQuery.ToListAsync();

        var totalJokes = jokes.Count;
        var modeLabel = (_createImages, _evaluateCategories) switch
        {
            (true, true) => "jokes missing images or categories",
            (true, false) => "jokes needing image descriptions",
            (false, true) => "jokes without assigned categories",
            _ => "jokes to process"
        };
        AnsiConsole.MarkupLine($"[cyan]Found {totalJokes} {modeLabel}[/]\n");

        if (totalJokes == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No jokes found to process[/]");
            return;
        }

        // Load existing categories from the database
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

    /// <summary>
    /// Routes a single joke to the appropriate processing method based on active modes.
    /// </summary>
    private async Task ProcessSingleJokeAsync(JokeDbContext context, Joke joke, CategoryState categoryState)
    {
        var needsImage = _createImages && string.IsNullOrEmpty(joke.ImageTxt);
        var needsCategories = _evaluateCategories;

        if (needsImage && needsCategories)
        {
            await ProcessCombinedWithEvaluationAsync(context, joke, categoryState);
        }
        else if (needsImage)
        {
            await ProcessCombinedAsync(context, joke, categoryState);
        }
        else if (needsCategories)
        {
            await ProcessCategoryEvaluationAsync(context, joke, categoryState);
        }
        else
        {
            // createImages is on but the joke already has an image and evaluateCategories is off
            AnsiConsole.MarkupLine($"  [yellow]⊘ Skipping - image description already exists[/]");
        }
    }

    /// <summary>
    /// Generates an image description and assigns free-form categories (createImages &amp;&amp; !evaluateCategories).
    /// </summary>
    private async Task ProcessCombinedAsync(JokeDbContext context, Joke joke, CategoryState categoryState)
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
        var analysisResult = ParseJsonResponse<JokeAnalysisResponse>(response.Content);

        joke.ImageTxt = analysisResult.ImageDescription;
        joke.ChangeDateTime = DateTime.UtcNow;
        joke.ChangeUserName = "JokeAnalyzer";

        AnsiConsole.MarkupLine($"  [green]✓ Generated image description[/]");
        AnsiConsole.MarkupLine($"  [gray]  {Markup.Escape(joke.ImageTxt ?? string.Empty)}[/]");

        AnsiConsole.MarkupLine($"  [green]✓ Identified categories: {string.Join(", ", analysisResult.Categories)}[/]");

        // Process categories (may create new ones)
        foreach (var categoryName in analysisResult.Categories)
        {
            await ProcessCategoryAsync(context, joke, categoryName, categoryState);
        }

        // Save changes
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates an image description only (createImages only, no category evaluation).
    /// </summary>
    private async Task ProcessImageOnlyAsync(JokeDbContext context, Joke joke)
    {
        AnsiConsole.MarkupLine($"  [blue]Generating image description...[/]");

        var prompt = string.Format(ImageOnlyPrompt, joke.JokeTxt);
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);

        var response = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            new OpenAIPromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object> { ["max_completion_tokens"] = 400 }
            }
        );

        DisplayTokenUsage(response, "Image Description");

        var result = ParseJsonResponse<ImageOnlyResponse>(response.Content);
        if (!string.IsNullOrWhiteSpace(result.ImageDescription))
        {
            joke.ImageTxt = result.ImageDescription;
            joke.ChangeDateTime = DateTime.UtcNow;
            joke.ChangeUserName = "JokeAnalyzer";
            await context.SaveChangesAsync();
            AnsiConsole.MarkupLine($"  [green]✓ Generated image description[/]");
            AnsiConsole.MarkupLine($"  [gray]  {Markup.Escape(joke.ImageTxt)}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"  [yellow]⚠ Could not generate image description[/]");
        }
    }

    /// <summary>
    /// Evaluates categories for a joke using the restricted allowed-category list,
    /// replacing all existing category assignments (!createImages &amp;&amp; evaluateCategories).
    /// </summary>
    private async Task ProcessCategoryEvaluationAsync(JokeDbContext context, Joke joke, CategoryState categoryState)
    {
        AnsiConsole.MarkupLine($"  [blue]Evaluating categories...[/]");

        var allowedCategoryNames = _allowedCategories.Count > 0
            ? string.Join(", ", _allowedCategories)
            : categoryState.CategoryNames;

        var prompt = string.Format(CategoryEvaluationPrompt, allowedCategoryNames, joke.JokeTxt);
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);

        var response = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            new OpenAIPromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object> { ["max_completion_tokens"] = 150 }
            }
        );

        DisplayTokenUsage(response, "Category Evaluation");

        var result = ParseJsonResponse<CategoryEvaluationResponse>(response.Content);
        await AssignEvaluatedCategoriesAsync(context, joke, result.PrimaryCategory, result.SecondaryCategory, categoryState);
    }

    /// <summary>
    /// Generates an image description (if missing) and evaluates categories with the restricted allowed list
    /// (createImages &amp;&amp; evaluateCategories).
    /// </summary>
    private async Task ProcessCombinedWithEvaluationAsync(JokeDbContext context, Joke joke, CategoryState categoryState)
    {
        var allowedCategoryNames = _allowedCategories.Count > 0
            ? string.Join(", ", _allowedCategories)
            : categoryState.CategoryNames;

        if (string.IsNullOrEmpty(joke.ImageTxt))
        {
            // Joke needs both image and category evaluation – use the combined evaluation prompt
            AnsiConsole.MarkupLine($"  [blue]Analyzing joke (image + category evaluation)...[/]");

            var prompt = string.Format(CombinedEvaluationPrompt, allowedCategoryNames, joke.JokeTxt);
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            var response = await _chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                new OpenAIPromptExecutionSettings
                {
                    ExtensionData = new Dictionary<string, object> { ["max_completion_tokens"] = 500 }
                }
            );

            DisplayTokenUsage(response, "Combined Evaluation");

            var result = ParseJsonResponse<CombinedEvaluationResponse>(response.Content);

            if (!string.IsNullOrWhiteSpace(result.ImageDescription))
            {
                joke.ImageTxt = result.ImageDescription;
                joke.ChangeDateTime = DateTime.UtcNow;
                joke.ChangeUserName = "JokeAnalyzer";
                AnsiConsole.MarkupLine($"  [green]✓ Generated image description[/]");
                AnsiConsole.MarkupLine($"  [gray]  {Markup.Escape(joke.ImageTxt)}[/]");
            }

            await AssignEvaluatedCategoriesAsync(context, joke, result.PrimaryCategory, result.SecondaryCategory, categoryState);
        }
        else
        {
            // Joke already has an image – only evaluate categories
            AnsiConsole.MarkupLine($"  [yellow]⊘ Image already exists - evaluating categories only[/]");
            await ProcessCategoryEvaluationAsync(context, joke, categoryState);
        }
    }

    /// <summary>
    /// Clears all existing category assignments for a joke and assigns the AI-evaluated categories,
    /// restricting to the allowed-categories list.
    /// </summary>
    private async Task AssignEvaluatedCategoriesAsync(
        JokeDbContext context,
        Joke joke,
        string primaryCategory,
        string secondaryCategory,
        CategoryState categoryState)
    {
        // Remove all existing category assignments for this joke
        var existingAssignments = await context.JokeJokeCategories
            .Where(jjc => jjc.JokeId == joke.JokeId)
            .ToListAsync();

        if (existingAssignments.Count > 0)
        {
            context.JokeJokeCategories.RemoveRange(existingAssignments);
            AnsiConsole.MarkupLine($"  [gray]  Removed {existingAssignments.Count} existing category assignment(s)[/]");
        }

        // Build the list of categories to assign (primary always; secondary only when provided)
        var categoriesToAssign = new List<string>();
        if (!string.IsNullOrWhiteSpace(primaryCategory))
        {
            categoriesToAssign.Add(primaryCategory.Trim());
        }
        if (!string.IsNullOrWhiteSpace(secondaryCategory))
        {
            categoriesToAssign.Add(secondaryCategory.Trim());
        }

        if (categoriesToAssign.Count == 0)
        {
            AnsiConsole.MarkupLine($"  [yellow]⚠ No valid categories returned by AI[/]");
            await context.SaveChangesAsync();
            return;
        }

        AnsiConsole.MarkupLine($"  [green]✓ Assigning categories: {string.Join(", ", categoriesToAssign)}[/]");

        foreach (var categoryName in categoriesToAssign)
        {
            // Validate against the allowed list when one is configured
            if (_allowedCategories.Count > 0 &&
                !_allowedCategories.Any(c => c.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine($"  [yellow]⚠ Skipping '{Markup.Escape(categoryName)}' - not in allowed categories list[/]");
                continue;
            }

            await AssignSingleRestrictedCategoryAsync(context, joke, categoryName, categoryState);
        }

        // Update the joke's change timestamp to reflect the category updates
        joke.ChangeDateTime = DateTime.UtcNow;
        joke.ChangeUserName = "JokeAnalyzer";

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Ensures a category exists in the database (creating it if needed) and links it to the joke.
    /// Only called in evaluation mode – does not create categories outside the allowed list.
    /// </summary>
    private async Task AssignSingleRestrictedCategoryAsync(
        JokeDbContext context,
        Joke joke,
        string categoryName,
        CategoryState categoryState)
    {
        var category = categoryState.ExistingCategories.FirstOrDefault(c =>
            c.JokeCategoryTxt.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

        if (category == null)
        {
            // Create the category if it is in the allowed list but not yet in the database
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

        var jokeCategory = new JokeJokeCategory
        {
            JokeId = joke.JokeId,
            JokeCategoryId = category.JokeCategoryId,
            CreateDateTime = DateTime.UtcNow,
            CreateUserName = "JokeAnalyzer"
        };
        context.JokeJokeCategories.Add(jokeCategory);
        AnsiConsole.MarkupLine($"  [green]  ✓ Assigned to category: {categoryName}[/]");
    }

    /// <summary>
    /// Deserializes an AI JSON response into the given type, stripping markdown code fences if present.
    /// </summary>
    private static T ParseJsonResponse<T>(string? content) where T : new()
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new T();
        }

        try
        {
            var jsonContent = content.Trim();
            if (jsonContent.StartsWith("```"))
            {
                var lines = jsonContent.Split('\n');
                jsonContent = string.Join('\n', lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<T>(
                jsonContent,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result ?? new T();
        }
        catch (System.Text.Json.JsonException ex)
        {
            AnsiConsole.MarkupLine($"  [yellow]⚠ Failed to parse JSON response: {Markup.Escape(ex.Message)}[/]");
            AnsiConsole.MarkupLine($"  [gray]  Raw response: {Markup.Escape(content)}[/]");
            return new T();
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
