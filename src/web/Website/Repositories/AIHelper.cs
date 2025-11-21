using Azure.AI.OpenAI;
using Azure.Identity;
using DadABase.Web.Models.AIModels;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;

namespace DadABase.Web.Repositories;

/// <summary>
/// Chat Agent to manage conversations with Azure OpenAI Service
/// </summary>
public class AIHelper : IAIHelper
{
    #region Variables
    private readonly string openaiEndpointUrl = string.Empty;
    private readonly Uri openaiEndpoint = null;
    private readonly string openaiDeploymentName = "gpt-4o";
    private readonly string openaiApiKey = string.Empty;

    private readonly int openaiMaxTokens = 100;
    private readonly float openaiTemperature = 0.7f;
    private readonly float openaiTopP = 0.95f;

    private readonly string openaiImageEndpointUrl = string.Empty;
    private readonly Uri openaiImageEndpoint = null;
    private readonly string openaiImageDeploymentName = "dall-e-3";
    private readonly string openaiImageApiKey = string.Empty;

    private ChatClient chatClient = null;
    private ChatCompletionConfiguration chatCompletionConfiguration = null;
    private ChatCompletionOptions chatRequestOptions = null;

    private ImageClient imageGenerator = null;

    private readonly string vsTenantId = string.Empty;
    private DefaultAzureCredential credential = null;
    //private readonly ILogger logger;
    #endregion

    private const string JokeImageGeneratorPrompt =
        "You are going to be told a funny joke or a humorous line or an insightful quote. " +
        "It is your responsibility to describe that joke so that an artist can draw a picture of the mental image that this joke creates. " +
        "Give clear instructions on how the scene should look and what objects should be included in the scene." +
        "Instruct the artist to draw it in a humorous cartoon format." +
        "Make sure the description does not ask for anything violent, sexual, or political so that it does not violate safety rules. " +
        "Keep the scene description under 250 words or less.";


    /// <summary>
    /// Give it a joke and get back an image description
    /// </summary>
    /// <returns></returns>
    public async Task<(string description, bool success, string message)> GetJokeSceneDescription(string jokeText)
    {
        var imageDescription = string.Empty;

        try
        {
            if (!InitializeChatAgent())
            {
                return (string.Empty, false, "AI Chat Keys not found!");
            }

            var chatConversation = new List<ChatMessage>
            {
               ChatMessage.CreateSystemMessage(JokeImageGeneratorPrompt),
               ChatMessage.CreateUserMessage(jokeText)
            };
            // Get latest system message from AI configuration
            var chatMessages = new List<ChatMessage>(GetChatMessages(chatCompletionConfiguration));
            chatMessages.AddRange(chatConversation);

            // Get AI response and add it to chat conversation
            var response = await chatClient.CompleteChatAsync(chatMessages, chatRequestOptions);

            imageDescription = response.Value.Content[0].Text;
            chatConversation.Add(ChatMessage.CreateAssistantMessage(imageDescription));
            Console.WriteLine($"Joke: {jokeText} \nImage description {imageDescription}");
            return (imageDescription, true, string.Empty);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error during description generation: {ex.Message}";
            Console.WriteLine(errorMessage);
            // logger.LogError(errorMessage, "AIHelper");
            return (imageDescription, false, "Could not generate an image description - see log for details!");
        }
    }

    /// <summary>
    /// Give this a description and get back an generated image URL
    /// </summary>
    /// <returns></returns>
    public async Task<(string, bool, string)> GenerateAnImage(string imageDescription)
    {
        var imageUrl = string.Empty;
        try
        {
            if (!InitializeImageGenerator())
            {
                return (string.Empty, false, "AI Image Keys not found!");
            }

            var imageResult = await imageGenerator.GenerateImageAsync(imageDescription, new()
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1024xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Uri
            });
            var image = imageResult.Value;
            imageUrl = image.ImageUri.ToString();
            Console.WriteLine($"Generated Image URL {imageUrl}");
            return (imageUrl, true, string.Empty);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error during image generation: {ex.Message} Prompt: {imageDescription}";
            Console.WriteLine(errorMessage);
            // logger.LogError(errorMessage, "AIHelper");

            var sorryMessage = "Sorry - I can't even imagine drawing that picture...!  Try again with a different joke!";
            if (ex.Message.Contains("safety system", StringComparison.CurrentCultureIgnoreCase))
            {
                sorryMessage += " (safety violation)";
            }
            if (ex.Message.Contains("content filter", StringComparison.CurrentCultureIgnoreCase))
            {
                sorryMessage += " (content filter violation)";
            }
            return (imageDescription, false, sorryMessage);
        }
    }

    #region Helper Methods
    /// <summary>
    /// Initialization
    /// </summary>
    public AIHelper(IConfiguration config) // , ILogger systemLogger)
    {
        // logger = systemLogger;

        openaiEndpointUrl = config["AppSettings:AzureOpenAI:Chat:Endpoint"];
        openaiEndpoint = !string.IsNullOrEmpty(openaiEndpointUrl) ? new(config["AppSettings:AzureOpenAI:Chat:Endpoint"]) : null;
        openaiDeploymentName = config["AppSettings:AzureOpenAI:Chat:DeploymentName"];
        openaiApiKey = config["AppSettings:AzureOpenAI:Chat:ApiKey"];
        openaiMaxTokens = int.TryParse(config["AppSettings:AzureOpenAI:Chat:MaxTokens"], out var parsedMaxTokens) ? parsedMaxTokens : 300;
        openaiTemperature = float.TryParse(config["AppSettings:AzureOpenAI:Chat:Temperature"], out var parsedTemperature) ? parsedTemperature : 0.7f;
        openaiTopP = float.TryParse(config["AppSettings:AzureOpenAI:Chat:TopP"], out var topP) ? topP : 0.95f;

        openaiImageEndpointUrl = config["AppSettings:AzureOpenAI:Image:Endpoint"];
        openaiImageEndpoint = !string.IsNullOrEmpty(openaiImageEndpointUrl) ? new(config["AppSettings:AzureOpenAI:Image:Endpoint"]) : null;
        openaiImageDeploymentName = config["AppSettings:AzureOpenAI:Image:DeploymentName"];
        openaiImageApiKey = config["AppSettings:AzureOpenAI:Image:ApiKey"];

        vsTenantId = config["VisualStudioTenantId"];
    }

    /// <summary>
    /// Helper method to convert configuration messages to ChatMessage objects
    /// </summary>
    private static IEnumerable<ChatMessage> GetChatMessages(ChatCompletionConfiguration chatCompletionConfiguration)
    {
        return chatCompletionConfiguration.Messages.Select<ChatCompletionMessage, ChatMessage>(message => message.Role switch
        {
            "system" => ChatMessage.CreateSystemMessage(message.Content),
            "user" => ChatMessage.CreateUserMessage(message.Content),
            "assistant" => ChatMessage.CreateAssistantMessage(message.Content),
            _ => throw new ArgumentException($"Unknown role: {message.Role}", nameof(message.Role))
        });
    }

    /// <summary>
    /// Initialize the chat agent
    /// </summary>
    private bool InitializeChatAgent()
    {
        if (chatClient != null) return true;

        if (string.IsNullOrEmpty(openaiEndpointUrl) || string.IsNullOrEmpty(openaiDeploymentName))
        {
            Console.WriteLine("No OpenAI API keys available");
            return false;
        }
        AzureOpenAIClient _chatClientHost = null;
        try
        {
            if (string.IsNullOrEmpty(openaiApiKey))
            {
                _chatClientHost = new AzureOpenAIClient(openaiEndpoint, GetCredentials());
            }
            else
            {
                _chatClientHost = new(openaiEndpoint, new ApiKeyCredential(openaiApiKey));
            }

            chatClient = _chatClientHost.GetChatClient(openaiDeploymentName);
            // alternate way of doing this...
            //chatClient = new(
            //    model: openaiDeploymentName,
            //    credential: new ApiKeyCredential(openaiApiKey),
            //    options: new OpenAIClientOptions(){ Endpoint = openaiEndpoint }
            //);

            chatCompletionConfiguration = new ChatCompletionConfiguration
            {
                MaxTokens = openaiMaxTokens,
                Temperature = openaiTemperature,
                TopP = openaiTopP,
                Messages = [
                    new() {
                    Role = "system",
                    Content = JokeImageGeneratorPrompt
                }
                ]
            };
            chatRequestOptions = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = openaiMaxTokens,
                Temperature = openaiTemperature,
                TopP = openaiTopP
            };
            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            // logger.LogError($"Error initializing Chat Agent: {errorMessage}", "AIHelper");
            Console.WriteLine($"Error initializing Chat Agent: {errorMessage}");
            return false;
        }
    }

    /// <summary>
    /// Initialize the Image Generator
    /// </summary>
    private bool InitializeImageGenerator()
    {
        if (imageGenerator != null) return true;

        if (string.IsNullOrEmpty(openaiImageEndpointUrl))
        {
            Console.WriteLine("No OpenAI API image keys available");
            return false;
        }
        AzureOpenAIClient _imageClientHost = null;
        try
        {
            if (string.IsNullOrEmpty(openaiImageApiKey))
            {
                _imageClientHost = new AzureOpenAIClient(openaiImageEndpoint, GetCredentials());
            }
            else
            {
                _imageClientHost = new AzureOpenAIClient(openaiImageEndpoint, new ApiKeyCredential(openaiImageApiKey));
            }
            imageGenerator = _imageClientHost.GetImageClient(openaiImageDeploymentName);
            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            // logger.LogError($"Error initializing Image Agent: {errorMessage}", "AIHelper");
            Console.WriteLine($"Error initializing Image Agent: {errorMessage}");
            return false;
        }
    }

    /// <summary>
    /// Get Credentials if needed
    /// </summary>
    private DefaultAzureCredential GetCredentials()
    {
        var credential = string.IsNullOrEmpty(vsTenantId) ?
            new DefaultAzureCredential() :
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true,
                TenantId = vsTenantId // if you get an error "Token tenant does not match resource tenant" during local development, force the tenant
            });
        return credential;
    }
    #endregion
}
