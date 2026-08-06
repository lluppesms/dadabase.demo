using Azure.Core;
using GitHub.Copilot;

namespace DadABase.Web.Services;

/// <summary>
/// AI chat service backed by the GitHub Copilot SDK using a Foundry OpenAI-compatible endpoint.
/// </summary>
public class CopilotSdkChatService : IAiChatService
{
    private readonly IConfiguration config;
    private readonly DefaultAzureCredential credential;
    private readonly ILogger<CopilotSdkChatService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotSdkChatService"/> class.
    /// </summary>
    public CopilotSdkChatService(IConfiguration config, DefaultAzureCredential credential, ILogger<CopilotSdkChatService> logger)
    {
        this.config = config;
        this.credential = credential;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> CompleteAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
    {
        var foundryUrl = (config["AppSettings:CopilotSDK:FoundryResourceUrl"] ?? config["AppSettings:AzureOpenAI:Chat:Endpoint"])?.TrimEnd('/');
        var modelName = config["AppSettings:CopilotSDK:ModelName"] ?? config["AppSettings:AzureOpenAI:Chat:DeploymentName"];
        var tokenScope = config["AppSettings:CopilotSDK:TokenScope"] ?? "https://cognitiveservices.azure.com/.default";

        if (string.IsNullOrEmpty(foundryUrl) || string.IsNullOrEmpty(modelName))
        {
            throw new AiServiceConfigurationException("AI Chat Keys not found!");
        }

        var baseUrl = $"{foundryUrl}/openai/v1";
        var prompt = $"{systemPrompt}\n\n{userMessage}";

        logger.LogInformation("Calling Copilot SDK chat provider with model {ModelName} on {FoundryUrl}.", modelName, foundryUrl);

        await using CopilotClient client = new();
        await using CopilotSession session = await client.CreateSessionAsync(
            new SessionConfig
            {
                Model = modelName,
                Provider = new ProviderConfig
                {
                    Type = "openai",
                    BaseUrl = baseUrl,
                    WireApi = "responses",
                    BearerTokenProvider = async _ =>
                    {
                        var token = await credential.GetTokenAsync(
                            new TokenRequestContext([tokenScope]),
                            cancellationToken);

                        return token.Token;
                    },
                },
            },
            cancellationToken);

        var response = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = prompt, },
            cancellationToken: cancellationToken);

        return response?.Data?.Content ?? string.Empty;
    }
}
