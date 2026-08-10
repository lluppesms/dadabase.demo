using DadABase.Web.Helpers;

namespace DadABase.Web.Repositories;

/// <summary>
/// AI helper that keeps joke-specific prompt composition and response parsing while delegating model calls to AI services.
/// </summary>
public class AIHelper : IAIHelper
{
    private readonly IAiChatService aiChatService;
    private readonly IAiImageService aiImageService;
    private readonly string jokeCategoryClassifierPrompt;
    private readonly string jokeImageGeneratorPrompt;
    private readonly string jokeAnalyzerPrompt;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIHelper"/> class.
    /// </summary>
    public AIHelper(IAiChatService aiChatService, IAiImageService aiImageService)
    {
        this.aiChatService = aiChatService;
        this.aiImageService = aiImageService;

        jokeCategoryClassifierPrompt = ReadPromptFile("JokeCategoryClassifierPrompt.txt");
        jokeImageGeneratorPrompt = ReadPromptFile("JokeImageGeneratorPrompt.txt");
        jokeAnalyzerPrompt = ReadPromptFile("JokeAnalyzerPrompt.txt");
    }

    private static string ReadPromptFile(string fileName)
    {
        var promptPath = Path.Combine(AppContext.BaseDirectory, "Data", fileName);
        return File.ReadAllText(promptPath);
    }

    /// <summary>
    /// Give it a joke and get back an image description.
    /// </summary>
    public async Task<(string description, bool success, string message)> GetJokeSceneDescription(string jokeText)
    {
        var imageDescription = string.Empty;

        try
        {
            imageDescription = await aiChatService.CompleteAsync(jokeImageGeneratorPrompt, jokeText);

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
    /// Suggest relevant categories for a joke using AI.
    /// </summary>
    public async Task<(List<string> suggestedCategories, bool success, string message)> SuggestCategories(string jokeText, IEnumerable<string> availableCategories)
    {
        var suggestedCategories = new List<string>();
        try
        {
            var message = $"Joke: {jokeText}\n\nAvailable categories: {string.Join(", ", availableCategories)}\n\nWhich categories from the list above best fit this joke? Return only the matching category names as a comma-separated list.";
            var responseText = await aiChatService.CompleteAsync(jokeCategoryClassifierPrompt, message);

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
    /// Give this a description and get back a generated image as a base64 data URL or blob route.
    /// </summary>
    public async Task<(string, bool, string)> GenerateAnImage(string imageDescription, int jokeId = 0)
    {
        return await aiImageService.GenerateAnImage(imageDescription, jokeId);
    }

    /// <summary>
    /// Get the image URL for a joke if it exists in blob storage.
    /// </summary>
    public string GetJokeImagePath(int jokeId)
    {
        return aiImageService.GetJokeImagePath(jokeId);
    }

    /// <summary>
    /// Save an already-generated base64 image to blob storage.
    /// </summary>
    public async Task<(string blobUrl, bool success, string message)> SaveBase64ImageToBlob(string base64ImageDataUrl, int jokeId)
    {
        return await aiImageService.SaveBase64ImageToBlob(base64ImageDataUrl, jokeId);
    }

    /// <summary>
    /// Analyze joke to get both category suggestions and scene description in a single AI call.
    /// </summary>
    public async Task<(List<string> suggestedCategories, string sceneDescription, bool success, string message)> AnalyzeJoke(string jokeText, IEnumerable<string> availableCategories)
    {
        var suggestedCategories = new List<string>();
        var sceneDescription = string.Empty;

        try
        {
            var message = $"Joke: {jokeText}\n\nAvailable categories: {string.Join(", ", availableCategories)}\n\nAnalyze this joke and provide category suggestions and a scene description.";
            var responseText = await aiChatService.CompleteAsync(jokeAnalyzerPrompt, message);

            Console.WriteLine($"Joke analysis response: {responseText}");

            var lines = responseText.Split('\n');
            var categoriesLine = lines.FirstOrDefault(l => l.StartsWith("CATEGORIES:", StringComparison.OrdinalIgnoreCase));
            var sceneStartIndex = Array.FindIndex(lines, l => l.StartsWith("SCENE:", StringComparison.OrdinalIgnoreCase));

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
}
