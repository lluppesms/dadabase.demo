//-----------------------------------------------------------------------
// <copyright file="IBackupStorageService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Backup Storage Service Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Services;

/// <summary>
/// Stores and manages backup files in Azure Blob Storage.
/// </summary>
public interface IBackupStorageService
{
    /// <summary>
    /// Uploads a compressed backup to blob storage.
    /// </summary>
    /// <param name="blobName">The name (path) of the blob to create.</param>
    /// <param name="compressedContent">The gzip-compressed backup content.</param>
    /// <param name="checksum">The SHA256 checksum of the uncompressed content, stored as blob metadata.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The absolute URI of the uploaded blob.</returns>
    Task<string> UploadBackupAsync(string blobName, byte[] compressedContent, string checksum, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all backup blobs in the container whose file name starts with the specified prefix.
    /// </summary>
    /// <param name="fileNamePrefix">The backup file name prefix, for example "dadabase-backup-".</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A list of <see cref="BackupBlobInfo"/> records describing the matching blobs.</returns>
    Task<List<BackupBlobInfo>> ListBackupBlobsAsync(string fileNamePrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific backup blob.
    /// </summary>
    /// <param name="blobName">The name (path) of the blob to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the blob was deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteBackupAsync(string blobName, CancellationToken cancellationToken = default);
}
