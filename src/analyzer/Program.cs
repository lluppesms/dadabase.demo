//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Analyzer - Batch processes jokes using Phi-4 local model
// </summary>
//-----------------------------------------------------------------------
using JokeAnalyzer;
using JokeAnalyzer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Display Banner
AnsiConsole.Write(new FigletText("Joke Analyzer").LeftJustified().Color(Color.Green));
AnsiConsole.MarkupLine("[yellow]Batch processing jokes with Phi-4 local model[/]\n");

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    AnsiConsole.MarkupLine("[red]Error: Connection string not found in appsettings.json[/]");
    return;
}

var phi4Endpoint = configuration["Phi4:Endpoint"] ?? "http://localhost:1234/v1";
var phi4ModelId = configuration["Phi4:ModelId"] ?? "phi-4";

// Configure database
var optionsBuilder = new DbContextOptionsBuilder<JokeDbContext>();
optionsBuilder.UseSqlServer(connectionString);

// Test database connection
try
{
    await using (var testContext = new JokeDbContext(optionsBuilder.Options))
    {
        await testContext.Database.CanConnectAsync();
        AnsiConsole.MarkupLine("[green]✓ Database connection successful[/]");
    }
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]✗ Database connection failed: {ex.Message}[/]");
    AnsiConsole.MarkupLine("[yellow]Please check your connection string in appsettings.json[/]");
    return;
}

// Configure Semantic Kernel with Phi-4
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: phi4ModelId,
    apiKey: "not-needed-for-local", // Local models don't need API keys
    endpoint: new Uri(phi4Endpoint)
);
var kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Prompt template for image description generation
const string imageDescriptionPrompt = 
    "You are going to be told a funny joke or a humorous line or an insightful quote. " +
    "It is your responsibility to describe that joke so that an artist can draw a picture of the mental image that this joke creates. " +
    "Give clear instructions on how the scene should look and what objects should be included in the scene. " +
    "Instruct the artist to draw it in a humorous cartoon format. " +
    "Make sure the description does not ask for anything violent, sexual, or political so that it does not violate safety rules. " +
    "Keep the scene description under 250 words or less.\n\n" +
    "Joke: {0}\n\n" +
    "Image Description:";

// Prompt template for category evaluation
const string categoryPrompt =
    "Given the following joke, identify all appropriate categories it belongs to. " +
    "Choose from existing categories if they fit, or suggest new categories if needed. " +
    "Return only the category names, separated by commas.\n\n" +
    "Existing categories: {0}\n\n" +
    "Joke: {1}\n\n" +
    "Categories:";

// Process jokes
await using (var context = new JokeDbContext(optionsBuilder.Options))
{
    // Get all active jokes
    var jokes = await context.Jokes
        .Where(j => j.ActiveInd == "Y")
        .OrderBy(j => j.JokeId)
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
    
    var categoryNames = string.Join(", ", existingCategories.Select(c => c.JokeCategoryTxt));

    var processedCount = 0;
    var updatedCount = 0;
    var errorCount = 0;

    await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[green]Processing jokes[/]", maxValue: totalJokes);

            foreach (var joke in jokes)
            {
                processedCount++;
                AnsiConsole.MarkupLine($"[cyan]Processing record {processedCount} of {totalJokes}[/] - Joke ID: {joke.JokeId}");

                try
                {
                    // Generate image description only if ImageTxt is empty
                    if (string.IsNullOrWhiteSpace(joke.ImageTxt))
                    {
                        var imagePrompt = string.Format(imageDescriptionPrompt, joke.JokeTxt);
                        var imageHistory = new ChatHistory();
                        imageHistory.AddUserMessage(imagePrompt);

                        var imageResponse = await chatCompletionService.GetChatMessageContentAsync(
                            imageHistory,
                            new OpenAIPromptExecutionSettings { MaxTokens = 300, Temperature = 0.7 }
                        );

                        joke.ImageTxt = imageResponse.Content?.Trim();
                        joke.ChangeDateTime = DateTime.UtcNow;
                        joke.ChangeUserName = "JokeAnalyzer";

                        AnsiConsole.MarkupLine($"  [green]✓ Generated image description[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"  [yellow]⊘ Image description already exists, skipping[/]");
                    }

                    // Evaluate categories
                    var catPrompt = string.Format(categoryPrompt, categoryNames, joke.JokeTxt);
                    var catHistory = new ChatHistory();
                    catHistory.AddUserMessage(catPrompt);

                    var categoryResponse = await chatCompletionService.GetChatMessageContentAsync(
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
                        // Check if category exists
                        var category = existingCategories.FirstOrDefault(c => 
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
                            existingCategories.Add(category);
                            categoryNames = string.Join(", ", existingCategories.Select(c => c.JokeCategoryTxt));
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

                    // Save changes
                    await context.SaveChangesAsync();
                    updatedCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    AnsiConsole.MarkupLine($"  [red]✗ Error processing joke {joke.JokeId}: {ex.Message}[/]");
                }

                task.Increment(1);
            }
        });

    // Display summary
    AnsiConsole.WriteLine();
    var summaryTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Green)
        .AddColumn(new TableColumn("[yellow]Summary[/]").Centered());

    summaryTable.AddRow($"[cyan]Total Records:[/] {totalJokes}");
    summaryTable.AddRow($"[green]Successfully Processed:[/] {updatedCount}");
    summaryTable.AddRow($"[red]Errors:[/] {errorCount}");

    AnsiConsole.Write(summaryTable);
}

AnsiConsole.MarkupLine("\n[green]Processing complete![/]");
