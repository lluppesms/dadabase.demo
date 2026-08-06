//-----------------------------------------------------------------------
// <copyright file="IBackupExportService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Backup Export Service Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Services;

/// <summary>
/// Builds the data included in a backup export. Shared by manual exports and the scheduled backup job.
/// </summary>
public interface IBackupExportService
{
    /// <summary>
    /// Builds the complete backup payload (jokes, categories, and ratings).
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A <see cref="BackupData"/> instance containing all exported records.</returns>
    Task<BackupData> BuildBackupDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a snapshot of the current joke data used for change detection.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A <see cref="JokeChangeSnapshot"/> describing the current state of the joke data.</returns>
    Task<JokeChangeSnapshot> GetLastJokeChangeSnapshotAsync(CancellationToken cancellationToken = default);
}
