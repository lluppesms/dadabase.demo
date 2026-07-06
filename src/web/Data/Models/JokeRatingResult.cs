//-----------------------------------------------------------------------
// <copyright file="JokeRatingResult.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Keyless result type returned by the [Dad].[usp_Joke_Rate] stored procedure.
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data.Models;

/// <summary>
/// Represents the single-row result set returned by <c>[Dad].[usp_Joke_Rate]</c>.
/// Mapped as a keyless entity so EF Core can materialise it from a raw SQL query.
/// </summary>
[ExcludeFromCodeCoverage]
[Keyless]
public class JokeRatingResult
{
    /// <summary>Gets or sets the joke identifier.</summary>
    public int JokeId { get; set; }

    /// <summary>Gets or sets the star value that was persisted (1–5).</summary>
    public int UserRating { get; set; }

    /// <summary>Gets or sets the recomputed average rating for the joke.</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Gets or sets the recomputed total vote count for the joke.</summary>
    public int VoteCount { get; set; }

    /// <summary>Gets or sets a value indicating whether a new row was inserted (<see langword="true"/>) or an existing row updated (<see langword="false"/>).</summary>
    public bool WasInsert { get; set; }
}
