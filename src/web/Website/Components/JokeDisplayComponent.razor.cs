//-----------------------------------------------------------------------
// <copyright file="JokeDisplayComponent.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke display component code-behind
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Components;

/// <summary>
/// Renders a joke and prepares formatted display text.
/// </summary>
public partial class JokeDisplayComponent : ComponentBase
{
    /// <summary>
    /// Gets or sets the joke to display.
    /// </summary>
    [Parameter]
    public Joke myJoke { get; set; }

    [Inject] IJokeRepository JokeRepository { get; set; }
    [Inject] IHttpContextAccessor HttpContextAccessor { get; set; }
    [Inject] SweetAlertService SweetAlert { get; set; }
    [Inject] ISnackbar Snackbar { get; set; }
    [Inject] DadABase.Web.Services.RatingUserKeyResolver RatingKeyResolver { get; set; }

    private string myJokeText = string.Empty;
    private string myFullText = string.Empty;
    private int displayRatingValue = 0;
    private bool ratingSubmitting = false;

    /// <summary>
    /// Recomputes the display text when parameter values change.
    /// </summary>
    protected override void OnParametersSet()
    {
        ParseJokeText(myJoke);
    }

    /// <summary>
    /// Converts raw joke content into HTML-formatted text for rendering.
    /// </summary>
    /// <param name="joke">The source joke.</param>
    protected void ParseJokeText(Joke joke)
    {
        if (string.IsNullOrEmpty(joke.JokeTxt) || myJokeText == joke.JokeTxt) return;

        displayRatingValue = joke.Rating != null ? Convert.ToInt32(Math.Round((decimal)joke.Rating)) : 0;

        myJokeText = System.Web.HttpUtility.HtmlEncode(joke.JokeTxt);
        myJokeText = myJokeText.Replace("\n", "<br/>");
        if (myJokeText.StartsWith("KK/WT:"))
        {
            var myFirstQuestionMark = myJokeText.IndexOf("?");
            var myQuestion = myJokeText.Substring(6, myFirstQuestionMark - 6).Trim();
            var myResponse = myJokeText.Substring(myFirstQuestionMark + 1, myJokeText.Length - myFirstQuestionMark - 1).Trim();
            myFullText =
              $"Knock Knock!<br/>" +
              $"&nbsp;&nbsp;Who's There?<br />" +
              $"{myQuestion}<br/>" +
              $"&nbsp;&nbsp;{myQuestion} who?<br/>" +
              $"{myResponse}";
        }
        else
        {
            // Only insert a line break after "?" when followed by whitespace or end-of-string,
            // so characters like ), ', " immediately after ? don't get pushed to a new line.
            myFullText = Regex.Replace(myJokeText, @"\?(?=\s|$)", "?<br/>");
        }

        myFullText = myFullText.Replace("<br/><br/>", "<br/>").Replace("<br/> <br/>", "<br/>").Replace("<br/>  <br/>", "<br/>");
        myFullText = myFullText.EndsWith("?<br/>") ? myFullText.Substring(0, myFullText.Length - 5) : myFullText;
        if (!string.IsNullOrEmpty(joke.Attribution))
        {
            myFullText += $"<br /><i>({joke.Attribution})</i>";
        }
    }

    /// <summary>
    /// Handles a new star rating selection from the MudRating control.
    /// </summary>
    /// <param name="newValue">The newly selected star value (1–5).</param>
    private async Task OnRatingChanged(int newValue)
    {
        if (myJoke == null || newValue < 1 || newValue > 5 || ratingSubmitting)
        {
            return;
        }

        displayRatingValue = newValue;
        ratingSubmitting = true;
        StateHasChanged();

        try
        {
            var ratingKey = RatingKeyResolver.Resolve(HttpContextAccessor?.HttpContext);
            var userName = HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "ANON";

            var (success, _, averageRating, voteCount, wasInsert) =
                JokeRepository.SubmitOrUpdateRating(myJoke.JokeId, newValue, ratingKey, userName);

            if (success)
            {
                myJoke.Rating = averageRating;
                myJoke.VoteCount = voteCount;
                var action = wasInsert ? "Rating saved" : "Rating updated";
                Snackbar.Add($"{action}: {newValue} ★  |  Average: {averageRating:F1} ({voteCount} votes)", Severity.Success);
            }
            else
            {
                Snackbar.Add("Could not save your rating. Please try again.", Severity.Error);
            }
        }
        catch
        {
            Snackbar.Add("An error occurred while saving your rating.", Severity.Error);
        }
        finally
        {
            ratingSubmitting = false;
            StateHasChanged();
        }

        await Task.CompletedTask;
    }
}