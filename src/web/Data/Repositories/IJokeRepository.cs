//-----------------------------------------------------------------------
// <copyright file="IJokeRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// Joke Interface
/// </summary>
public interface IJokeRepository
{
    /// <summary>
    /// Find All Records
    /// </summary>
    /// <param name="activeInd">Active?</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Records</returns>
    IQueryable<Joke> ListAll(string activeInd = "Y", string requestingUserName = "ANON");

    /// <summary>
    /// Find One Specific Joke
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <param name="id">id</param>
    /// <returns>Records</returns>
    Joke GetOne(int id, string requestingUserName = "ANON");

    /// <summary>
    /// Get a random joke
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Record</returns>
    Joke GetRandomJoke(string requestingUserName = "ANON");

    /// <summary>
    /// Get Joke Categories
    /// </summary>
    /// <returns>List of Category Names</returns>
    IQueryable<string> GetJokeCategories(string requestingUserName = "ANON");

    /// <summary>
    /// Find Records by Search Text and/or Category
    /// </summary>
    /// <param name="searchTxt">Search</param>
    /// <param name="jokeCategoryTxt">Category</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Records</returns>
    IQueryable<Joke> SearchJokes(string searchTxt, string jokeCategoryTxt, string requestingUserName = "ANON");

    /// <summary>
    /// Update ImageTxt field for a specific joke
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
    /// <param name="imageTxt">Image description text</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Success</returns>
    bool UpdateImageTxt(int jokeId, string imageTxt, string requestingUserName = "ANON");

    /// <summary>
    /// Export all jokes and categories to SQL format
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>SQL script content</returns>
    string ExportToSql(string requestingUserName = "ANON");

    /// <summary>
    /// Update a joke
    /// </summary>
    /// <param name="joke">Joke to update</param>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>Success</returns>
    bool UpdateJoke(Joke joke, string requestingUserName = "ANON");

    /// <summary>
    /// Get all joke categories (entities, not just names)
    /// </summary>
    /// <param name="requestingUserName">Requesting UserName</param>
    /// <returns>List of JokeCategory entities</returns>
    IQueryable<JokeCategory> GetAllCategories(string requestingUserName = "ANON");

    /// <summary>
    /// Update joke categories
    /// </summary>
    /// <param name="jokeId">Joke ID</param>
        /// <param name="categoryIds">List of category IDs</param>
        /// <param name="requestingUserName">Requesting UserName</param>
        /// <returns>Success</returns>
        bool UpdateJokeCategories(int jokeId, List<int> categoryIds, string requestingUserName = "ANON");

        /// <summary>
        /// Add a new joke
        /// </summary>
        /// <param name="joke">Joke to add</param>
        /// <param name="requestingUserName">Requesting UserName</param>
        /// <returns>The ID of the newly created joke, or -1 if failed</returns>
        int AddJoke(Joke joke, string requestingUserName = "ANON");

        /// <summary>
        /// Disposal
        /// </summary>
        void Dispose();
    }
