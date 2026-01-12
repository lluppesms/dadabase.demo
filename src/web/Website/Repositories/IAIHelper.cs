//-----------------------------------------------------------------------
// <copyright file="IAIHelper.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
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
    /// <returns>Tuple with image URL, success flag, and message</returns>
    Task<(string, bool, string)> GenerateAnImage(string imageDescription);
}
