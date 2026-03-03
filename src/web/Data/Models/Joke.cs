//-----------------------------------------------------------------------
// <copyright file="Joke.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Table
// </summary>
//-----------------------------------------------------------------------
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace DadABase.Data.Models;

/// <summary>
/// Represents a joke entity in the database.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("Joke")]
public class Joke
{
    //CREATE TABLE [dbo].[Joke](
    //  [JokeId] [int] IDENTITY(1,1) NOT NULL,
    //  [JokeTxt] [nvarchar](max) NULL,
    //  [JokeCategoryId] [int] NULL,
    //  [Attribution] [nvarchar](500) NULL,
    //  [SortOrderNbr] [int] NOT NULL,
    //  [Rating] decimal (3,1)  NULL,
    //  [ActiveInd] [nchar](1) NOT NULL,
    //  [CreateDateTime] [datetime] NOT NULL,
    //  [CreateUserName] [nvarchar](255) NOT NULL,
    //  [ChangeDateTime] [datetime] NOT NULL,
    //  [ChangeUserName] [nvarchar](255) NOT NULL,

    /// <summary>
    /// Gets or sets the unique identifier for the joke.
    /// </summary>
    /// <value>The integer key uniquely identifying the joke.</value>
    [Key, Column(Order = 0)]
    [Required(ErrorMessage = "JokeId is required")]
    [Display(Name = "JokeId", Description = "This is the JokeId field.", Prompt = "Enter JokeId")]
    public int JokeId { get; set; }

    /// <summary>
    /// Gets or sets the text content of the joke.
    /// </summary>
    /// <value>A string containing the actual joke text.</value>
    [Display(Name = "Joke Text", Description = "This is the Joke Text field.", Prompt = "Enter Joke Text")]
    [DataType(DataType.MultilineText)]
    [Required(ErrorMessage = "Joke Text is required")]
    public string? JokeTxt { get; set; }

    /// <summary>
    /// Gets or sets multiple categories associated with the joke.
    /// </summary>
    /// <remarks>
    /// This is populated by stored procedures via FOR XML PATH aggregation, not a direct table column.
    /// Do NOT add [NotMapped] - EF Core needs to map this from raw SQL query results.
    /// </remarks>
    /// <value>A comma-separated string of categories.</value>
    [Display(Name = "Categories", Description = "Multiple categories for this joke (comma-separated).", Prompt = "Enter Categories")]
    public string? Categories { get; set; }

    /// <summary>
    /// Gets or sets the attribution for the joke.
    /// </summary>
    /// <value>A string containing the source or attribution of the joke.</value>
    [Display(Name = "Attribution", Description = "This is the Attribution field.", Prompt = "Enter Attribution")]
    [StringLength(500)]
    public string? Attribution { get; set; }

    /// <summary>
    /// Gets or sets the image description text generated for the joke.
    /// </summary>
    /// <value>A string describing the context of the AI-generated image.</value>
    [Display(Name = "Image Description", Description = "This is the AI-generated image description field.", Prompt = "Enter Image Description")]
    public string? ImageTxt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the joke is active.
    /// </summary>
    /// <value>A string representing the active indicator ("Y" or "N").</value>
    [JsonIgnore]
    [Display(Name = "Active", Description = "This is the Active field.", Prompt = "Enter Active")]
    [StringLength(1)]
    public string? ActiveInd { get; set; }

    /// <summary>
    /// Gets or sets the sort order number for the joke.
    /// </summary>
    /// <value>An integer used for sorting.</value>
    [JsonIgnore]
    [Display(Name = "Sort Order", Description = "This is the Sort Order field.", Prompt = "Enter Sort Order")]
    public int SortOrderNbr { get; set; }

    /// <summary>
    /// Gets or sets the overall rating for the joke.
    /// </summary>
    /// <value>A decimal indicating the joke's rating, or <see langword="null"/> if not rated.</value>
    [JsonIgnore]
    [Display(Name = "Rating", Description = "This is the Rating field.", Prompt = "Enter Rating")]
    public decimal? Rating { get; set; }

    /// <summary>
    /// Gets or sets the total vote count for the joke.
    /// </summary>
    /// <value>An optional integer representing the total number of ratings cast.</value>
    [JsonIgnore]
    [Display(Name = "Vote Count", Description = "This is the Vote Count field.", Prompt = "Enter Vote Count")]
    public int? VoteCount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the joke was created.
    /// </summary>
    /// <value>A <see cref="DateTime"/> value indicating when the record was inserted.</value>
    [JsonIgnore]
    [Display(Name = "Create Date Time", Description = "This is the Create Date Time field.", Prompt = "Enter Create Date Time")]
    [DataType(DataType.DateTime)]
    public DateTime CreateDateTime { get; set; }

    /// <summary>
    /// Gets or sets the username of the person who created the joke.
    /// </summary>
    /// <value>A string containing the creator's username.</value>
    [JsonIgnore]
    [Display(Name = "Create User Name", Description = "This is the Create User Name field.", Prompt = "Enter Create User Name")]
    [StringLength(255)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the joke was last updated.
    /// </summary>
    /// <value>A <see cref="DateTime"/> value indicating when the record was last modified.</value>
    [JsonIgnore]
    [Display(Name = "Change Date Time", Description = "This is the Change Date Time field.", Prompt = "Enter Change Date Time")]
    [DataType(DataType.DateTime)]
    public DateTime ChangeDateTime { get; set; }

    /// <summary>
    /// Gets or sets the username of the person who last updated the joke.
    /// </summary>
    /// <value>A string containing the last modifier's username.</value>
    [JsonIgnore]
    [Display(Name = "Change User Name", Description = "This is the Change User Name field.", Prompt = "Enter Change User Name")]
    [StringLength(255)]
    public string? ChangeUserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Joke"/> class.
    /// </summary>
    public Joke()
    {
        JokeId = 0;
        JokeTxt = string.Empty;
        Categories = string.Empty;
        Attribution = string.Empty;
        ImageTxt = string.Empty;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Joke"/> class with the specified ID.
    /// </summary>
    /// <param name="jokeId">The unique identifier of the joke.</param>
    public Joke(int jokeId)
    {
        JokeId = jokeId;
        JokeTxt = string.Empty;
        Categories = string.Empty;
        Attribution = string.Empty;
        ImageTxt = string.Empty;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Joke"/> class with the specified text.
    /// </summary>
    /// <param name="jokeTxt">The text content of the joke.</param>
    public Joke(string jokeTxt)
    {
        JokeId = 0;
        JokeTxt = jokeTxt;
        Categories = string.Empty;
        Attribution = string.Empty;
        ImageTxt = string.Empty;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Joke"/> class with the specified text and categories.
    /// </summary>
    /// <param name="jokeTxt">The text content of the joke.</param>
    /// <param name="categories">The starting categories for the joke.</param>
    public Joke(string jokeTxt, string categories)
    {
        JokeId = 0;
        JokeTxt = jokeTxt;
        Categories = categories;
        Attribution = string.Empty;
        ImageTxt = string.Empty;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Joke"/> class with the specified text, categories, and image description.
    /// </summary>
    /// <param name="jokeTxt">The text content of the joke.</param>
    /// <param name="categories">The starting categories for the joke.</param>
    /// <param name="imageTxt">The AI-generated image description.</param>
    public Joke(string jokeTxt, string categories, string imageTxt)
    {
        JokeId = 0;
        JokeTxt = jokeTxt;
        Categories = categories;
        ImageTxt = imageTxt;
        Attribution = string.Empty;
        ImageTxt = imageTxt;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Joke"/> class with all core fields specified.
    /// </summary>
    /// <param name="jokeId">The unique identifier of the joke.</param>
    /// <param name="jokeTxt">The text content of the joke.</param>
    /// <param name="categories">The categories for the joke.</param>
    /// <param name="attribution">The source or attribution of the joke.</param>
    /// <param name="imageTxt">The image description.</param>
    public Joke(int jokeId, string jokeTxt, string categories, string attribution, string imageTxt)
    {
        JokeId = jokeId;
        JokeTxt = jokeTxt;
        Categories = categories;
        Attribution = attribution;
        ImageTxt = imageTxt;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }
}
