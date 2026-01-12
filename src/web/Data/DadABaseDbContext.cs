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
[ExcludeFromCodeCoverage]
public class DadABaseDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the DadABaseDbContext class
    /// </summary>
    /// <param name="options">DbContext options</param>
    public DadABaseDbContext(DbContextOptions<DadABaseDbContext> options)
        : base(options)
    {
    }

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
    /// On Model Creating
    /// </summary>
    /// <param name="modelBuilder">Model Builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure table names explicitly if needed
        // modelBuilder.Entity<Joke>().ToTable("Joke");
        // modelBuilder.Entity<JokeCategory>().ToTable("JokeCategory");
        // modelBuilder.Entity<JokeRating>().ToTable("JokeRating");
    }
}
