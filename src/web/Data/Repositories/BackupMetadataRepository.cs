//-----------------------------------------------------------------------
// <copyright file="BackupMetadataRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Backup Metadata Repository
// </summary>
//-----------------------------------------------------------------------
using System.Data;
using System.Data.Common;
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// Persists backup export metadata to the <c>Dad.BackupMetadata</c> table via stored procedures.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BackupMetadataRepository"/> class.
/// </remarks>
/// <param name="context">The database context used to reach the database.</param>
public class BackupMetadataRepository(DadABaseDbContext context) : IBackupMetadataRepository
{
    private readonly DadABaseDbContext _context = context;

    /// <summary>
    /// Gets the metadata for the most recent successful export of the specified type.
    /// </summary>
    /// <param name="exportType">The export type, for example "Weekly".</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The last successful <see cref="BackupMetadata"/> record, or <see langword="null"/> if none exists.</returns>
    public async Task<BackupMetadata?> GetLastSuccessfulExportAsync(string exportType, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "[Dad].[usp_Get_Last_Successful_Backup]";
            command.CommandType = CommandType.StoredProcedure;
            AddParameter(command, "@ExportType", exportType ?? BackupConstants.WeeklyExportType);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new BackupMetadata
            {
                BackupMetadataId = GetValue<int>(reader, "BackupMetadataId"),
                ExportType = GetValue<string>(reader, "ExportType") ?? BackupConstants.WeeklyExportType,
                LastExportedAtUtc = GetValue<DateTime>(reader, "LastExportedAtUtc"),
                LastExportedMaxChangeDateTimeUtc = GetNullableValue<DateTime>(reader, "LastExportedMaxChangeDateTimeUtc"),
                LastExportedJokeCount = GetValue<int>(reader, "LastExportedJokeCount"),
                BackupBlobUri = GetValue<string>(reader, "BackupBlobUri"),
                Checksum = GetValue<string>(reader, "Checksum"),
                Status = GetValue<string>(reader, "Status") ?? BackupConstants.StatusSuccess,
                ErrorMessage = GetValue<string>(reader, "ErrorMessage"),
                CreatedAtUtc = GetValue<DateTime>(reader, "CreatedAtUtc")
            };
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }
    }

    /// <summary>
    /// Saves the metadata describing the outcome of an export run.
    /// </summary>
    /// <param name="metadata">The metadata to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveExportMetadataAsync(BackupMetadata metadata, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "[Dad].[usp_Upsert_Backup_Metadata]";
            command.CommandType = CommandType.StoredProcedure;
            AddParameter(command, "@ExportType", metadata.ExportType);
            AddParameter(command, "@LastExportedAtUtc", metadata.LastExportedAtUtc);
            AddParameter(command, "@LastExportedMaxChangeDateTimeUtc", metadata.LastExportedMaxChangeDateTimeUtc);
            AddParameter(command, "@LastExportedJokeCount", metadata.LastExportedJokeCount);
            AddParameter(command, "@BackupBlobUri", metadata.BackupBlobUri);
            AddParameter(command, "@Checksum", metadata.Checksum);
            AddParameter(command, "@Status", metadata.Status);
            AddParameter(command, "@ErrorMessage", metadata.ErrorMessage);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static T? GetValue<T>(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? default : (T)reader.GetValue(ordinal);
    }

    private static T? GetNullableValue<T>(DbDataReader reader, string columnName)
        where T : struct
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : (T)reader.GetValue(ordinal);
    }
}
