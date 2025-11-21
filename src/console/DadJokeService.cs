namespace DadaBase;

public class DadJokeService
{
    private JokeList JokeData = new();
    private List<string> JokeCategories = [];

    public DadJokeService()
    {
        // load up the jokes into memory
        var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        if (resourceNames.Length > 0)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceNames[0]))
                if (stream != null)
                {
                    using var r = new StreamReader(stream);
                    var json = r.ReadToEnd();
                    JokeData = JsonSerializer.Deserialize<JokeList>(json) ?? new JokeList();
                }

            // select distinct categories from JokeData
            JokeCategories = JokeData.Jokes.Select(joke => joke.JokeCategoryTxt).Distinct().Order().ToList();
        }
    }

    public async Task<DadJoke> GetDadJoke()
    {
        _ = await Task.FromResult(true);
        try
        {
            var joke = JokeData.Jokes[Random.Shared.Next(0, JokeData.Jokes.Count)];
            return joke ?? new DadJoke("No jokes here!");
        }
        catch (Exception ex)
        {
            return new DadJoke($"Why did the dad joke not work? {ex.Message}");
        }
    }

    public async Task<List<DadJoke>> GetDadJokesByCategory(string categoryName)
    {
        _ = await Task.FromResult(true);
        try
        {
            var jokesInCategory = JokeData.Jokes
                .Where(joke => joke.JokeCategoryTxt == categoryName)
                .ToList();
            return jokesInCategory;
        }
        catch (Exception ex)
        {
            return [new DadJoke($"Why did the dad joke not work? {ex.Message}")];
        }
    }

    public async Task<List<string>> GetDadJokeCategories()
    {
        _ = await Task.FromResult(true);
        try
        {
            return JokeCategories;
        }
        catch (Exception ex)
        {
            return [$"Why did the dad joke not return a category? {ex.Message}"];
        }
    }
}
