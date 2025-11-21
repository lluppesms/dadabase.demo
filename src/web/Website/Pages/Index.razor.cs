//-----------------------------------------------------------------------
// <copyright file="Index.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Index Page Code Behind
// </summary>
//-----------------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;

namespace DadABase.Web.Pages;

/// <summary>
/// Index Page Code Behind
/// </summary>
[AllowAnonymous]
public partial class Index : ComponentBase
{
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] IJokeRepository JokeRepository { get; set; }
    [Inject] IAIHelper GenAIAgent { get; set; }

    private Joke myJoke = new();
    private readonly bool addDelay = false;
    private LoadingIndicator jokeLoadingIndicator;
    private SnackbarStack snackbarstack;

    // Store the last 10 jokes
    private List<Joke> jokeHistory = new();
    private bool isHistoryCollapsed = true;

    private bool imageDescriptionGenerated = false;
    private string jokeImageMessage = string.Empty;
    private string jokeImageDescription = string.Empty;
    private string jokeImageUrl = string.Empty;
    private bool showButtons = false;
    private bool imageGenerating = false;
    private LoadingIndicator imageLoadingIndicator;
    private Modal imageDescriptionPopup;

    /// <summary>
    /// Initialization
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
            await ExecuteRandom();
            StateHasChanged();
        }
    }

    private async Task ExecuteRandom()
    {
        showButtons = false;
        imageDescriptionGenerated = false;
        myJoke = new();
        await jokeLoadingIndicator.Show();
        var timer = Stopwatch.StartNew();
        if (addDelay) { await Task.Delay(500).ConfigureAwait(false); } // I want to see the spinners for now...
        myJoke = JokeRepository.GetRandomJoke();
        // Add to history, but skip if duplicate of last
        if (jokeHistory.Count == 0 || jokeHistory[0].JokeTxt != myJoke.JokeTxt)
        {
            jokeHistory.Insert(0, myJoke);
            if (jokeHistory.Count > 10)
                jokeHistory.RemoveAt(jokeHistory.Count - 1);
        }
        var elaspsedMS = timer.ElapsedMilliseconds;
        await jokeLoadingIndicator.Hide().ConfigureAwait(false);
        await snackbarstack.PushAsync($"Joke Elapsed: {(decimal)elaspsedMS / 1000m:0.0} seconds", SnackbarColor.Info).ConfigureAwait(false);

        jokeImageMessage = string.Empty;
        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;

        jokeImageMessage = "🚀 Generating a mental image of this scenario...";

        await imageLoadingIndicator.Show();
        StateHasChanged();

        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;

        var scene = $"{myJoke.JokeTxt} ({myJoke.JokeCategoryTxt})";
        (jokeImageDescription, var success, var errorMessage) = await GenAIAgent.GetJokeSceneDescription(scene);
        jokeImageMessage = string.Empty;
        imageDescriptionGenerated = success;
        showButtons = true;
        await imageLoadingIndicator.Hide();
    }
    private async Task CreatePicture()
    {
        showButtons = false;
        imageGenerating = true;
        jokeImageMessage = "🚀 OK - I've got an idea! Let me draw that for you! (gimme a sec...)";
        await imageLoadingIndicator.Show();
        StateHasChanged();

        (jokeImageUrl, var genSuccess, var genErrorMessage) = await GenAIAgent.GenerateAnImage(jokeImageDescription);
        jokeImageMessage = genSuccess ? string.Empty : genErrorMessage;
        showButtons = true;
        imageGenerating = false;
        await imageLoadingIndicator.Hide();
        StateHasChanged();
    }
    private void ToggleHistory()
    {
        isHistoryCollapsed = !isHistoryCollapsed;
    }

    private void ShowImageDescriptionPopup()
    {
        imageDescriptionPopup.Show();
    }

    private void CloseImageDescriptionPopup()
    {
        imageDescriptionPopup.Hide();
    }
}
