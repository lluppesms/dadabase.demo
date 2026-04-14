namespace DadJokeMCP.Shared;

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
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceNames[0]))
                {
                    if (stream != null)
                    {
                        using var r = new StreamReader(stream);
                        var json = r.ReadToEnd();
                        JokeData = JsonSerializer.Deserialize<JokeList>(json, DadJokeContext.Default.JokeList) ?? new JokeList();
                    }
                }
            }
            catch (Exception ex)
            {
                JokeData = new JokeList();
                JokeData.Jokes.Add(new DadJoke()
                {
                    JokeCategoryTxt = "Dad",
                    JokeTxt = "My son just became a father for the first time today... to pass the paternal torch, he asked me where I kept all my dad jokes, so I told him - they are stored in my dadabase."
                });
                JokeCategories.Add("Dad");
                Console.WriteLine($"Error loading jokes: {ex.Message}");
            }
            // select distinct categories from Joke Data File
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

    public Task<List<string>> GetDadJokeCategories()
    {
        try
        {
            return Task.FromResult(JokeCategories);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new List<string> { $"Why did the dad joke not return a category? {ex.Message}" });
        }
    }
}
