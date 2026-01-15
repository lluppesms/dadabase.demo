//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Analyzer - Batch processes jokes using local Phi model or Azure OpenAI
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
AnsiConsole.MarkupLine("[yellow]Analyzing jokes with an AI models to create images and categories[/]\n");

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

// Configure database
var optionsBuilder = new DbContextOptionsBuilder<JokeDbContext>();
optionsBuilder.UseSqlServer(connectionString);

// Parse connection string to get database name
var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
var databaseName = connectionStringBuilder.InitialCatalog;
var serverName = connectionStringBuilder.DataSource;

AnsiConsole.MarkupLine("[yellow]Connecting to database...[/]");
// Test database connection
try
{
    await using (var testContext = new JokeDbContext(optionsBuilder.Options))
    {
        await testContext.Database.CanConnectAsync();
        AnsiConsole.MarkupLine($"[green]✓ Database connection successful to {serverName}/{databaseName}\n[/]");
    }
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]✗ Database connection failed: {ex.Message}[/]");
    AnsiConsole.MarkupLine("[yellow]Please check your connection string in appsettings.json[/]");
    return;
}

// Create HttpClient with extended timeout for long-running AI requests
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromMinutes(10)
};

// Determine model provider
var modelProviderStr = configuration["ModelProvider"] ?? "Local";
var useAzureOpenAI = modelProviderStr.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase);
AnsiConsole.MarkupLine($"[yellow]Using {modelProviderStr} model...[/]");

// Configure Semantic Kernel based on provider
var kernelBuilder = Kernel.CreateBuilder();

string modelDisplayName;

if (useAzureOpenAI)
{
    // Azure OpenAI configuration
    var azureEndpoint = configuration["AzureOpenAI:Endpoint"];
    var azureApiKey = configuration["AzureOpenAI:ApiKey"];
    var azureDeploymentName = configuration["AzureOpenAI:DeploymentName"];
    var azureModelName = configuration["AzureOpenAI:ModelName"] ?? azureDeploymentName ?? "gpt-5-mini";

    if (string.IsNullOrEmpty(azureEndpoint) || string.IsNullOrEmpty(azureApiKey) || string.IsNullOrEmpty(azureDeploymentName))
    {
        AnsiConsole.MarkupLine("[red]  Error: Azure OpenAI configuration is incomplete. Please check AzureOpenAI settings in appsettings.json[/]");
        return;
    }

    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: azureDeploymentName,
        endpoint: azureEndpoint,
        apiKey: azureApiKey,
        httpClient: httpClient
    );

    modelDisplayName = $"{azureModelName} (Azure OpenAI)";
    AnsiConsole.MarkupLine($"[green]✓ Using Cloud Model: {modelDisplayName} at {azureEndpoint}[/]");
    AnsiConsole.MarkupLine("[green]✓ Token tracking enabled for cloud model[/]\n");
}
else
{
    // Local model configuration
    var localEndpoint = configuration["LocalModel:Endpoint"] ?? "http://localhost:1234/v1";
    var localModelId = configuration["LocalModel:ModelId"] ?? "phi-4";
    var localModelName = configuration["LocalModel:ModelName"] ?? "phi-4";

    kernelBuilder.AddOpenAIChatCompletion(
        modelId: localModelId,
        apiKey: "not-needed-for-local",
        endpoint: new Uri(localEndpoint),
        httpClient: httpClient
    );

    modelDisplayName = $"{localModelName} ({localModelId})";
    AnsiConsole.MarkupLine($"[green]✓ Using Local Model: {modelDisplayName} at {localEndpoint}[/]");
    AnsiConsole.MarkupLine("[green]✓ Token tracking disenabled for local model[/]\n");
}

var kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Get batch size from configuration (0 = no limit)
var maxBatchSize = configuration.GetValue<int>("MaxBatchSize", 100);
if (maxBatchSize == 0)
{
    AnsiConsole.MarkupLine("[yellow]Batch size: No limit (processing all records)[/]\n");
}
else
{
    AnsiConsole.MarkupLine($"[yellow]Batch size: {maxBatchSize} records[/]\n");
}

// Process jokes using RecordProcessor
var recordProcessor = new RecordProcessor(optionsBuilder.Options, chatCompletionService, useAzureOpenAI, maxBatchSize);
await recordProcessor.ProcessJokesAsync();

AnsiConsole.MarkupLine("\n[green]Processing complete![/]");
