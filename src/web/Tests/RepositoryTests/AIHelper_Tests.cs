namespace DadABase.Tests;

using Azure.Identity;
using DadABase.Web.Helpers;
using DadABase.Web.Repositories;
using DadABase.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

[ExcludeFromCodeCoverage]
public class AIHelper_Tests
{
    [Fact]
    public async Task SuggestCategories_FiltersChatResponseToAvailableCategories()
    {
        var chatService = new StubAiChatService("animals, not-a-category, Puns, Animals");
        var imageService = new Mock<IAiImageService>();
        var helper = new AIHelper(chatService, imageService.Object);

        var (suggestedCategories, success, message) = await helper.SuggestCategories(
            "Why did the chicken cross the playground?",
            ["Animals", "Puns", "Food"]);

        Assert.True(success);
        Assert.Equal(string.Empty, message);
        Assert.Equal(["Animals", "Puns"], suggestedCategories);
        Assert.Contains("Available categories: Animals, Puns, Food", chatService.LastUserMessage);
    }

    [Fact]
    public async Task AnalyzeJoke_ParsesCategoriesAndSceneDescription()
    {
        var response = "CATEGORIES: Puns, Animals, Food\nSCENE: A cartoon chicken crossing a playground.\nAdd a slide in the background.";
        var chatService = new StubAiChatService(response);
        var imageService = new Mock<IAiImageService>();
        var helper = new AIHelper(chatService, imageService.Object);

        var (suggestedCategories, sceneDescription, success, message) = await helper.AnalyzeJoke(
            "Why did the chicken cross the playground? To get to the other slide.",
            ["Animals", "Puns", "Food"]);

        Assert.True(success);
        Assert.Equal(string.Empty, message);
        Assert.Equal(["Puns", "Animals"], suggestedCategories);
        Assert.Equal("A cartoon chicken crossing a playground.\nAdd a slide in the background.", sceneDescription);
    }

    [Fact]
    public async Task GenerateAnImage_DelegatesToImageService()
    {
        var chatService = new StubAiChatService(string.Empty);
        var imageService = new Mock<IAiImageService>();
        imageService
            .Setup(service => service.GenerateAnImage("cartoon scene", 42))
            .ReturnsAsync(("/api/images/jokes/42.png", true, string.Empty));
        var helper = new AIHelper(chatService, imageService.Object);

        var (imageUrl, success, message) = await helper.GenerateAnImage("cartoon scene", 42);

        Assert.True(success);
        Assert.Equal("/api/images/jokes/42.png", imageUrl);
        Assert.Equal(string.Empty, message);
        imageService.Verify(service => service.GenerateAnImage("cartoon scene", 42), Times.Once);
    }

    [Fact]
    public void AddAiServices_DefaultsToAgentFrameworkProvider()
    {
        var provider = BuildServiceProvider([]);

        var chatService = provider.GetRequiredService<IAiChatService>();

        Assert.IsType<AgentFrameworkChatService>(chatService);
    }

    [Fact]
    public void AddAiServices_SelectsCopilotSdkProviderFromConfig()
    {
        var provider = BuildServiceProvider(new Dictionary<string, string>
        {
            ["AppSettings:AiServiceProvider"] = "CopilotSDK"
        });

        var chatService = provider.GetRequiredService<IAiChatService>();

        Assert.IsType<CopilotSdkChatService>(chatService);
    }

    private static ServiceProvider BuildServiceProvider(Dictionary<string, string> settings)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton(new DefaultAzureCredential());
        services.AddLogging();
        services.AddAiServices(config);

        return services.BuildServiceProvider();
    }

    private sealed class StubAiChatService : IAiChatService
    {
        private readonly string response;

        public StubAiChatService(string response)
        {
            this.response = response;
        }

        public string LastSystemPrompt { get; private set; } = string.Empty;

        public string LastUserMessage { get; private set; } = string.Empty;

        public Task<string> CompleteAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
        {
            LastSystemPrompt = systemPrompt;
            LastUserMessage = userMessage;
            return Task.FromResult(response);
        }
    }
}
