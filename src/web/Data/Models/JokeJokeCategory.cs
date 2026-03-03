//-----------------------------------------------------------------------
// <copyright file="JokeJokeCategory.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// JokeJokeCategory Junction Table
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data.Models;

/// <summary>
/// Represents a junction table for associating jokes with categories in the database.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("JokeJokeCategory")]
public class JokeJokeCategory
{
    /// <summary>
    /// Gets or sets the unique identifier of the associated joke.
    /// </summary>
    /// <value>The integer key uniquely identifying the joke.</value>
    [Key, Column(Order = 0)]
    [Required(ErrorMessage = "JokeId is required")]
    public int JokeId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the associated joke category.
    /// </summary>
    /// <value>The integer key uniquely identifying the joke category.</value>
    [Key, Column(Order = 1)]
    [Required(ErrorMessage = "JokeCategoryId is required")]
    public int JokeCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the relationship was created.
    /// </summary>
    /// <value>A <see cref="DateTime"/> value indicating when the record was inserted.</value>
    [JsonIgnore]
    [Display(Name = "Create Date Time")]
    [DataType(DataType.DateTime)]
    public DateTime CreateDateTime { get; set; }

    /// <summary>
    /// Gets or sets the username of the person who created the relationship.
    /// </summary>
    /// <value>A string containing the creator's username.</value>
    [JsonIgnore]
    [Display(Name = "Create User Name")]
    [StringLength(255)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeJokeCategory"/> class.
    /// </summary>
    public JokeJokeCategory()
    {
        JokeId = 0;
        JokeCategoryId = 0;
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeJokeCategory"/> class with the specified joke and category IDs.
    /// </summary>
    /// <param name="jokeId">The unique identifier of the joke.</param>
    /// <param name="jokeCategoryId">The unique identifier of the joke category.</param>
    public JokeJokeCategory(int jokeId, int jokeCategoryId)
    {
        JokeId = jokeId;
        JokeCategoryId = jokeCategoryId;
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
    }
}
