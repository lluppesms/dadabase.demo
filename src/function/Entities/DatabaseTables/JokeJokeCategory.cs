//-----------------------------------------------------------------------
// <copyright file="JokeJokeCategory.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// JokeJokeCategory Junction Table
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data;

/// <summary>
/// JokeJokeCategory Junction Table
/// </summary>
[ExcludeFromCodeCoverage]
[Table("JokeJokeCategory")]
public class JokeJokeCategory
{
    /// <summary>
    /// Joke Id
    /// </summary>
    [Key, Column(Order = 0)]
    [Required(ErrorMessage = "JokeId is required")]
    public int JokeId { get; set; }

    /// <summary>
    /// Joke Category Id
    /// </summary>
    [Key, Column(Order = 1)]
    [Required(ErrorMessage = "JokeCategoryId is required")]
    public int JokeCategoryId { get; set; }

    /// <summary>
    /// Create Date Time
    /// </summary>
    [JsonIgnore]
    [Display(Name = "Create Date Time")]
    [DataType(DataType.DateTime)]
    public DateTime CreateDateTime { get; set; }

    /// <summary>
    /// Create User Name
    /// </summary>
    [JsonIgnore]
    [Display(Name = "Create User Name")]
    [StringLength(255)]
    public string CreateUserName { get; set; }

    /// <summary>
    /// New Instance of JokeJokeCategory
    /// </summary>
    public JokeJokeCategory()
    {
        JokeId = 0;
        JokeCategoryId = 0;
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// New Instance of JokeJokeCategory
    /// </summary>
    public JokeJokeCategory(int jokeId, int jokeCategoryId)
    {
        JokeId = jokeId;
        JokeCategoryId = jokeCategoryId;
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
    }
}
