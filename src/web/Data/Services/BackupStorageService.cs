//-----------------------------------------------------------------------
// <copyright file="BackupStorageService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Backup Storage Service
// </summary>
//-----------------------------------------------------------------------
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DadABase.Data.Models;
using Microsoft.Extensions.Configuration;

namespace DadABase.Data.Services;

/// <summary>
/// Stores backup files in an Azure Blob Storage container using Managed Identity authentication.
/// </summary>
public class BackupStorageService : IBackupStorageService
{
    private readonly BlobContainerClient _containerClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupStorageService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration containing the storage account and container names.</param>
    /// <exception cref="InvalidOperationException">Thrown when the blob storage account name is not configured.</exception>
    public BackupStorageService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var storageAccountName = configuration["AppSettings:BlobStorageAccountName"];
        if (string.IsNullOrWhiteSpace(storageAccountName))
        {
            throw new InvalidOperationException("AppSettings:BlobStorageAccountName is not configured - unable to store backups.");
        }

        var containerName = configuration["AppSettings:BackupContainerName"];
        if (string.IsNullOrWhiteSpace(containerName))
        {
            containerName = BackupConstants.DefaultContainerName;
        }

        var blobServiceClient = new BlobServiceClient(
            new Uri($"https://{storageAccountName}.blob.core.windows.net"),
            Utilities.GetCredentials());
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    /// <summary>
    /// Uploads a compressed backup to blob storage.
    /// </summary>
    /// <param name="blobName">The name (path) of the blob to create.</param>
    /// <param name="compressedContent">The gzip-compressed backup content.</param>
    /// <param name="checksum">The SHA256 checksum of the uncompressed content, stored as blob metadata.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The absolute URI of the uploaded blob.</returns>
    public async Task<string> UploadBackupAsync(string blobName, byte[] compressedContent, string checksum, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
        ArgumentNullException.ThrowIfNull(compressedContent);

        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobClient = _containerClient.GetBlobClient(blobName);
        using var stream = new MemoryStream(compressedContent, writable: false);
        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/json",
                ContentEncoding = "gzip"
            },
            Metadata = new Dictionary<string, string>
            {
                ["checksum"] = checksum ?? string.Empty,
                ["checksumAlgorithm"] = "SHA256",
                ["exportedAtUtc"] = DateTime.UtcNow.ToString("O")
            }
        };

        await blobClient.UploadAsync(stream, options, cancellationToken);
        return blobClient.Uri.ToString();
    }

    /// <summary>
    /// Lists all backup blobs in the container whose file name starts with the specified prefix.
    /// </summary>
    /// <param name="fileNamePrefix">The backup file name prefix, for example "dadabase-backup-".</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A list of <see cref="BackupBlobInfo"/> records describing the matching blobs.</returns>
    public async Task<List<BackupBlobInfo>> ListBackupBlobsAsync(string fileNamePrefix, CancellationToken cancellationToken = default)
    {
        var blobs = new List<BackupBlobInfo>();

        if (!await _containerClient.ExistsAsync(cancellationToken))
        {
            return blobs;
        }

        await foreach (var blob in _containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix: null, cancellationToken))
        {
            var fileName = blob.Name.Contains('/') ? blob.Name[(blob.Name.LastIndexOf('/') + 1)..] : blob.Name;
            if (string.IsNullOrEmpty(fileNamePrefix) || fileName.StartsWith(fileNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                blobs.Add(new BackupBlobInfo(blob.Name, blob.Properties?.CreatedOn));
            }
        }

        return blobs;
    }

    /// <summary>
    /// Deletes a specific backup blob.
    /// </summary>
    /// <param name="blobName">The name (path) of the blob to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the blob was deleted; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DeleteBackupAsync(string blobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        Response<bool> response = await _containerClient
            .GetBlobClient(blobName)
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

        return response.Value;
    }
}
