//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Analyzer - Batch processes jokes using local Phi model or Azure OpenAI
// </summary>
//-----------------------------------------------------------------------
using JokeAnalyzer;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

// Display Banner
AnsiConsole.Write(new FigletText("Joke Analyzer").LeftJustified().Color(Color.Green));
AnsiConsole.MarkupLine("[yellow]Analyzing jokes with AI models to create images and categories[/]\n");

// Parse command-line arguments
var createImages = false;
var evaluateCategories = true;
string? categoryListArg = null;

for (var i = 0; i < args.Length; i++)
{
    var argLower = args[i].ToLowerInvariant();
    if (argLower is "--images" or "-i" or "/images")
    {
        createImages = true;
    }
    else if (argLower is "--categories" or "-c" or "/categories")
    {
        evaluateCategories = true;
    }
    else if ((argLower is "--category-list" or "-cl") && i + 1 < args.Length)
    {
        categoryListArg = args[++i];
    }
}

// Display active run modes
AnsiConsole.MarkupLine("[yellow]Run options:[/]");
AnsiConsole.MarkupLine($"  [cyan]Create image descriptions:[/] {(createImages ? "[green]Yes[/]" : "[grey]No[/]")}");
AnsiConsole.MarkupLine($"  [cyan]Evaluate joke categories:[/]  {(evaluateCategories ? "[green]Yes[/]" : "[grey]No[/]")}");

if (!createImages && !evaluateCategories)
{
    AnsiConsole.MarkupLine("\n[yellow]No processing modes enabled. Specify at least one of the following options:[/]");
    AnsiConsole.MarkupLine("[grey]  --images                    Generate image descriptions for jokes that do not have one[/]");
    AnsiConsole.MarkupLine("[grey]  --categories                Evaluate and simplify joke category assignments[/]");
    AnsiConsole.MarkupLine("[grey]  --category-list \"cat1,...\"   CSV list of allowed categories (optional, used with --categories)[/]");
    AnsiConsole.MarkupLine("\n[grey]Examples:[/]");
    AnsiConsole.MarkupLine("[grey]  dotnet run -- --images[/]");
    AnsiConsole.MarkupLine("[grey]  dotnet run -- --categories[/]");
    AnsiConsole.MarkupLine("[grey]  dotnet run -- --images --categories[/]");
    AnsiConsole.MarkupLine("[grey]  dotnet run -- --categories --category-list \"Dad,Puns,Science\"[/]");
    return;
}

// Build allowed categories list when category evaluation is enabled
List<string>? allowedCategories = null;
if (evaluateCategories)
{
    var rawList = categoryListArg ?? JokeAnalyzer.RecordProcessor.DefaultCategoryList;
    allowedCategories = rawList.Split(',')
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .ToList();
    AnsiConsole.MarkupLine($"\n[yellow]Category list ({allowedCategories.Count} categories):[/]");
    AnsiConsole.MarkupLine($"  [grey]{string.Join(", ", allowedCategories)}[/]");
}

AnsiConsole.WriteLine();

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    // .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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
    var azureDeploymentName = configuration["AzureOpenAI:DeploymentName"];
    var azureModelName = configuration["AzureOpenAI:ModelName"] ?? azureDeploymentName ?? "gpt-5-mini";
    var visualStudioTenantId = configuration["VisualStudioTenantId"];

    if (string.IsNullOrEmpty(azureEndpoint) || string.IsNullOrEmpty(azureDeploymentName))
    {
        AnsiConsole.MarkupLine("[red]  Error: Azure OpenAI configuration is incomplete. Please check AzureOpenAI settings in appsettings.json[/]");
        return;
    }

    // Get credentials using identity-based authentication
    var credentials = Utilities.GetCredentials(visualStudioTenantId ?? string.Empty);

    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: azureDeploymentName,
        endpoint: azureEndpoint,
        credentials: credentials,
        httpClient: httpClient
    );

    modelDisplayName = $"{azureModelName} (Azure OpenAI)";
    AnsiConsole.MarkupLine($"[green]✓ Using Cloud Model: {modelDisplayName} at {azureEndpoint}[/]");
    AnsiConsole.MarkupLine("[green]✓ Using identity-based authentication (Managed Identity/DefaultAzureCredential)[/]");
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
var recordProcessor = new RecordProcessor(
    optionsBuilder.Options,
    chatCompletionService,
    useAzureOpenAI,
    maxBatchSize,
    createImages,
    evaluateCategories,
    allowedCategories);
await recordProcessor.ProcessJokesAsync();

AnsiConsole.MarkupLine("\n[green]Processing complete![/]");
