//-----------------------------------------------------------------------
// <copyright file="DadABaseDbContext.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// DadABase Database Context
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data;

/// <summary>
/// Represents the Entity Framework database context for DadABase.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DadABaseDbContext"/> class.
/// </remarks>
/// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
[ExcludeFromCodeCoverage]
public class DadABaseDbContext(DbContextOptions<DadABaseDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the set of jokes in the database.
    /// </summary>
    /// <value>A <see cref="DbSet{TEntity}"/> of <see cref="Joke"/> entities.</value>
    public DbSet<Joke> Jokes { get; set; }

    /// <summary>
    /// Gets or sets the set of joke categories in the database.
    /// </summary>
    /// <value>A <see cref="DbSet{TEntity}"/> of <see cref="JokeCategory"/> entities.</value>
    public DbSet<JokeCategory> JokeCategories { get; set; }

    /// <summary>
    /// Gets or sets the set of joke ratings in the database.
    /// </summary>
    /// <value>A <see cref="DbSet{TEntity}"/> of <see cref="JokeRating"/> entities.</value>
    public DbSet<JokeRating> JokeRatings { get; set; }

    /// <summary>
    /// Gets or sets the junction dataset representing the many-to-many relationship between jokes and categories.
    /// </summary>
    /// <value>A <see cref="DbSet{TEntity}"/> of <see cref="JokeJokeCategory"/> entities.</value>
    public DbSet<JokeJokeCategory> JokeJokeCategories { get; set; }

    /// <summary>
    /// Configures the schema needed for the DadABase context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
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
