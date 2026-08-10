//-----------------------------------------------------------------------
// <copyright file="IAIHelper.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// AI Helper Interface
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Repositories;

/// <summary>
/// AI Helper Interface
/// </summary>
public interface IAIHelper
{
    /// <summary>
    /// Get a joke scene description using AI
    /// </summary>
    /// <param name="jokeText">Joke text</param>
    /// <returns>Tuple with description, success flag, and message</returns>
    Task<(string description, bool success, string message)> GetJokeSceneDescription(string jokeText);

    /// <summary>
    /// Generate an image using AI
    /// </summary>
    /// <param name="imageDescription">Image description</param>
    /// <param name="jokeId">Joke ID for saving the image</param>
    /// <returns>Tuple with image URL, success flag, and message</returns>
    Task<(string, bool, string)> GenerateAnImage(string imageDescription, int jokeId = 0);

    /// <summary>
    /// Save an already-generated base64 image to blob storage
    /// </summary>
    /// <param name="base64ImageDataUrl">Base64 data URL (e.g., data:image/png;base64,...)</param>
    /// <param name="jokeId">Joke ID for saving the image</param>
    /// <returns>Tuple with blob URL, success flag, and message</returns>
    Task<(string blobUrl, bool success, string message)> SaveBase64ImageToBlob(string base64ImageDataUrl, int jokeId);

    /// <summary>
    /// Get the image file path for a joke
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
    /// <returns>Image file path if exists, empty string otherwise</returns>
    string GetJokeImagePath(int jokeId);

    /// <summary>
    /// Suggest relevant categories for a joke
    /// </summary>
    /// <param name="jokeText">The joke text</param>
    /// <param name="availableCategories">All available category names</param>
    /// <returns>Tuple with list of suggested category names, success flag, and message</returns>
    Task<(List<string> suggestedCategories, bool success, string message)> SuggestCategories(string jokeText, IEnumerable<string> availableCategories);

    /// <summary>
    /// Analyze joke to get both category suggestions and scene description in a single AI call
    /// </summary>
    /// <param name="jokeText">The joke text</param>
    /// <param name="availableCategories">All available category names</param>
    /// <returns>Tuple with category list, scene description, success flag, and message</returns>
    Task<(List<string> suggestedCategories, string sceneDescription, bool success, string message)> AnalyzeJoke(string jokeText, IEnumerable<string> availableCategories);
}
