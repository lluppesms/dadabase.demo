//-----------------------------------------------------------------------
// <copyright file="Joke.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Table
// </summary>
//-----------------------------------------------------------------------
namespace JokeAnalyzer.Models;

/// <summary>
/// Joke Table
/// </summary>
[Table("Joke")]
public class Joke
{
    /// <summary>
    /// Joke Id
    /// </summary>
    [Key]
    public int JokeId { get; set; }

    /// <summary>
    /// Joke Text
    /// </summary>
    [Required]
    public string JokeTxt { get; set; } = string.Empty;

    /// <summary>
    /// Image Description Text
    /// </summary>
    public string? ImageTxt { get; set; }

    /// <summary>
    /// Attribution
    /// </summary>
    [StringLength(500)]
    public string? Attribution { get; set; }

    /// <summary>
    /// Active
    /// </summary>
    [StringLength(1)]
    public string ActiveInd { get; set; } = "Y";

    /// <summary>
    /// Sort Order
    /// </summary>
    public int SortOrderNbr { get; set; } = 50;

    /// <summary>
    /// Rating
    /// </summary>
    public decimal? Rating { get; set; }

    /// <summary>
    /// Vote Count
    /// </summary>
    public int? VoteCount { get; set; }

    /// <summary>
    /// Create Date Time
    /// </summary>
    public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Create User Name
    /// </summary>
    [StringLength(255)]
    public string CreateUserName { get; set; } = "UNKNOWN";

    /// <summary>
    /// Change Date Time
    /// </summary>
    public DateTime ChangeDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Change User Name
    /// </summary>
    [StringLength(255)]
    public string ChangeUserName { get; set; } = "UNKNOWN";
}
