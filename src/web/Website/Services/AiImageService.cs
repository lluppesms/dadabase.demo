using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using DadABase.Web.Helpers;
using OpenAI;
using OpenAI.Images;
using System.ClientModel;
using System.Text.Json;

namespace DadABase.Web.Services;

/// <summary>
/// Image service that preserves the existing Azure OpenAI, MAI, OpenAI, and blob-storage image behavior.
/// </summary>
public class AiImageService : IAiImageService
{
    private readonly string openaiImageEndpointUrl = string.Empty;
    private readonly Uri openaiImageEndpoint = null;
    private readonly string openaiImageDeploymentName = "dall-e-3";
    private readonly string openaiImageApiKey = string.Empty;
    private readonly string openaiImageModelProvider = string.Empty;
    private ImageClient imageGenerator = null;
    private readonly string vsTenantId = string.Empty;
    private readonly string blobStorageAccountName = string.Empty;
    private readonly string blobContainerName = "joke-images";
    private readonly DefaultAzureCredential azureCredential;
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AiImageService"/> class.
    /// </summary>
    public AiImageService(IConfiguration config, DefaultAzureCredential credential)
    {
        openaiImageEndpointUrl = config["AppSettings:AzureOpenAI:Image:Endpoint"];
        openaiImageEndpoint = !string.IsNullOrEmpty(openaiImageEndpointUrl) ? new(config["AppSettings:AzureOpenAI:Image:Endpoint"]) : null;
        openaiImageDeploymentName = config["AppSettings:AzureOpenAI:Image:DeploymentName"];
        openaiImageApiKey = config["AppSettings:AzureOpenAI:Image:ApiKey"];
        openaiImageModelProvider = config["AppSettings:AzureOpenAI:Image:ModelProvider"] ?? string.Empty;
        blobStorageAccountName = config["AppSettings:BlobStorageAccountName"];
        vsTenantId = config["VisualStudioTenantId"];
        azureCredential = credential;
    }

    private bool IsMaiModel =>
        string.Equals(openaiImageModelProvider, "MAI", StringComparison.OrdinalIgnoreCase)
        || (string.IsNullOrEmpty(openaiImageModelProvider)
            && openaiImageDeploymentName.Contains("mai", StringComparison.OrdinalIgnoreCase));

    private bool IsOpenAIModel =>
        string.Equals(openaiImageModelProvider, "OpenAI", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public async Task<(string, bool, string)> GenerateAnImage(string imageDescription, int jokeId = 0)
    {
        var imageDataUrl = string.Empty;
        try
        {
            if (!InitializeImageGenerator())
            {
                return (string.Empty, false, "AI Image Keys not found!");
            }

            if (jokeId > 0)
            {
                var existingImageUrl = await GetJokeImageUrlAsync(jokeId);
                if (!string.IsNullOrEmpty(existingImageUrl))
                {
                    Console.WriteLine($"Image already exists for JokeId {jokeId}: {existingImageUrl}");
                    return (existingImageUrl, true, string.Empty);
                }
            }

            if (IsMaiModel)
            {
                return await GenerateMaiImageAsync(imageDescription, jokeId);
            }

            if (IsOpenAIModel)
            {
                return await GenerateOpenAIImageAsync(imageDescription, jokeId);
            }

            Console.WriteLine($"Generating Image for Joke {jokeId} using endpoint {openaiImageEndpointUrl} and model {openaiImageDeploymentName} with Prompt: {imageDescription[..Math.Min(15, imageDescription.Length)]}...");
            var imageQuality = openaiImageDeploymentName.StartsWith("dall-e", StringComparison.OrdinalIgnoreCase)
                ? GeneratedImageQuality.HighQuality
                : GeneratedImageQuality.MediumQuality;
            var imageResult = await imageGenerator.GenerateImageAsync(imageDescription, new()
            {
                Quality = imageQuality,
                Size = GeneratedImageSize.W1024xH1024
            });

            var image = imageResult.Value;

            if (image != null && image.ImageBytes != null)
            {
                var imageBytes = image.ImageBytes.ToArray();

                if (jokeId > 0)
                {
                    var imageBlobUrl = await SaveImageToBlobAsync(imageBytes, jokeId);
                    if (!string.IsNullOrEmpty(imageBlobUrl))
                    {
                        Console.WriteLine($"Saved image to blob storage ({imageBytes.Length} bytes)");
                        return (imageBlobUrl, true, string.Empty);
                    }
                }

                var base64Image = Convert.ToBase64String(imageBytes);
                imageDataUrl = $"data:image/png;base64,{base64Image}";
                Console.WriteLine($"Generated Image (base64 data URL, {imageBytes.Length} bytes)");
            }
            else
            {
                if (image != null && image.ImageUri != null)
                {
                    imageDataUrl = image.ImageUri.ToString();
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

            var sorryMessage = string.Empty;
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

    /// <inheritdoc/>
    public string GetJokeImagePath(int jokeId)
    {
        if (jokeId <= 0) return string.Empty;

        return Task.Run(async () => await GetJokeImageUrlAsync(jokeId)).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task<(string blobUrl, bool success, string message)> SaveBase64ImageToBlob(string base64ImageDataUrl, int jokeId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(base64ImageDataUrl))
            {
                return (string.Empty, false, "No image data provided");
            }

            var existingImageUrl = await GetJokeImageUrlAsync(jokeId);
            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                Console.WriteLine($"Image already exists for JokeId {jokeId}: {existingImageUrl}");
                return (existingImageUrl, true, string.Empty);
            }

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

    private BlobContainerClient GetBlobContainerClient()
    {
        if (string.IsNullOrEmpty(blobStorageAccountName))
        {
            Console.WriteLine("[DIAGNOSTIC] Blob storage account name is empty - cannot create BlobContainerClient");
            return null;
        }
        var blobOptions = new BlobClientOptions { Retry = { MaxRetries = 1 } };
        var blobUri = new Uri($"https://{blobStorageAccountName}.blob.core.windows.net");
        Console.WriteLine($"[DIAGNOSTIC] Creating BlobServiceClient with URI: {blobUri}");
        Console.WriteLine($"[DIAGNOSTIC] Using managed identity credential chain for blob authentication");
        var blobServiceClient = new BlobServiceClient(blobUri, azureCredential, blobOptions);
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
        Console.WriteLine($"[DIAGNOSTIC] Created BlobContainerClient for container: {blobContainerName}");
        return containerClient;
    }

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
                return GetJokeImageRoute(jokeId);
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

            await containerClient.CreateIfNotExistsAsync();

            var blobName = $"{jokeId}.png";
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(imageBytes);
            await blobClient.UploadAsync(stream, overwrite: true);

            Console.WriteLine($"Uploaded blob: {blobClient.Uri}");
            return GetJokeImageRoute(jokeId);
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error saving image to blob storage for JokeId {jokeId}: {msg}");
            return string.Empty;
        }
    }

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

    private bool InitializeImageGenerator()
    {
        if (IsMaiModel) return !string.IsNullOrEmpty(openaiImageEndpointUrl);

        if (IsOpenAIModel) return !string.IsNullOrEmpty(openaiImageApiKey);

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

    private async Task<(string imageDataUrl, bool success, string message)> GenerateOpenAIImageAsync(string imageDescription, int jokeId)
    {
        if (string.IsNullOrEmpty(openaiImageApiKey))
        {
            return (string.Empty, false, "OpenAI API key not configured!");
        }

        var baseUrl = !string.IsNullOrEmpty(openaiImageEndpointUrl)
            ? openaiImageEndpointUrl.TrimEnd('/')
            : "https://api.openai.com/v1";
        var url = $"{baseUrl}/images/generations";

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            model = openaiImageDeploymentName,
            prompt = imageDescription,
            n = 1,
            size = "1024x1024",
            response_format = "b64_json"
        });

        Console.WriteLine($"Generating image for Joke {jokeId} using OpenAI API {url} model={openaiImageDeploymentName} prompt={imageDescription[..Math.Min(15, imageDescription.Length)]}...");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"******");
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
            return ("Blank!", false, $"No image data returned from OpenAI image generator! Response: {responseJson[..Math.Min(200, responseJson.Length)]}");
        }

        var imageBytes = Convert.FromBase64String(base64Data);

        if (jokeId > 0)
        {
            var imageBlobUrl = await SaveImageToBlobAsync(imageBytes, jokeId);
            if (!string.IsNullOrEmpty(imageBlobUrl))
            {
                Console.WriteLine($"Saved OpenAI image to blob storage ({imageBytes.Length} bytes)");
                return (imageBlobUrl, true, string.Empty);
            }
        }

        var imageDataUrl = $"data:image/png;base64,{base64Data}";
        Console.WriteLine($"Generated OpenAI image (base64 data URL, {imageBytes.Length} bytes)");
        return (imageDataUrl, true, string.Empty);
    }

    private static string GetJokeImageRoute(int jokeId) => $"/api/images/jokes/{jokeId}.png";
}
