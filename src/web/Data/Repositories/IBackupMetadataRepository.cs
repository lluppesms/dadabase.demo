//-----------------------------------------------------------------------
// <copyright file="IBackupMetadataRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Backup Metadata Repository Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// Persists the audit trail of backup export runs.
/// </summary>
public interface IBackupMetadataRepository
{
    /// <summary>
    /// Gets the metadata for the most recent successful export of the specified type.
    /// </summary>
    /// <param name="exportType">The export type, for example "Weekly".</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The last successful <see cref="BackupMetadata"/> record, or <see langword="null"/> if none exists.</returns>
    Task<BackupMetadata?> GetLastSuccessfulExportAsync(string exportType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the metadata describing the outcome of an export run.
    /// </summary>
    /// <param name="metadata">The metadata to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveExportMetadataAsync(BackupMetadata metadata, CancellationToken cancellationToken = default);
}
