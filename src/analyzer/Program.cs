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
AnsiConsole.MarkupLine("[yellow]Batch processing jokes with local SLM model[/]\n");

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
var phi4ModelName = configuration["Phi4:ModelName"] ?? "phi-4";

AnsiConsole.MarkupLine($"[green]✓ Using {phi4ModelName} ({phi4ModelId}) at {phi4Endpoint}\n[/]");

// Configure database
var optionsBuilder = new DbContextOptionsBuilder<JokeDbContext>();
optionsBuilder.UseSqlServer(connectionString);

// Parse connection string to get database name
var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
var databaseName = connectionStringBuilder.InitialCatalog;
var serverName = connectionStringBuilder.DataSource;

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

// Configure Semantic Kernel with Phi-4
var kernelBuilder = Kernel.CreateBuilder();

// Create HttpClient with extended timeout for long-running AI requests
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromMinutes(10)
};

kernelBuilder.AddOpenAIChatCompletion(
    modelId: phi4ModelId,
    apiKey: "not-needed-for-local", // Local models don't need API keys
    endpoint: new Uri(phi4Endpoint),
    httpClient: httpClient
);
var kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Process jokes using RecordProcessor
var recordProcessor = new RecordProcessor(optionsBuilder.Options, chatCompletionService);
await recordProcessor.ProcessJokesAsync();

AnsiConsole.MarkupLine("\n[green]Processing complete![/]");
