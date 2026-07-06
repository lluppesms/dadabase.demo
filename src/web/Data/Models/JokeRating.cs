//-----------------------------------------------------------------------
// <copyright file="JokeRating.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Rating Table
// </summary>
//-----------------------------------------------------------------------
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace DadABase.Data.Models;

/// <summary>
/// Represents a joke rating entity.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("JokeRating", Schema = "Dad")]
public class JokeRating
{
    //CREATE TABLE [Dad].[JokeRating](
    //	[JokeRatingId] [int] IDENTITY(1,1) NOT NULL,
    //	[JokeId] [int] NOT NULL,
    //	[UserRating] [int] NOT NULL,
    //	[CreateDateTime] [datetime] NOT NULL,
    //	[CreateUserName] [nvarchar](255) NOT NULL,

    /// <summary>
    /// Gets or sets the joke rating identifier.
    /// </summary>
    /// <value>
    /// An integer representing the joke rating identifier.
    /// </value>
    [Key, Column(Order = 0)]
    [Required(ErrorMessage = "JokeRatingId is required")]
    [Display(Name = "JokeRatingId", Description = "This is the JokeRatingId field.", Prompt = "Enter JokeRatingId")]
    public int JokeRatingId { get; set; }

    /// <summary>
    /// Gets or sets the joke identifier.
    /// </summary>
    /// <value>
    /// An integer representing the associated joke identifier.
    /// </value>
    [Required(ErrorMessage = "JokeId is required")]
    [Display(Name = "JokeId", Description = "This is the JokeId field.", Prompt = "Enter JokeId")]
    public int JokeId { get; set; }

    /// <summary>
    /// Gets or sets the user rating.
    /// </summary>
    /// <value>
    /// An integer representing the rating provided by the user.
    /// </value>
    [Display(Name = "User Rating", Description = "This is the User Rating field.", Prompt = "Enter User Rating")]
    public int UserRating { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> value indicating when the record was created.
    /// </value>
    [Display(Name = "Create Date Time", Description = "This is the Create Date Time field.", Prompt = "Enter Create Date Time")]
    [DataType(DataType.DateTime)]
    public DateTime CreateDateTime { get; set; }

    /// <summary>
    /// Gets or sets the opaque rating user key that uniquely identifies the rater.
    /// Authenticated users: their identity claim value.
    /// Anonymous users: "ANON_IP_" followed by a SHA-256 hash of the client IP and an app salt.
    /// Together with <see cref="JokeId"/>, this forms the unique constraint that enforces
    /// one rating per user per joke.
    /// </summary>
    /// <value>A non-empty string identifying the rater; never a raw IP address.</value>
    [Display(Name = "Rating User Key", Description = "Opaque key identifying the rater.", Prompt = "Enter Rating User Key")]
    [StringLength(255)]
    public string RatingUserKey { get; set; } = "UNKNOWN";

    /// <summary>
    /// Gets or sets the name of the user who created the record.
    /// </summary>
    /// <value>
    /// A string representing the username of the creator.
    /// </value>
    [Display(Name = "Create User Name", Description = "This is the Create User Name field.", Prompt = "Enter Create User Name")]
    [StringLength(255)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeRating"/> class.
    /// </summary>
    public JokeRating()
    {
        JokeRatingId = 0;
        JokeId = 0;
        UserRating = 0;
        RatingUserKey = "UNKNOWN";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeRating"/> class.
    /// </summary>
    /// <param name="jokeId">The associated joke identifier.</param>
    public JokeRating(int jokeId)
    {
        JokeRatingId = 0;
        JokeId = jokeId;
        UserRating = 0;
        RatingUserKey = "UNKNOWN";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeRating"/> class.
    /// </summary>
    /// <param name="jokeId">The associated joke identifier.</param>
    /// <param name="userRating">The user rating value.</param>
    public JokeRating(int jokeId, int userRating)
    {
        JokeRatingId = 0;
        JokeId = jokeId;
        UserRating = userRating;
        RatingUserKey = "UNKNOWN";
        CreateUserName = "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeRating"/> class with a rating user key.
    /// </summary>
    /// <param name="jokeId">The associated joke identifier.</param>
    /// <param name="userRating">The user rating value.</param>
    /// <param name="ratingUserKey">The opaque key identifying the rater.</param>
    public JokeRating(int jokeId, int userRating, string ratingUserKey)
    {
        JokeRatingId = 0;
        JokeId = jokeId;
        UserRating = userRating;
        RatingUserKey = ratingUserKey ?? "UNKNOWN";
        CreateUserName = ratingUserKey ?? "UNKNOWN";
        CreateDateTime = DateTime.UtcNow;
    }
}
