namespace DadJokeMCP;

[JsonSerializable(typeof(List<DadJoke>))]
internal sealed partial class DadJokeContext : JsonSerializerContext { }

public class JokeList
{
    public List<DadJoke> Jokes { get; set; }

    public JokeList()
    {
        Jokes = [];
    }
}

public class DadJoke
{
    public string JokeCategoryTxt { get; set; }
    public string JokeTxt { get; set; }
    public string Attribution { get; set; }
    public string ImageTxt { get; set; }
    public DadJoke()
    {
        JokeTxt = string.Empty;
        JokeCategoryTxt = string.Empty;
        Attribution = string.Empty;
        ImageTxt = string.Empty;
    }
    public DadJoke(string jokeTxt)
    {
        JokeTxt = jokeTxt;
        JokeCategoryTxt = string.Empty;
        Attribution = string.Empty;
        ImageTxt = string.Empty;
    }
    public override string ToString()
    {
        var myJokeText = JokeTxt.Length > 0 ? JokeTxt : "No joke here!";
        if (myJokeText.StartsWith("KK/WT:"))
        {
            var myFirstQuestionMark = myJokeText.IndexOf("?");
            var myQuestion = myJokeText.Substring(6, myFirstQuestionMark - 6).Trim();
            var myResponse = myJokeText.Substring(myFirstQuestionMark + 1, myJokeText.Length - myFirstQuestionMark - 1).Trim();
            var myFullText =
              $"Knock Knock!<br/>" +
              $"&nbsp;&nbsp;Who's There?<br />" +
              $"{myQuestion}<br/>" +
              $"&nbsp;&nbsp;{myQuestion} who?<br/>" +
              $"{myResponse}";
            myJokeText = myFullText;
        }

        if (Attribution.Length > 0)
        {
            myJokeText += $" - {Attribution}";
        }

        return myJokeText;
    }
}
