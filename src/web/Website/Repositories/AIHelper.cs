using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;

namespace DadABase.Web.Repositories;

/// <summary>
/// AI Agent Helper using Microsoft Agent Framework to manage conversations with Azure OpenAI Service
/// </summary>
public class AIHelper : IAIHelper
{
    #region Variables
    private readonly string openaiEndpointUrl = string.Empty;
    private readonly Uri openaiEndpoint = null;
    private readonly string openaiDeploymentName = "gpt-4o";
    private readonly string openaiApiKey = string.Empty;

    private readonly string openaiImageEndpointUrl = string.Empty;
    private readonly Uri openaiImageEndpoint = null;
    private readonly string openaiImageDeploymentName = "dall-e-3";
    private readonly string openaiImageApiKey = string.Empty;

    private AIAgent jokeDescriptionAgent = null;
    private ImageClient imageGenerator = null;

    private readonly string vsTenantId = string.Empty;
    private readonly IWebHostEnvironment environment;
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
            if (!InitializeJokeAgent())
            {
                return (string.Empty, false, "AI Chat Keys not found!");
            }

            // Use the Agent Framework to run the agent with the joke text
            var response = await jokeDescriptionAgent.RunAsync(jokeText);
            imageDescription = response.ToString();

            Console.WriteLine($"Joke: {jokeText} \nImage description {imageDescription}");
            return (imageDescription, true, string.Empty);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error during description generation: {ex.Message}";
            Console.WriteLine(errorMessage);
            return (imageDescription, false, "Could not generate an image description - see log for details!");
        }
    }

    /// <summary>
    /// Give this a description and get back a generated image as a base64 data URL
    /// </summary>
    /// <param name="imageDescription">Image description</param>
    /// <param name="jokeId">Joke ID for saving the image</param>
    /// <returns></returns>
    public async Task<(string, bool, string)> GenerateAnImage(string imageDescription, int jokeId = 0)
    {
        var imageDataUrl = string.Empty;
        try
        {
            if (!InitializeImageGenerator())
            {
                return (string.Empty, false, "AI Image Keys not found!");
            }

            // Check if image already exists
            if (jokeId > 0)
            {
                var existingImagePath = GetJokeImagePath(jokeId);
                if (!string.IsNullOrEmpty(existingImagePath))
                {
                    Console.WriteLine($"Image already exists for JokeId {jokeId}: {existingImagePath}");
                    return (existingImagePath, true, string.Empty);
                }
            }

            // gpt-image-1 parameters:
            // - Size: 1024x1024, 1024x1536, or 1536x1024
            // - Quality: Low, Medium (default), or High
            // - Note: gpt-image-1 models only return base64, no URI option
            var imageResult = await imageGenerator.GenerateImageAsync(imageDescription, new()
            {
                Quality = GeneratedImageQuality.Medium,
                Size = GeneratedImageSize.W1024xH1024
            });

            var image = imageResult.Value;

            // gpt-image-1 models return base64 encoded image bytes
            var imageBytes = image.ImageBytes.ToArray();

            // Save to file if jokeId is provided
            if (jokeId > 0)
            {
                var imagePath = SaveImageToFile(imageBytes, jokeId);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    Console.WriteLine($"Saved image to {imagePath} ({imageBytes.Length} bytes)");
                    return (imagePath, true, string.Empty);
                }
            }

            // Fallback to base64 if saving failed or no jokeId
            var base64Image = Convert.ToBase64String(imageBytes);
            imageDataUrl = $"data:image/png;base64,{base64Image}";

            Console.WriteLine($"Generated Image (base64 data URL, {imageBytes.Length} bytes)");
            return (imageDataUrl, true, string.Empty);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error during image generation: {ex.Message} Prompt: {imageDescription}";
            Console.WriteLine(errorMessage);

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

    /// <summary>
    /// Get the image file path for a joke if it exists
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
    /// <returns>Image file path if exists, empty string otherwise</returns>
    public string GetJokeImagePath(int jokeId)
    {
        if (jokeId <= 0) return string.Empty;

        var fileName = $"{jokeId}.png";
        var webRootPath = environment.WebRootPath;
        var imageFolderPath = Path.Combine(webRootPath, "images", "jokes");
        var fullPath = Path.Combine(imageFolderPath, fileName);

        if (File.Exists(fullPath))
        {
            return $"/images/jokes/{fileName}";
        }

        return string.Empty;
    }

    #region Helper Methods
    /// <summary>
    /// Initialization
    /// </summary>
    public AIHelper(IConfiguration config, IWebHostEnvironment env)
    {
        openaiEndpointUrl = config["AppSettings:AzureOpenAI:Chat:Endpoint"];
        openaiEndpoint = !string.IsNullOrEmpty(openaiEndpointUrl) ? new(config["AppSettings:AzureOpenAI:Chat:Endpoint"]) : null;
        openaiDeploymentName = config["AppSettings:AzureOpenAI:Chat:DeploymentName"];
        openaiApiKey = config["AppSettings:AzureOpenAI:Chat:ApiKey"];

        openaiImageEndpointUrl = config["AppSettings:AzureOpenAI:Image:Endpoint"];
        openaiImageEndpoint = !string.IsNullOrEmpty(openaiImageEndpointUrl) ? new(config["AppSettings:AzureOpenAI:Image:Endpoint"]) : null;
        openaiImageDeploymentName = config["AppSettings:AzureOpenAI:Image:DeploymentName"];
        openaiImageApiKey = config["AppSettings:AzureOpenAI:Image:ApiKey"];

        vsTenantId = config["VisualStudioTenantId"];
        environment = env;
    }

    /// <summary>
    /// Initialize the Joke Description Agent using Microsoft Agent Framework
    /// </summary>
    private bool InitializeJokeAgent()
    {
        if (jokeDescriptionAgent != null) return true;

        if (string.IsNullOrEmpty(openaiEndpointUrl) || string.IsNullOrEmpty(openaiDeploymentName))
        {
            Console.WriteLine("No OpenAI API keys available");
            return false;
        }

        try
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

            // Get the chat client and create an AI Agent using the Agent Framework
            var chatClient = azureClient.GetChatClient(openaiDeploymentName);

            // Create the AI Agent using the Agent Framework extension method
            jokeDescriptionAgent = chatClient.CreateAIAgent(
                name: "JokeImageDescriber",
                instructions: JokeImageGeneratorPrompt
            );

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Console.WriteLine($"Error initializing Joke Agent: {errorMessage}");
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

        try
        {
            AzureOpenAIClient imageClientHost;

            if (string.IsNullOrEmpty(openaiImageApiKey))
            {
                imageClientHost = new AzureOpenAIClient(openaiImageEndpoint, Utilities.GetCredentials(vsTenantId));
            }
            else
            {
                imageClientHost = new AzureOpenAIClient(openaiImageEndpoint, new ApiKeyCredential(openaiImageApiKey));
            }

            imageGenerator = imageClientHost.GetImageClient(openaiImageDeploymentName);
            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Console.WriteLine($"Error initializing Image Agent: {errorMessage}");
            return false;
        }
    }

    /// <summary>
    /// Save image bytes to a file
    /// </summary>
    /// <param name="imageBytes">Image bytes</param>
    /// <param name="jokeId">Joke ID</param>
    /// <returns>Web path to the saved image or empty string if failed</returns>
    private string SaveImageToFile(byte[] imageBytes, int jokeId)
    {
        try
        {
            var fileName = $"{jokeId}.png";
            var webRootPath = environment.WebRootPath;
            var imageFolderPath = Path.Combine(webRootPath, "images", "jokes");

            // Ensure directory exists
            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }

            var fullPath = Path.Combine(imageFolderPath, fileName);
            File.WriteAllBytes(fullPath, imageBytes);

            return $"/images/jokes/{fileName}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving image for JokeId {jokeId}: {ex.Message}");
            return string.Empty;
        }
    }
    #endregion
}
