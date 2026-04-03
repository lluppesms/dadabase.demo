using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;
using System.Text.Json;

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
    private AIAgent jokeCategoryAgent = null;
    private AIAgent jokeAnalyzerAgent = null;
    private ImageClient imageGenerator = null;

    private readonly string vsTenantId = string.Empty;
    private readonly string blobStorageAccountName = string.Empty;
    private readonly string blobContainerName = "joke-images";
    private readonly DefaultAzureCredential azureCredential;

    private static readonly HttpClient _httpClient = new();
    #endregion

    private bool IsMaiModel => openaiImageDeploymentName.Contains("mai", StringComparison.OrdinalIgnoreCase);

    private const string JokeCategoryClassifierPrompt =
        "You are a joke classification assistant. Given a joke, identify which categories from a provided list best describe it. " +
        "Select a MAXIMUM of TWO categories - choose only the most relevant and applicable ones. " +
        "Return ONLY the names of matching categories as a comma-separated list with no other text. " +
        "Only return categories that are actually in the provided list - do not invent new ones. " +
        "If no categories match well, return the single most appropriate one from the list. " +
        "Prioritize quality over quantity - fewer, more accurate categories are better than multiple loosely-related ones.";

    private const string JokeImageGeneratorPrompt =
        "You are going to be told a funny joke or a humorous line or an insightful quote. " +
        "It is your responsibility to describe that joke so that an artist can draw a picture of the mental image that this joke creates. " +
        "Give clear instructions on how the scene should look and what objects should be included in the scene." +
        "Instruct the artist to draw it in a humorous cartoon format." +
        "Make sure the description does not ask for anything violent, sexual, or political so that it does not violate safety rules. " +
        "Keep the scene description under 250 words or less.";

    private const string JokeAnalyzerPrompt =
        "You are a joke analysis assistant. Given a joke and a list of available categories, you will provide two things:\n" +
        "1. Suggest up to TWO most relevant categories from the provided list (choose the best matches only)\n" +
        "2. Create a scene description for an artist to draw a humorous cartoon representation of the joke\n\n" +
        "Format your response EXACTLY as follows:\n" +
        "CATEGORIES: category1, category2\n" +
        "SCENE: [your scene description here]\n\n" +
        "Guidelines for categories:\n" +
        "- Select MAXIMUM of TWO categories from the provided list\n" +
        "- Only use categories that are in the provided list\n" +
        "- Choose the most relevant and applicable ones\n" +
        "- Prioritize quality over quantity\n\n" +
        "Guidelines for scene description:\n" +
        "- Describe what an artist should draw to represent this joke\n" +
        "- Give clear instructions on the scene, objects, and setting\n" +
        "- Request a humorous cartoon format\n" +
        "- Avoid anything violent, sexual, or political\n" +
        "- Keep description under 250 words";


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
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error during description generation: {msg}");
            return (imageDescription, false, "Could not generate an image description - see log for details!");
        }
    }

    /// <summary>
    /// Suggest relevant categories for a joke using AI
    /// </summary>
    /// <param name="jokeText">The joke text</param>
    /// <param name="availableCategories">All available category names</param>
    /// <returns>Tuple with list of suggested category names, success flag, and message</returns>
    public async Task<(List<string> suggestedCategories, bool success, string message)> SuggestCategories(string jokeText, IEnumerable<string> availableCategories)
    {
        var suggestedCategories = new List<string>();
        try
        {
            if (!InitializeCategoryAgent())
            {
                return (suggestedCategories, false, "AI Chat Keys not found!");
            }

            var message = $"Joke: {jokeText}\n\nAvailable categories: {string.Join(", ", availableCategories)}\n\nWhich categories from the list above best fit this joke? Return only the matching category names as a comma-separated list.";
            var response = await jokeCategoryAgent.RunAsync(message);
            var responseText = response.ToString();

            var categoryList = availableCategories.ToList();
            var suggestions = responseText.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Where(s => categoryList.Any(c => c.Equals(s, StringComparison.OrdinalIgnoreCase)))
                .Select(s => categoryList.First(c => c.Equals(s, StringComparison.OrdinalIgnoreCase)))
                .Distinct()
                .ToList();

            suggestedCategories = suggestions;
            Console.WriteLine($"Category suggestions for joke: {responseText} -> matched: {string.Join(", ", suggestedCategories)}");
            return (suggestedCategories, true, string.Empty);
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error during category suggestion: {msg}");
            return (suggestedCategories, false, "Could not suggest categories - see log for details!");
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

            // Check if image already exists in blob storage
            if (jokeId > 0)
            {
                var existingImageUrl = await GetJokeImageUrlAsync(jokeId);
                if (!string.IsNullOrEmpty(existingImageUrl))
                {
                    Console.WriteLine($"Image already exists for JokeId {jokeId}: {existingImageUrl}");
                    return (existingImageUrl, true, string.Empty);
                }
            }

            // Route to MAI image generator if the deployment model is MAI-based
            if (IsMaiModel)
            {
                return await GenerateMaiImageAsync(imageDescription, jokeId);
            }

            // gpt-image-1 parameters:
            // - Size: 1024x1024, 1024x1536, or 1536x1024
            // - Quality: Low, Medium (default), or High
            // - Note: gpt-image-1 models only return base64, no URI option
            Console.WriteLine($"Generating Image for Joke {jokeId} using endpoint {openaiImageEndpointUrl} and model {openaiImageDeploymentName} with Prompt: {imageDescription[..Math.Min(15, imageDescription.Length)]}...");
            var imageQuality = openaiImageDeploymentName == "dall-e-3" ? GeneratedImageQuality.HighQuality : GeneratedImageQuality.MediumQuality;
            var imageResult = await imageGenerator.GenerateImageAsync(imageDescription, new()
            {
                Quality = imageQuality,
                Size = GeneratedImageSize.W1024xH1024
            });

            var image = imageResult.Value;

            // gpt-image-1 models return base64 encoded image bytes, Dall-e-3 return a URL to a private storage account
            if (image != null && image.ImageBytes != null)
            {
                var imageBytes = image.ImageBytes.ToArray();

                // Save to Azure Blob Storage if jokeId is provided
                if (jokeId > 0)
                {
                    var imageBlobUrl = await SaveImageToBlobAsync(imageBytes, jokeId);
                    if (!string.IsNullOrEmpty(imageBlobUrl))
                    {
                        Console.WriteLine($"Saved image to blob storage ({imageBytes.Length} bytes)");
                        return (imageBlobUrl, true, string.Empty);
                    }
                }

                // Fallback to base64 if saving failed or no jokeId
                var base64Image = Convert.ToBase64String(imageBytes);
                imageDataUrl = $"data:image/png;base64,{base64Image}";
                Console.WriteLine($"Generated Image (base64 data URL, {imageBytes.Length} bytes)");
            }
            else
            {
                if (image!= null && image.ImageUri != null)
                {
                    imageDataUrl = image.ImageUri.ToString();
                    // TODO: get this image data from the URL and then save it to our blob and return it...?
                    Console.WriteLine($"Generated Image URI (not bytes!): {imageDataUrl}");
                }
                else
                {
                    return ("Blank!", false, "No image data was returned from the image generator!");
                }
            }
            return (imageDataUrl, true, string.Empty);
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            var errorMessage = $"Error during image generation: {msg} Endpoint: {openaiImageEndpointUrl} Model: {openaiImageDeploymentName} Prompt: {imageDescription[..Math.Min(100, imageDescription.Length)]}...";
            Console.WriteLine(errorMessage);

            var sorryMessage = string.Empty; // "Sorry - I can't even imagine drawing that picture...!  Try again with a different joke!";
            if (msg.Contains("safety system", StringComparison.CurrentCultureIgnoreCase) || msg.Contains("content filter", StringComparison.CurrentCultureIgnoreCase))
            {
                sorryMessage = "Sorry - I can't even imagine drawing that picture...!  Try again with a different joke!";
                if (msg.Contains("safety system", StringComparison.CurrentCultureIgnoreCase))
                {
                    sorryMessage += " (safety violation)";
                }
                if (msg.Contains("content filter", StringComparison.CurrentCultureIgnoreCase))
                {
                    sorryMessage += " (content filter violation)";
                }
            }
            else
            {
              sorryMessage = "Sorry - I'm having serious trouble imagining anything right now...!";
            }
            return (imageDescription, false, sorryMessage);
        }
    }

    /// <summary>
    /// Get the image URL for a joke if it exists in blob storage
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
    /// <returns>Image URL if exists, empty string otherwise</returns>
    public string GetJokeImagePath(int jokeId)
    {
        if (jokeId <= 0) return string.Empty;

        // Use Task.Run to avoid blocking the synchronization context
        return Task.Run(async () => await GetJokeImageUrlAsync(jokeId)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Get a BlobContainerClient for the joke images container
    /// </summary>
    /// <returns>BlobContainerClient or null if not configured</returns>
    private BlobContainerClient GetBlobContainerClient()
    {
        if (string.IsNullOrEmpty(blobStorageAccountName))
        {
            return null;
        }
        var blobOptions = new BlobClientOptions {Retry={MaxRetries=1}};
        var blobServiceClient = new BlobServiceClient(new Uri($"https://{blobStorageAccountName}.blob.core.windows.net"), azureCredential, blobOptions);
        return blobServiceClient.GetBlobContainerClient(blobContainerName);
    }

    /// <summary>
    /// Get the image URL for a joke if it exists in blob storage (async)
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
    /// <returns>Image URL if exists, empty string otherwise</returns>
    private async Task<string> GetJokeImageUrlAsync(int jokeId)
    {
        var blobName = string.Empty;
        if (jokeId <= 0)
        {
            return string.Empty;
        }

        try
        {
            var containerClient = GetBlobContainerClient();
            if (containerClient == null)
            {
                return string.Empty;
            }

            blobName = $"{jokeId}.png";
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                Console.WriteLine($"    Found existing image {blobName} in Storage Account: {blobStorageAccountName} Container: {blobContainerName}");
                return blobClient.Uri.ToString();
            }
            Console.WriteLine($"    Did NOT find Image {blobName} in Storage Account: {blobStorageAccountName} Container: {blobContainerName}");
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error checking blob existence for JokeId {jokeId}: {msg}");
            Console.WriteLine($"    Searching for Image {blobName} in Storage Account: {blobStorageAccountName} Container: {blobContainerName}");
        }

        return string.Empty;
    }

    /// <summary>
    /// Save an already-generated base64 image to blob storage
    /// </summary>
    /// <param name="base64ImageDataUrl">Base64 data URL (e.g., data:image/png;base64,...)</param>
    /// <param name="jokeId">Joke ID for saving the image</param>
    /// <returns>Tuple with blob URL, success flag, and message</returns>
    public async Task<(string blobUrl, bool success, string message)> SaveBase64ImageToBlob(string base64ImageDataUrl, int jokeId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(base64ImageDataUrl))
            {
                return (string.Empty, false, "No image data provided");
            }

            // Check if image already exists in blob storage
            var existingImageUrl = await GetJokeImageUrlAsync(jokeId);
            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                Console.WriteLine($"Image already exists for JokeId {jokeId}: {existingImageUrl}");
                return (existingImageUrl, true, string.Empty);
            }

            // Extract base64 data from data URL (e.g., "data:image/png;base64,iVBORw0...")
            var base64Data = base64ImageDataUrl;
            if (base64ImageDataUrl.Contains(","))
            {
                base64Data = base64ImageDataUrl.Split(',')[1];
            }

            var imageBytes = Convert.FromBase64String(base64Data);
            var blobUrl = await SaveImageToBlobAsync(imageBytes, jokeId);

            if (!string.IsNullOrEmpty(blobUrl))
            {
                Console.WriteLine($"Saved existing image to blob storage for JokeId {jokeId} ({imageBytes.Length} bytes)");
                return (blobUrl, true, string.Empty);
            }
            else
            {
                return (string.Empty, false, "Failed to save image to blob storage");
            }
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error saving base64 image to blob for JokeId {jokeId}: {msg}");
            return (string.Empty, false, $"Error saving image: {msg}");
        }
    }

    /// <summary>
    /// Save image bytes to Azure Blob Storage
    /// </summary>
    /// <param name="imageBytes">Image bytes</param>
    /// <param name="jokeId">Joke ID</param>
    /// <returns>Blob URL of the saved image or empty string if failed</returns>
    private async Task<string> SaveImageToBlobAsync(byte[] imageBytes, int jokeId)
    {
        try
        {
            var containerClient = GetBlobContainerClient();
            if (containerClient == null)
            {
                Console.WriteLine("Blob storage account name not configured");
                return string.Empty;
            }

            // Create container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var blobName = $"{jokeId}.png";
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(imageBytes);
            await blobClient.UploadAsync(stream, overwrite: true);

            Console.WriteLine($"Uploaded blob: {blobClient.Uri}");
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error saving image to blob storage for JokeId {jokeId}: {msg}");
            return string.Empty;
        }
    }

    #region Helper Methods
    /// <summary>
    /// Initialization
    /// </summary>
    public AIHelper(IConfiguration config, DefaultAzureCredential credential)
    {
        openaiEndpointUrl = config["AppSettings:AzureOpenAI:Chat:Endpoint"];
        openaiEndpoint = !string.IsNullOrEmpty(openaiEndpointUrl) ? new(config["AppSettings:AzureOpenAI:Chat:Endpoint"]) : null;
        openaiDeploymentName = config["AppSettings:AzureOpenAI:Chat:DeploymentName"];
        openaiApiKey = config["AppSettings:AzureOpenAI:Chat:ApiKey"];

        openaiImageEndpointUrl = config["AppSettings:AzureOpenAI:Image:Endpoint"];
        openaiImageEndpoint = !string.IsNullOrEmpty(openaiImageEndpointUrl) ? new(config["AppSettings:AzureOpenAI:Image:Endpoint"]) : null;
        openaiImageDeploymentName = config["AppSettings:AzureOpenAI:Image:DeploymentName"];
        openaiImageApiKey = config["AppSettings:AzureOpenAI:Image:ApiKey"];

        blobStorageAccountName = config["AppSettings:BlobStorageAccountName"];
        vsTenantId = config["VisualStudioTenantId"];
        azureCredential = credential;
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
            jokeDescriptionAgent = chatClient.AsAIAgent(
                name: "JokeImageDescriber",
                instructions: JokeImageGeneratorPrompt
            );

            return true;
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error initializing Joke Agent: {msg}");
            return false;
        }
    }

    /// <summary>
    /// Initialize the Joke Category Classifier Agent using Microsoft Agent Framework
    /// </summary>
    private bool InitializeCategoryAgent()
    {
        if (jokeCategoryAgent != null) return true;

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

            var chatClient = azureClient.GetChatClient(openaiDeploymentName);

            jokeCategoryAgent = chatClient.AsAIAgent(
                name: "JokeCategoryClassifier",
                instructions: JokeCategoryClassifierPrompt
            );

            return true;
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error initializing Category Agent: {msg}");
            return false;
        }
    }

    /// <summary>
    /// Generate an image using the MAI REST API (e.g. MAI-Image-2)
    /// Endpoint: {endpoint}/mai/v1/images/generations
    /// Response: { "data": [ { "b64_json": "..." } ] }
    /// </summary>
    private async Task<(string imageDataUrl, bool success, string message)> GenerateMaiImageAsync(string imageDescription, int jokeId)
    {
        if (string.IsNullOrEmpty(openaiImageApiKey))
        {
            return (string.Empty, false, "MAI Image API key not configured!");
        }

        var url = $"{openaiImageEndpointUrl.TrimEnd('/')}/mai/v1/images/generations";
        var payload = JsonConvert.SerializeObject(new
        {
            model = openaiImageDeploymentName,
            prompt = imageDescription,
            width = 1024,
            height = 1024
        });

        Console.WriteLine($"Generating MAI image for Joke {jokeId} using {url} model={openaiImageDeploymentName} prompt={imageDescription[..Math.Min(15, imageDescription.Length)]}...");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("api-key", openaiImageApiKey);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        string base64Data = null;
        using var responseDoc = JsonDocument.Parse(responseJson);
        if (responseDoc.RootElement.TryGetProperty("data", out var dataArray))
        {
            foreach (var item in dataArray.EnumerateArray())
            {
                if (item.TryGetProperty("b64_json", out var b64Element))
                {
                    base64Data = b64Element.GetString();
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(base64Data))
        {
            return ("Blank!", false, $"No image data returned from MAI image generator! Response: {responseJson[..Math.Min(200, responseJson.Length)]}");
        }

        var imageBytes = Convert.FromBase64String(base64Data);

        if (jokeId > 0)
        {
            var imageBlobUrl = await SaveImageToBlobAsync(imageBytes, jokeId);
            if (!string.IsNullOrEmpty(imageBlobUrl))
            {
                Console.WriteLine($"Saved MAI image to blob storage ({imageBytes.Length} bytes)");
                return (imageBlobUrl, true, string.Empty);
            }
        }

        var imageDataUrl = $"data:image/png;base64,{base64Data}";
        Console.WriteLine($"Generated MAI Image (base64 data URL, {imageBytes.Length} bytes)");
        return (imageDataUrl, true, string.Empty);
    }

    /// <summary>
    /// Initialize the Image Generator
    /// </summary>
    private bool InitializeImageGenerator()
    {
        // MAI models use HttpClient directly — no SDK ImageClient needed
        if (IsMaiModel) return !string.IsNullOrEmpty(openaiImageEndpointUrl);

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
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error initializing Image Agent: {msg}");
            return false;
        }
    }

    /// <summary>
    /// Initialize the Joke Analyzer Agent using Microsoft Agent Framework
    /// </summary>
    private bool InitializeAnalyzerAgent()
    {
        if (jokeAnalyzerAgent != null) return true;

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

            var chatClient = azureClient.GetChatClient(openaiDeploymentName);

            jokeAnalyzerAgent = chatClient.AsAIAgent(
                name: "JokeAnalyzer",
                instructions: JokeAnalyzerPrompt
            );

            return true;
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error initializing Analyzer Agent: {msg}");
            return false;
        }
    }

    /// <summary>
    /// Analyze joke to get both category suggestions and scene description in a single AI call
    /// </summary>
    /// <param name="jokeText">The joke text</param>
    /// <param name="availableCategories">All available category names</param>
    /// <returns>Tuple with category list, scene description, success flag, and message</returns>
    public async Task<(List<string> suggestedCategories, string sceneDescription, bool success, string message)> AnalyzeJoke(string jokeText, IEnumerable<string> availableCategories)
    {
        var suggestedCategories = new List<string>();
        var sceneDescription = string.Empty;

        try
        {
            if (!InitializeAnalyzerAgent())
            {
                return (suggestedCategories, sceneDescription, false, "AI Chat Keys not found!");
            }

            var message = $"Joke: {jokeText}\n\nAvailable categories: {string.Join(", ", availableCategories)}\n\nAnalyze this joke and provide category suggestions and a scene description.";
            var response = await jokeAnalyzerAgent.RunAsync(message);
            var responseText = response.ToString();

            Console.WriteLine($"Joke analysis response: {responseText}");

            // Parse the response to extract categories and scene description
            var lines = responseText.Split('\n');
            var categoriesLine = lines.FirstOrDefault(l => l.StartsWith("CATEGORIES:", StringComparison.OrdinalIgnoreCase));
            var sceneStartIndex = Array.FindIndex(lines, l => l.StartsWith("SCENE:", StringComparison.OrdinalIgnoreCase));

            // Extract categories
            if (categoriesLine != null)
            {
                var categoriesText = categoriesLine.Substring("CATEGORIES:".Length).Trim();
                var categoryList = availableCategories.ToList();
                suggestedCategories = categoriesText.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Where(s => categoryList.Any(c => c.Equals(s, StringComparison.OrdinalIgnoreCase)))
                    .Select(s => categoryList.First(c => c.Equals(s, StringComparison.OrdinalIgnoreCase)))
                    .Distinct()
                    .Take(2)
                    .ToList();
            }

            // Extract scene description
            if (sceneStartIndex >= 0)
            {
                var sceneText = string.Join("\n", lines.Skip(sceneStartIndex));
                sceneDescription = sceneText.Substring("SCENE:".Length).Trim();
            }

            Console.WriteLine($"Parsed categories: {string.Join(", ", suggestedCategories)}");
            Console.WriteLine($"Parsed scene description: {sceneDescription[..Math.Min(50, sceneDescription.Length)]}...");

            return (suggestedCategories, sceneDescription, true, string.Empty);
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error during joke analysis: {msg}");
            return (suggestedCategories, sceneDescription, false, "Could not analyze joke - see log for details!");
        }
    }
    #endregion
}
