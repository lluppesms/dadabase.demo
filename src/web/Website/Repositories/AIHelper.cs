namespace DadABase.Web.Repositories;

/// <summary>
/// AI helper that keeps joke-specific prompt composition and response parsing while delegating model calls to AI services.
/// </summary>
public class AIHelper : IAIHelper
{
    private readonly IAiChatService aiChatService;
    private readonly IAiImageService aiImageService;

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
    /// Initializes a new instance of the <see cref="AIHelper"/> class.
    /// </summary>
    public AIHelper(IAiChatService aiChatService, IAiImageService aiImageService)
    {
        this.aiChatService = aiChatService;
        this.aiImageService = aiImageService;
    }

    /// <summary>
    /// Give it a joke and get back an image description.
    /// </summary>
    public async Task<(string description, bool success, string message)> GetJokeSceneDescription(string jokeText)
    {
        var imageDescription = string.Empty;

        try
        {
            imageDescription = await aiChatService.CompleteAsync(JokeImageGeneratorPrompt, jokeText);

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
            var responseText = await aiChatService.CompleteAsync(JokeCategoryClassifierPrompt, message);

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
            var responseText = await aiChatService.CompleteAsync(JokeAnalyzerPrompt, message);

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
