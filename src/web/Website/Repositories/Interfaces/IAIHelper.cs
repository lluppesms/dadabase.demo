//-----------------------------------------------------------------------
// <copyright file="IAIHelper.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Chat Agent Interface
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Repositories;

/// <summary>
/// AI Agent Interface
/// </summary>
public interface IAIHelper
{
    /// <summary>
    /// Give it a joke and get back an image description
    /// </summary>
    Task<(string description, bool success, string message)> GetJokeSceneDescription(string jokeText);

    /// <summary>
    /// Give this a description and get back an generated image URL
    /// </summary>
    Task<(string, bool, string)> GenerateAnImage(string imageDescription);
}
