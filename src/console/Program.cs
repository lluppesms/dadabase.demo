
// Example of a Console Program using https://spectreconsole.net/ to format output...!

Console.OutputEncoding = Encoding.UTF8;

var djs = new DadJokeService();
var continueJoking = true;
var maxJokesToShowForACategory = 5;

// Display Banner
AnsiConsole.Write(new FigletText("Dad Jokes").LeftJustified().Color(Color.Red));

// Display Welcome Message
AnsiConsole.MarkupLine("\n[red]Hello[/] I'm Dad and I like bad jokes! " + Emoji.Known.RollingOnTheFloorLaughing);

// Prompt User for Options
var jokeOption = AnsiConsole.Prompt(
     new SelectionPrompt<string>()
    .Title("\nWhat's your [green]pleasure[/]?")
    .AddChoices([JokeTellingOptions.RandomJoke, JokeTellingOptions.JokesInACategory])
    );

while (continueJoking)
{
    if (jokeOption.Contains("Category"))
    {
        // -- Jokes in a Category ---------------------------------------------------------------------------
        var categories = await djs.GetDadJokeCategories();
        var category = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What's your [green]favorite type of joke[/]?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more categories)[/]")
                .AddChoices(categories));

        var jokeList = await djs.GetDadJokesByCategory(category);
        var sb = new StringBuilder();
        var jokeCount = 0;
        foreach (var joke in jokeList)
        {
            sb.AppendLine(joke.ToString());
            jokeCount++;
            if (jokeCount >= maxJokesToShowForACategory) { break; }
        }
        AnsiConsole.MarkupLine($"\n[green]Here are some {category} Jokes![/] " + Emoji.Known.Scroll);
        AnsiConsole.MarkupInterpolated($"[yellow]{sb}[/]\n");
    }
    else
    {
        // -- Random Dad Joke ---------------------------------------------------------------------------
        var dadJoke = await djs.GetDadJoke();
        AnsiConsole.MarkupLine("\n[green]Here's a random Dad Joke:[/] " + Emoji.Known.ThinkingFace);
        AnsiConsole.MarkupInterpolated($"[yellow]{dadJoke}[/]\n");
    }

    // Prompt User for Continuing Options
    jokeOption = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("\nWhat's your [green]pleasure[/]?")
        .AddChoices([JokeTellingOptions.RandomJoke, JokeTellingOptions.JokesInACategory, JokeTellingOptions.StopTalking])
        );

    continueJoking = jokeOption != JokeTellingOptions.StopTalking;

    if (!continueJoking)
    {
        AnsiConsole.MarkupLine("\n[aqua]Okay, thanks for listening -- no more jokes for now![/] " + Emoji.Known.ZanyFace + "  " + Emoji.Known.Zzz);
    }
}
