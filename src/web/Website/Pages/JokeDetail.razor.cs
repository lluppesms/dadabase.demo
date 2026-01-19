//-----------------------------------------------------------------------
// <copyright file="JokeDetail.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Detail Page Code Behind
// </summary>
//-----------------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;

namespace DadABase.Web.Pages;

/// <summary>
/// Joke Detail Page Code Behind
/// </summary>
[AllowAnonymous]
public partial class JokeDetail : ComponentBase
{
    [Parameter]
    public int Id { get; set; }

    [Inject] IJokeRepository JokeRepository { get; set; }
    [Inject] IAIHelper GenAIAgent { get; set; }
    [Inject] ISnackbar Snackbar { get; set; }

    private Joke? myJoke;

    private bool imageGenerated = false;
    private string jokeImageMessage = string.Empty;
    private string jokeImageDescription = string.Empty;
    private string jokeImageUrl = string.Empty;
    private bool imageLoading = false;
    private bool imageDescriptionDialogVisible = false;

    protected override void OnInitialized()
    {
        LoadJoke();
    }

    private void LoadJoke()
    {
        myJoke = JokeRepository.GetOne(Id);

        if (myJoke == null) return;

        // Reset AI-related state
        jokeImageMessage = string.Empty;
        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;
        imageGenerated = false;

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
            jokeImageMessage = "🚀 Using existing description! Let me draw that for you! (gimme a sec...)";
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

    private void ShowImageDescriptionPopup()
    {
        imageDescriptionDialogVisible = true;
    }

    private void CloseImageDescriptionPopup()
    {
        imageDescriptionDialogVisible = false;
    }
}
