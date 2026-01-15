//-----------------------------------------------------------------------
// <copyright file="JokeDbContext.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Database Context
// </summary>
//-----------------------------------------------------------------------
using JokeAnalyzer.Models;

namespace JokeAnalyzer;

/// <summary>
/// Joke Database Context
/// </summary>
public class JokeDbContext : DbContext
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
    /// Joke to Category Junction
    /// </summary>
    public DbSet<JokeJokeCategory> JokeJokeCategories { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public JokeDbContext(DbContextOptions<JokeDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// On Model Creating
    /// </summary>
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
