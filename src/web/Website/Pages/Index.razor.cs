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
public partial class Index : ComponentBase, IDisposable
{
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] IJokeRepository JokeRepository { get; set; }
    [Inject] IAIHelper GenAIAgent { get; set; }
    [Inject] ISnackbar Snackbar { get; set; }
    [Inject] DadABase.Web.Repositories.ThemeService ThemeService { get; set; }

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
    private bool isNinetiesTheme = false;

    protected override void OnInitialized()
    {
        ThemeService.OnThemeChanged += HandleThemeChanged;
    }

    /// <summary>
    /// Initialization
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
            var theme = await JsInterop.InvokeAsync<string>("localStorage.getItem", "theme-mode");
            isNinetiesTheme = theme == "nineties";
            await ExecuteRandom();
            StateHasChanged();
        }
    }

    private async void HandleThemeChanged()
    {
        var theme = await JsInterop.InvokeAsync<string>("localStorage.getItem", "theme-mode");
        isNinetiesTheme = theme == "nineties";
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ThemeService.OnThemeChanged -= HandleThemeChanged;
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
        // Snackbar.Add($"Joke Elapsed: {(decimal)elaspsedMS / 1000m:0.0} seconds", Severity.Info);

        // Reset AI-related state
        jokeImageMessage = string.Empty;
        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;

        // Check if an image already exists for this joke
        var existingImagePath = GenAIAgent.GetJokeImagePath(myJoke.JokeId);
        if (!string.IsNullOrEmpty(existingImagePath))
        {
            jokeImageUrl = existingImagePath;
            jokeImageDescription = myJoke.ImageTxt ?? string.Empty;
            imageGenerated = true;
        }

        StateHasChanged();
    }
    private async Task GenerateAIImage()
    {
        if (myJoke == null || string.IsNullOrEmpty(myJoke.JokeTxt))
        {
            Snackbar.Add("No joke loaded yet.", Severity.Info);
            return;
        }

        // Check if ImageTxt already exists - if so, skip LLM call
        if (!string.IsNullOrEmpty(myJoke.ImageTxt))
        {
            jokeImageDescription = myJoke.ImageTxt;
            jokeImageMessage = "🚀 The DadJoke AI has created a description! Let me draw that for you! (gimme a sec...)";
            imageLoading = true;
            StateHasChanged();

            // Skip to image generation with jokeId
            (jokeImageUrl, var imgSuccessFromCache, var imgErrorFromCache) = await GenAIAgent.GenerateAnImage(jokeImageDescription, myJoke.JokeId);
            jokeImageMessage = imgSuccessFromCache ? "The DadJoke AI tried to comprehend this joke and has done it's best to draw a mental picture for you!" : imgErrorFromCache;
            imageGenerated = imgSuccessFromCache;
            imageLoading = false;
            StateHasChanged();
            return;
        }

        // Step 1: Generate image description
        jokeImageMessage = "🚀 Generating a mental image of this scenario...";
        imageLoading = true;
        StateHasChanged();

        var scene = $"{myJoke.JokeTxt} ({myJoke.Categories})";
        (jokeImageDescription, var descSuccess, var descErrorMessage) = await GenAIAgent.GetJokeSceneDescription(scene);

        if (!descSuccess)
        {
            jokeImageMessage = descErrorMessage;
            imageLoading = false;
            StateHasChanged();
            return;
        }

        // Save the description to the database
        var updateSuccess = JokeRepository.UpdateImageTxt(myJoke.JokeId, jokeImageDescription);
        if (updateSuccess)
        {
            myJoke.ImageTxt = jokeImageDescription;
        }

        // Step 2: Generate the actual image from the description with jokeId
        jokeImageMessage = "🚀 OK - I've got an idea! Let me draw that for you! (gimme a sec...)";
        StateHasChanged();

        (jokeImageUrl, var imgSuccess, var imgErrorMessage) = await GenAIAgent.GenerateAnImage(jokeImageDescription, myJoke.JokeId);
        jokeImageMessage = imgSuccess ? "The DadJoke AI tried to comprehend this joke and has done it's best to draw a mental picture for you!" : imgErrorMessage;
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
