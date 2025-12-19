namespace DadJokeMCP;

[McpServerToolType]
public sealed class DadJokeTools
{
    private readonly DadJokeService DadJokeService;

    public DadJokeTools(DadJokeService DadJokeService)
    {
        this.DadJokeService = DadJokeService;
    }

    [McpServerTool, Description("Get a random Dad Joke")]
    public async Task<string> GetDadJoke()
    {
        var dadJoke = await DadJokeService.GetDadJoke();
        return dadJoke.ToString();
    }

    [McpServerTool, Description("Get a list of Dad Joke by category.")]
    public async Task<string> GetDadJokesByCategory([Description("The name of the Dad Joke Category to get a list of jobs for")] string name)
    {
        var dadJokes = await DadJokeService.GetDadJokesByCategory(name);
        var sb = new StringBuilder();
        foreach (var joke in dadJokes)
        {
            sb.AppendLine(joke.ToString());
        }
        return sb.ToString();
    }

    [McpServerTool, Description("Get a list of Dad Joke categories.")]
    public async Task<string> GetDadJokeCategories()
    {
        var categories = await DadJokeService.GetDadJokeCategories();
        return JsonSerializer.Serialize(categories);
    }
}
