//-----------------------------------------------------------------------
// <copyright file="IJokeRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// Represents the repository interface for accessing and managing joke data.
/// </summary>
public interface IJokeRepository
{
    /// <summary>
    /// Finds all joke records.
    /// </summary>
    /// <param name="activeInd">The active indicator used to filter jokes. The default is "Y".</param>
    /// <param name="requestingUserName">The username of the user requesting the records. The default is "ANON".</param>
    /// <returns>An <see cref="IQueryable{T}"/> of <see cref="Joke"/> records.</returns>
    IQueryable<Joke> ListAll(string activeInd = "Y", string requestingUserName = "ANON");

    /// <summary>
    /// Returns the most recently added jokes, limited to the specified count.
    /// For SQL repositories, results are ordered by <see cref="Joke.CreateDateTime"/> descending.
    /// For JSON repositories, the first <paramref name="count"/> entries are returned.
    /// </summary>
    /// <param name="count">The maximum number of jokes to return. The default is 100.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of the most recent <see cref="Joke"/> records.</returns>
    IQueryable<Joke> GetRecentAdditions(int count = 100);

    /// <summary>
    /// Finds a specific joke by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the joke.</param>
    /// <param name="requestingUserName">The username of the user requesting the record. The default is "ANON".</param>
    /// <returns>The specified <see cref="Joke"/> record.</returns>
    Joke GetOne(int id, string requestingUserName = "ANON");

    /// <summary>
    /// Gets a random joke.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the record. The default is "ANON".</param>
    /// <returns>A random <see cref="Joke"/> record.</returns>
    Joke GetRandomJoke(string requestingUserName = "ANON");

    /// <summary>
    /// Gets the names of all joke categories.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the categories. The default is "ANON".</param>
    /// <returns>An <see cref="IQueryable{T}"/> of category names.</returns>
    IQueryable<string> GetJokeCategories(string requestingUserName = "ANON");

    /// <summary>
    /// Finds joke records by search text and/or category.
    /// </summary>
    /// <param name="searchTxt">The text used to search jokes.</param>
    /// <param name="jokeCategoryTxt">The text used to filter jokes by category.</param>
    /// <param name="requestingUserName">The username of the user requesting the records. The default is "ANON".</param>
    /// <returns>An <see cref="IQueryable{T}"/> of matching <see cref="Joke"/> records.</returns>
    IQueryable<Joke> SearchJokes(string searchTxt, string jokeCategoryTxt, string requestingUserName = "ANON");

    /// <summary>
    /// Updates the image description text for a specific joke.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke.</param>
    /// <param name="imageTxt">The new image description text.</param>
    /// <param name="requestingUserName">The username of the user performing the update. The default is "ANON".</param>
    /// <returns><see langword="true" /> if the update was successful; otherwise, <see langword="false" />.</returns>
    bool UpdateImageTxt(int jokeId, string imageTxt, string requestingUserName = "ANON");

    /// <summary>
    /// Exports all jokes and categories to a SQL format.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the export. The default is "ANON".</param>
    /// <returns>A string containing the SQL script content.</returns>
    string ExportToSql(string requestingUserName = "ANON");

    /// <summary>
    /// Exports all jokes to a tab-delimited text format with fields: JokeId, Categories, JokeTxt, ImageTxt, Attribution, ActiveInd, SortOrderNbr, Rating, VoteCount.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the export. The default is "ANON".</param>
    /// <returns>A string containing the tab-delimited content including a header row.</returns>
    string ExportToTabDelimited(string requestingUserName = "ANON");

    /// <summary>
    /// Exports all jokes to a JSON array format with fields: JokeId, Categories, JokeTxt, ImageTxt, Attribution, ActiveInd, SortOrderNbr, Rating, VoteCount.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the export. The default is "ANON".</param>
    /// <returns>A string containing the JSON array of joke objects.</returns>
    string ExportToJson(string requestingUserName = "ANON");

    /// <summary>
    /// Imports jokes from a tab-delimited text string into the database. Parses the rows in C#,
    /// ensures all referenced categories exist, then inserts new jokes and their category associations
    /// using the existing repository methods. Jokes whose text already exists in the database are skipped.
    /// </summary>
    /// <param name="tabData">The tab-delimited content including a header row.</param>
    /// <param name="requestingUserName">The username of the user performing the import. The default is "ANON".</param>
    /// <returns>A tuple indicating success, the count of newly imported jokes, and a status message.</returns>
    /// <remarks>Deprecated: use <see cref="ImportFromTabDelimitedViaSproc"/> instead, which delegates all
    /// parsing and batch-insert work to the <c>usp_Joke_Import</c> stored procedure.</remarks>
    [Obsolete("Use ImportFromTabDelimitedViaSproc instead, which delegates batch import to usp_Joke_Import.")]
    (bool Success, int ImportedCount, string Message) ImportFromTabDelimited(string tabData, string requestingUserName = "ANON");

    /// <summary>
    /// Imports jokes from a tab-delimited text string by passing the raw TSV to the
    /// <c>usp_Joke_Import</c> stored procedure, which parses and batch-inserts data entirely in SQL.
    /// New categories are inserted, duplicate jokes (by text) are skipped, and category associations
    /// are created — all in a single database round-trip.
    /// </summary>
    /// <param name="tabData">The tab-delimited content including a header row.</param>
    /// <param name="removePreviousJokes">When <see langword="true"/>, all existing jokes, categories, and ratings are deleted and identity columns are reseeded before importing.</param>
    /// <param name="requestingUserName">The username of the user performing the import. The default is "ANON".</param>
    /// <returns>A tuple indicating success, the count of newly imported jokes, and a status message.</returns>
    (bool Success, int ImportedCount, string Message) ImportFromTabDelimitedViaSproc(string tabData, bool removePreviousJokes = false, string requestingUserName = "ANON");

    /// <summary>
    /// Updates an existing joke.
    /// </summary>
    /// <param name="joke">The <see cref="Joke"/> entity to update.</param>
    /// <param name="requestingUserName">The username of the user performing the update. The default is "ANON".</param>
    /// <returns><see langword="true" /> if the update was successful; otherwise, <see langword="false" />.</returns>
    bool UpdateJoke(Joke joke, string requestingUserName = "ANON");

    /// <summary>
    /// Gets all joke categories as entities.
    /// </summary>
    /// <param name="requestingUserName">The username of the user requesting the categories. The default is "ANON".</param>
    /// <returns>An <see cref="IQueryable{T}"/> of <see cref="JokeCategory"/> entities.</returns>
    IQueryable<JokeCategory> GetAllCategories(string requestingUserName = "ANON");

    /// <summary>
    /// Updates the categories associated with a specific joke.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke.</param>
    /// <param name="categoryIds">The list of category identifiers to associate.</param>
    /// <param name="requestingUserName">The username of the user performing the update. The default is "ANON".</param>
    /// <returns><see langword="true" /> if the update was successful; otherwise, <see langword="false" />.</returns>
    bool UpdateJokeCategories(int jokeId, List<int> categoryIds, string requestingUserName = "ANON");

    /// <summary>
    /// Adds a new joke.
    /// </summary>
    /// <param name="joke">The <see cref="Joke"/> entity to add.</param>
    /// <param name="requestingUserName">The username of the user performing the addition. The default is "ANON".</param>
    /// <returns>The identifier of the newly created joke, or <c>-1</c> if the addition failed.</returns>
    int AddJoke(Joke joke, string requestingUserName = "ANON");

    /// <summary>
    /// Deletes a specific joke.
    /// </summary>
    /// <param name="jokeId">The identifier of the joke to delete.</param>
    /// <param name="requestingUserName">The username of the user performing the deletion. The default is "ANON".</param>
    /// <returns><see langword="true" /> if the deletion was successful; otherwise, <see langword="false" />.</returns>
    bool DeleteJoke(int jokeId, string requestingUserName = "ANON");

        /// <summary>
        /// Disposal
        /// </summary>
        void Dispose();
    }
