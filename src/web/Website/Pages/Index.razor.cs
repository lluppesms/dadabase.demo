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
    [Inject] ISnackbar Snackbar { get; set; }

    private Joke myJoke = new();
    private readonly bool addDelay = false;
    private bool jokeLoading = false;

    // Store the last 10 jokes
    private List<Joke> jokeHistory = new();
    private bool isHistoryCollapsed = true;

    private bool imageGenerated = false;
    private string jokeImageMessage = string.Empty;
    private string jokeImageDescription = string.Empty;
    private string jokeImageUrl = string.Empty;
    private bool imageLoading = false;
    private bool imageDescriptionDialogVisible = false;

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
        imageGenerated = false;
        myJoke = new();
        jokeLoading = true;
        StateHasChanged();
        
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
        jokeLoading = false;
        Snackbar.Add($"Joke Elapsed: {(decimal)elaspsedMS / 1000m:0.0} seconds", Severity.Info);

        // Reset AI-related state
        jokeImageMessage = string.Empty;
        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;
        StateHasChanged();
    }
    private async Task GenerateAIImage()
    {
        if (myJoke == null || string.IsNullOrEmpty(myJoke.JokeTxt))
        {
            Snackbar.Add("No joke loaded yet.", Severity.Info);
            return;
        }

        // Step 1: Generate image description
        jokeImageMessage = "🚀 Generating a mental image of this scenario...";
        imageLoading = true;
        StateHasChanged();

        var scene = $"{myJoke.JokeTxt} ({myJoke.JokeCategoryTxt})";
        (jokeImageDescription, var descSuccess, var descErrorMessage) = await GenAIAgent.GetJokeSceneDescription(scene);
        
        if (!descSuccess)
        {
            jokeImageMessage = descErrorMessage;
            imageLoading = false;
            StateHasChanged();
            return;
        }

        // Step 2: Generate the actual image from the description
        jokeImageMessage = "🚀 OK - I've got an idea! Let me draw that for you! (gimme a sec...)";
        StateHasChanged();

        (jokeImageUrl, var imgSuccess, var imgErrorMessage) = await GenAIAgent.GenerateAnImage(jokeImageDescription);
        jokeImageMessage = imgSuccess ? string.Empty : imgErrorMessage;
        imageGenerated = imgSuccess;
        imageLoading = false;
        StateHasChanged();
    }
    private void ToggleHistory()
    {
        isHistoryCollapsed = !isHistoryCollapsed;
    }

    private void ShowImageDescriptionPopup()
    {
        imageDescriptionDialogVisible = true;
    }

    private void CloseImageDescriptionPopup()
    {
        imageDescriptionDialogVisible = false;
    }
}
