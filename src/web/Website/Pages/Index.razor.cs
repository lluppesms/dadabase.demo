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
    private bool aiFeaturesEnabled = false;

    // Store the last 10 jokes
    private List<Joke> jokeHistory = new();
    private bool isHistoryCollapsed = true;

    private bool imageDescriptionGenerated = false;
    private string jokeImageMessage = string.Empty;
    private string jokeImageDescription = string.Empty;
    private string jokeImageUrl = string.Empty;
    private bool showButtons = false;
    private bool imageGenerating = false;
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
        showButtons = false;
        imageDescriptionGenerated = false;
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

        jokeImageMessage = string.Empty;
        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;
        if (!aiFeaturesEnabled)
        {
            jokeImageMessage = "AI features are off. Enable the toggle to generate descriptions or images.";
            imageLoading = false;
            showButtons = false;
            StateHasChanged();
            return;
        }

        jokeImageMessage = "🚀 Generating a mental image of this scenario...";

        imageLoading = true;
        StateHasChanged();

        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;

        var scene = $"{myJoke.JokeTxt} ({myJoke.JokeCategoryTxt})";
        (jokeImageDescription, var success, var errorMessage) = await GenAIAgent.GetJokeSceneDescription(scene);
        jokeImageMessage = success ? string.Empty : errorMessage;
        imageDescriptionGenerated = success;
        showButtons = success;
        imageLoading = false;
        StateHasChanged();
    }
    private async Task CreatePicture()
    {
        if (!aiFeaturesEnabled)
        {
            Snackbar.Add("Enable AI features to generate images.", Severity.Info);
            return;
        }

        if (!imageDescriptionGenerated)
        {
            Snackbar.Add("Generate a scene description first, then request an image.", Severity.Info);
            return;
        }

        showButtons = false;
        imageGenerating = true;
        jokeImageMessage = "🚀 OK - I've got an idea! Let me draw that for you! (gimme a sec...)";
        imageLoading = true;
        StateHasChanged();

        (jokeImageUrl, var genSuccess, var genErrorMessage) = await GenAIAgent.GenerateAnImage(jokeImageDescription);
        jokeImageMessage = genSuccess ? string.Empty : genErrorMessage;
        showButtons = true;
        imageGenerating = false;
        imageLoading = false;
        StateHasChanged();
    }
    private void ToggleHistory()
    {
        isHistoryCollapsed = !isHistoryCollapsed;
    }

    private async Task GenerateAIDescription()
    {
        Console.WriteLine($"GenerateAIDescription called. aiFeaturesEnabled={aiFeaturesEnabled}");
        
        if (!aiFeaturesEnabled)
        {
            Snackbar.Add("Enable AI features first.", Severity.Info);
            return;
        }

        if (myJoke == null || string.IsNullOrEmpty(myJoke.JokeTxt))
        {
            Snackbar.Add("No joke loaded yet.", Severity.Info);
            return;
        }

        Snackbar.Add("Generating AI description...", Severity.Info);
        jokeImageMessage = "🚀 Generating a mental image of this scenario...";
        imageLoading = true;
        StateHasChanged();

        var scene = $"{myJoke.JokeTxt} ({myJoke.JokeCategoryTxt})";
        (jokeImageDescription, var success, var errorMessage) = await GenAIAgent.GetJokeSceneDescription(scene);
        jokeImageMessage = success ? string.Empty : errorMessage;
        imageDescriptionGenerated = success;
        showButtons = success;
        imageLoading = false;
        StateHasChanged();
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
