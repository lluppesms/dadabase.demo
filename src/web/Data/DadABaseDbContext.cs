//-----------------------------------------------------------------------
// <copyright file="DadABaseDbContext.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// DadABase Database Context
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data;

/// <summary>
/// DadABase Database Context
/// </summary>
/// <remarks>
/// Initializes a new instance of the DadABaseDbContext class
/// </remarks>
/// <param name="options">DbContext options</param>
[ExcludeFromCodeCoverage]
public class DadABaseDbContext(DbContextOptions<DadABaseDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Jokes
    /// </summary>
    public DbSet<Joke> Jokes { get; set; }

    /// <summary>
    /// Joke Categories
    /// </summary>
    public DbSet<JokeCategory> JokeCategories { get; set; }

    /// <summary>
    /// Joke Ratings
    /// </summary>
    public DbSet<JokeRating> JokeRatings { get; set; }

    /// <summary>
    /// Joke to Category Junction
    /// </summary>
    public DbSet<JokeJokeCategory> JokeJokeCategories { get; set; }

    /// <summary>
    /// On Model Creating
    /// </summary>
    /// <param name="modelBuilder">Model Builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal precision for Rating property to match SQL column decimal(3,1)
        modelBuilder.Entity<Joke>()
            .Property(j => j.Rating)
            .HasPrecision(3, 1);

        // Configure composite key for JokeJokeCategory
        modelBuilder.Entity<JokeJokeCategory>()
            .HasKey(jjc => new { jjc.JokeId, jjc.JokeCategoryId });
    }
}
