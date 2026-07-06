//-----------------------------------------------------------------------
// <copyright file="JokeRateRequest.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Request body for POST /api/joke/rate
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data;

/// <summary>
/// Request body for the joke rate endpoint.
/// </summary>
[ExcludeFromCodeCoverage]
public class JokeRateRequest
{
    /// <summary>
    /// Gets or sets the identifier of the joke to rate.
    /// </summary>
    public int JokeId { get; set; }

    /// <summary>
    /// Gets or sets the star rating value (1–5 inclusive).
    /// </summary>
    public int UserRating { get; set; }
}
