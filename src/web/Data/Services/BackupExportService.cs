//-----------------------------------------------------------------------
// <copyright file="BackupExportService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Backup Export Service
// </summary>
//-----------------------------------------------------------------------
using System.Data.Common;
using DadABase.Data.Models;
using DadABase.Data.Repositories;

namespace DadABase.Data.Services;

/// <summary>
/// Builds backup payloads from the SQL data source.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BackupExportService"/> class.
/// </remarks>
/// <param name="jokeRepository">The joke repository used to read jokes and categories.</param>
/// <param name="context">The database context used for ratings and the change-detection snapshot.</param>
public class BackupExportService(IJokeRepository jokeRepository, DadABaseDbContext context) : IBackupExportService
{
    private readonly IJokeRepository _jokeRepository = jokeRepository;
    private readonly DadABaseDbContext _context = context;

    /// <summary>
    /// Builds the complete backup payload (jokes, categories, and ratings).
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A <see cref="BackupData"/> instance containing all exported records.</returns>
    public async Task<BackupData> BuildBackupDataAsync(CancellationToken cancellationToken = default)
    {
        var jokes = _jokeRepository.ListAll().ToList();
        var categories = _jokeRepository.GetAllCategories().ToList();
        var ratings = _context.JokeRatings == null
            ? []
            : await _context.JokeRatings.AsNoTracking().ToListAsync(cancellationToken);

        return new BackupData
        {
            Jokes = jokes,
            Categories = categories,
            Ratings = ratings
        };
    }

    /// <summary>
    /// Gets a snapshot of the current joke data used for change detection.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A <see cref="JokeChangeSnapshot"/> describing the current state of the joke data.</returns>
    public async Task<JokeChangeSnapshot> GetLastJokeChangeSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = new JokeChangeSnapshot();
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State != System.Data.ConnectionState.Open;

        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "[Dad].[usp_Get_Last_Joke_Change_Snapshot]";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                snapshot.MaxChangeDateTimeUtc = GetNullableDateTime(reader, "MaxChangeDateTimeUtc");
                snapshot.JokeCount = GetInt32(reader, "JokeCount");
                snapshot.CategoryCount = GetInt32(reader, "CategoryCount");
            }
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }

        return snapshot;
    }

    private static DateTime? GetNullableDateTime(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static int GetInt32(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }
}
