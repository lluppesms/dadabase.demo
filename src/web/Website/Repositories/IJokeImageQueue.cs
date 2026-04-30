//-----------------------------------------------------------------------
// <copyright file="IJokeImageQueue.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Background queue for deferred joke image generation
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Repositories;

/// <summary>
/// Represents a queue for scheduling deferred AI image generation for jokes.
/// Enqueue a joke ID and the background service will generate the scene description
/// and image automatically without blocking the user.
/// </summary>
public interface IJokeImageQueue
{
    /// <summary>
    /// Adds a joke to the background image-generation queue.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke to process.</param>
    void Enqueue(int jokeId);

    /// <summary>
    /// Attempts to dequeue the next joke ID for processing.
    /// </summary>
    /// <param name="jokeId">The dequeued joke identifier, or 0 if the queue is empty.</param>
    /// <returns><see langword="true"/> if a joke ID was retrieved; otherwise <see langword="false"/>.</returns>
    bool TryDequeue(out int jokeId);

    /// <summary>
    /// Gets the approximate number of jokes currently waiting to be processed.
    /// </summary>
    int Count { get; }
}
