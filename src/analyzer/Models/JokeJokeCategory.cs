//-----------------------------------------------------------------------
// <copyright file="JokeJokeCategory.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// JokeJokeCategory Junction Table
// </summary>
//-----------------------------------------------------------------------
namespace JokeAnalyzer.Models;

/// <summary>
/// JokeJokeCategory Junction Table
/// </summary>
[Table("JokeJokeCategory")]
public class JokeJokeCategory
{
    /// <summary>
    /// Joke Id
    /// </summary>
    [Key, Column(Order = 0)]
    public int JokeId { get; set; }

    /// <summary>
    /// Joke Category Id
    /// </summary>
    [Key, Column(Order = 1)]
    public int JokeCategoryId { get; set; }

    /// <summary>
    /// Create Date Time
    /// </summary>
    public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Create User Name
    /// </summary>
    [StringLength(255)]
    public string CreateUserName { get; set; } = "UNKNOWN";
}
