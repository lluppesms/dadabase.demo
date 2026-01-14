//-----------------------------------------------------------------------
// <copyright file="JokeCategory.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// JokeCategory Table
// </summary>
//-----------------------------------------------------------------------
namespace JokeAnalyzer.Models;

/// <summary>
/// JokeCategory Table
/// </summary>
[Table("JokeCategory")]
public class JokeCategory
{
    /// <summary>
    /// JokeCategory Id
    /// </summary>
    [Key]
    public int JokeCategoryId { get; set; }

    /// <summary>
    /// Joke Category Text
    /// </summary>
    [StringLength(500)]
    [Required]
    public string JokeCategoryTxt { get; set; } = string.Empty;

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
