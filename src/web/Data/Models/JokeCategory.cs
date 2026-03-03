//-----------------------------------------------------------------------
// <copyright file="JokeCategory.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// JokeCategory Table
// </summary>
//-----------------------------------------------------------------------
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace DadABase.Data.Models;

/// <summary>
/// Represents a joke category entity in the database.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("JokeCategory")]
public class JokeCategory
{
    //CREATE TABLE [dbo].[JokeCategory](
    //	[JokeCategoryId] [int] IDENTITY(1,1) NOT NULL,
    //	[JokeCategoryTxt] [nvarchar](500) NULL,
    //	[SortOrderNbr] [int] NOT NULL,
    //	[ActiveInd] [nchar](1) NOT NULL,
    //	[CreateDateTime] [datetime] NOT NULL,
    //	[CreateUserName] [nvarchar](255) NOT NULL,
    //	[ChangeDateTime] [datetime] NOT NULL,
    //	[ChangeUserName] [nvarchar](255) NOT NULL,

    /// <summary>
    /// Gets or sets the unique identifier for the joke category.
    /// </summary>
    /// <value>The integer key uniquely identifying the joke category.</value>
    [Key, Column(Order = 0)]
    [Required(ErrorMessage = "JokeCategoryId is required")]
    [Display(Name = "JokeCategoryId", Description = "This is the JokeCategoryId field.", Prompt = "Enter JokeCategoryId")]
    public int JokeCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the text content of the joke category.
    /// </summary>
    /// <value>A string containing the category text.</value>
    [Display(Name = "Joke Category Text", Description = "This is the Joke Category Text field.", Prompt = "Enter Joke TCategory ext")]
    [StringLength(500)]
    [Required(ErrorMessage = "Joke Category Text is required")]
    public string? JokeCategoryTxt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the category is active.
    /// </summary>
    /// <value>A string representing the active indicator ("Y" or "N").</value>
    [JsonIgnore]
    [Display(Name = "Active", Description = "This is the Active field.", Prompt = "Enter Active")]
    [StringLength(1)]
    public string? ActiveInd { get; set; }

    /// <summary>
    /// Gets or sets the sort order number for the category.
    /// </summary>
    /// <value>An integer used for sorting.</value>
    [JsonIgnore]
    [Display(Name = "Sort Order", Description = "This is the Sort Order field.", Prompt = "Enter Sort Order")]
    public int SortOrderNbr { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the category was created.
    /// </summary>
    /// <value>A <see cref="DateTime"/> value indicating when the record was inserted.</value>
    [JsonIgnore]
    [Display(Name = "Create Date Time", Description = "This is the Create Date Time field.", Prompt = "Enter Create Date Time")]
    [DataType(DataType.DateTime)]
    public DateTime CreateDateTime { get; set; }

    /// <summary>
    /// Gets or sets the username of the person who created the category.
    /// </summary>
    /// <value>A string containing the creator's username.</value>
    [JsonIgnore]
    [Display(Name = "Create User Name", Description = "This is the Create User Name field.", Prompt = "Enter Create User Name")]
    [StringLength(255)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the category was last updated.
    /// </summary>
    /// <value>A <see cref="DateTime"/> value indicating when the record was last modified.</value>
    [JsonIgnore]
    [Display(Name = "Change Date Time", Description = "This is the Change Date Time field.", Prompt = "Enter Change Date Time")]
    [DataType(DataType.DateTime)]
    public DateTime ChangeDateTime { get; set; }

    /// <summary>
    /// Gets or sets the username of the person who last updated the category.
    /// </summary>
    /// <value>A string containing the last modifier's username.</value>
    [JsonIgnore]
    [Display(Name = "Change User Name", Description = "This is the Change User Name field.", Prompt = "Enter Change User Name")]
    [StringLength(255)]
    public string? ChangeUserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeCategory"/> class.
    /// </summary>
    public JokeCategory()
    {
        JokeCategoryId = 0;
        JokeCategoryTxt = string.Empty;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeCategory"/> class with the specified ID.
    /// </summary>
    /// <param name="jokeCategoryId">The unique identifier of the joke category.</param>
    public JokeCategory(int jokeCategoryId)
    {
        JokeCategoryId = jokeCategoryId;
        JokeCategoryTxt = string.Empty;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeCategory"/> class with the specified category text.
    /// </summary>
    /// <param name="jokeCategoryTxt">The text of the new category.</param>
    public JokeCategory(string jokeCategoryTxt)
    {
        JokeCategoryId = 0;
        JokeCategoryTxt = jokeCategoryTxt;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeCategory"/> class with the specified ID and category text.
    /// </summary>
    /// <param name="jokeCategoryId">The unique identifier of the joke category.</param>
    /// <param name="jokeCategoryTxt">The text of the category.</param>
    public JokeCategory(int jokeCategoryId, string jokeCategoryTxt)
    {
        JokeCategoryId = jokeCategoryId;
        JokeCategoryTxt = jokeCategoryTxt;
        SortOrderNbr = 50;
        ActiveInd = "Y";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
        ChangeUserName = "UNKNOWN";
        ChangeDateTime = DateTime.UtcNow;
    }
}
