namespace DadJokeMCPSSE;

[McpServerToolType]
public sealed class DadJokeTools(DadJokeService DadJokeService)
{
    private readonly DadJokeService DadJokeService = DadJokeService;

    [McpServerTool, Description("Get a random Dad Joke")]
    public async Task<string> GetDadJoke()
    {
        var dadJoke = await DadJokeService.GetDadJoke();
        return dadJoke.ToString();
    }

    [McpServerTool, Description("Get a list of Dad Joke categories.")]
    public async Task<string> GetDadJokeCategories()
    {
        var categories = await DadJokeService.GetDadJokeCategories();
        return JsonSerializer.Serialize(categories, DadJokeContext.Default.ListString);
    }

    [McpServerTool, Description("Get a list of Dad Jokes by category.")]
    public async Task<string> GetDadJokesByCategory([Description("The name of the Category to get a list of jokes for")] string name)
    {
        var dadJokes = await DadJokeService.GetDadJokesByCategory(name);
        var sb = new StringBuilder();
        foreach (var joke in dadJokes)
        {
            sb.AppendLine(joke.ToString());
        }
        return sb.ToString();
    }
}
