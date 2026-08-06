using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Collections.Concurrent;

namespace DadABase.Web.Services;

/// <summary>
/// AI chat service backed by Azure OpenAI through Microsoft Agent Framework.
/// </summary>
public class AgentFrameworkChatService : IAiChatService
{
    private readonly string openaiEndpointUrl = string.Empty;
    private readonly Uri openaiEndpoint = null;
    private readonly string openaiDeploymentName = "gpt-4o";
    private readonly string openaiApiKey = string.Empty;
    private readonly string vsTenantId = string.Empty;
    private readonly ConcurrentDictionary<string, AIAgent> agents = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentFrameworkChatService"/> class.
    /// </summary>
    public AgentFrameworkChatService(IConfiguration config)
    {
        openaiEndpointUrl = config["AppSettings:AzureOpenAI:Chat:Endpoint"];
        openaiEndpoint = !string.IsNullOrEmpty(openaiEndpointUrl) ? new(config["AppSettings:AzureOpenAI:Chat:Endpoint"]) : null;
        openaiDeploymentName = config["AppSettings:AzureOpenAI:Chat:DeploymentName"];
        openaiApiKey = config["AppSettings:AzureOpenAI:Chat:ApiKey"];
        vsTenantId = config["VisualStudioTenantId"];
    }

    /// <inheritdoc/>
    public async Task<string> CompleteAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(openaiEndpointUrl) || string.IsNullOrEmpty(openaiDeploymentName))
        {
            Console.WriteLine("No OpenAI API keys available");
            throw new AiServiceConfigurationException("AI Chat Keys not found!");
        }

        var agent = agents.GetOrAdd(systemPrompt, CreateAgent);
        var response = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);
        return response.ToString();
    }

    private AIAgent CreateAgent(string systemPrompt)
    {
        AzureOpenAIClient azureClient;

        if (string.IsNullOrEmpty(openaiApiKey))
        {
            Console.WriteLine("Using Azure AD credentials for OpenAI Chat Client");
            azureClient = new AzureOpenAIClient(openaiEndpoint, Utilities.GetCredentials(vsTenantId));
        }
        else
        {
            Console.WriteLine("Using API Key for OpenAI Chat Client");
            azureClient = new AzureOpenAIClient(openaiEndpoint, new ApiKeyCredential(openaiApiKey));
        }

        var chatClient = azureClient.GetChatClient(openaiDeploymentName);

        return chatClient.AsAIAgent(
            name: "DadABaseAIHelper",
            instructions: systemPrompt
        );
    }
}
