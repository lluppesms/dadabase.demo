//-----------------------------------------------------------------------
// <copyright file="BackupExportJob.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Weekly Backup Export Job
// </summary>
//-----------------------------------------------------------------------
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using DadABase.Data.Models;
using DadABase.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace DadABase.Data.Services;

/// <summary>
/// Orchestrates a backup export: change detection, serialization, compression, upload,
/// metadata recording, and retention rotation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BackupExportJob"/> class.
/// </remarks>
/// <param name="exportService">The service used to build the backup payload.</param>
/// <param name="storageService">The service used to store backups in blob storage.</param>
/// <param name="metadataRepository">The repository used to record the outcome of the export.</param>
/// <param name="logger">The logger used to record progress.</param>
public class BackupExportJob(
    IBackupExportService exportService,
    IBackupStorageService storageService,
    IBackupMetadataRepository metadataRepository,
    ILogger<BackupExportJob> logger)
{
    private readonly IBackupExportService _exportService = exportService;
    private readonly IBackupStorageService _storageService = storageService;
    private readonly IBackupMetadataRepository _metadataRepository = metadataRepository;
    private readonly ILogger<BackupExportJob> _logger = logger;

    /// <summary>
    /// Gets or sets the export type recorded for this run.
    /// </summary>
    /// <value>A string, defaulting to "Weekly".</value>
    public string ExportType { get; set; } = BackupConstants.WeeklyExportType;

    /// <summary>
    /// Runs one backup export.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    /// <exception cref="Exception">Rethrows any failure so the caller can report a non-zero exit code.</exception>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Backup export started at {UtcNow}", DateTime.UtcNow);

        try
        {
            // Step 1: Check for changes since the last successful export
            var snapshot = await _exportService.GetLastJokeChangeSnapshotAsync(cancellationToken);
            if (!await HasDataChangedAsync(snapshot, cancellationToken))
            {
                await RecordSkippedExportAsync("NoChanges", cancellationToken);
                return;
            }

            // Step 2: Build the backup data
            _logger.LogInformation("Building backup data...");
            var backupData = await _exportService.BuildBackupDataAsync(cancellationToken);
            if (backupData.Jokes.Count == 0)
            {
                _logger.LogWarning("No jokes found - skipping to avoid creating an empty backup");
                await RecordSkippedExportAsync("EmptyDataset", cancellationToken);
                return;
            }

            // Step 3: Serialize and enrich with metadata
            var backupContent = SerializeBackup(backupData);

            // Step 4: Compute the checksum and compress the payload
            var checksum = ComputeSha256(backupContent);
            var compressedBytes = GzipCompress(backupContent);
            var originalSize = Encoding.UTF8.GetByteCount(backupContent);
            _logger.LogInformation(
                "Backup prepared: {OriginalSize} bytes -> {CompressedSize} bytes ({CompressionRatio:P1} smaller)",
                originalSize,
                compressedBytes.Length,
                originalSize == 0 ? 0 : 1 - ((double)compressedBytes.Length / originalSize));

            // Step 5: Upload to blob storage
            var blobName = BuildBlobName(DateTime.UtcNow);
            _logger.LogInformation("Uploading backup to blob: {BlobName}", blobName);
            var blobUri = await _storageService.UploadBackupAsync(blobName, compressedBytes, checksum, cancellationToken);

            // Step 6: Record the successful export
            await _metadataRepository.SaveExportMetadataAsync(
                new BackupMetadata
                {
                    ExportType = ExportType,
                    LastExportedAtUtc = DateTime.UtcNow,
                    LastExportedMaxChangeDateTimeUtc = snapshot.MaxChangeDateTimeUtc,
                    LastExportedJokeCount = snapshot.JokeCount,
                    BackupBlobUri = blobUri,
                    Checksum = checksum,
                    Status = BackupConstants.StatusSuccess
                },
                cancellationToken);

            // Step 7: Rotate old backups
            _logger.LogInformation("Starting backup rotation...");
            var deletedCount = await RotateBackupsAsync(cancellationToken);

            _logger.LogInformation(
                "Backup export completed: {BlobUri} (deleted {DeletedCount} old backups)",
                blobUri,
                deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Weekly backup export failed");
            await RecordFailedExportAsync(Utilities.GetExceptionMessage(ex), CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// Determines whether the joke data has changed since the last successful export.
    /// </summary>
    /// <param name="currentSnapshot">The current joke data snapshot.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when a new backup should be created; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> HasDataChangedAsync(JokeChangeSnapshot currentSnapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentSnapshot);

        if (currentSnapshot.JokeCount == 0)
        {
            _logger.LogWarning("No jokes found in the database");
            return false;
        }

        var lastMetadata = await _metadataRepository.GetLastSuccessfulExportAsync(ExportType, cancellationToken);
        if (lastMetadata == null)
        {
            _logger.LogInformation("No previous backup metadata found - performing initial export");
            return true;
        }

        if (currentSnapshot.MaxChangeDateTimeUtc > lastMetadata.LastExportedMaxChangeDateTimeUtc)
        {
            _logger.LogInformation(
                "Detected data changes: {PreviousMax} -> {CurrentMax}",
                lastMetadata.LastExportedMaxChangeDateTimeUtc,
                currentSnapshot.MaxChangeDateTimeUtc);
            return true;
        }

        if (currentSnapshot.JokeCount != lastMetadata.LastExportedJokeCount)
        {
            _logger.LogInformation(
                "Detected joke count drift: {PreviousCount} -> {CurrentCount}",
                lastMetadata.LastExportedJokeCount,
                currentSnapshot.JokeCount);
            return true;
        }

        _logger.LogInformation("No data changes detected - skipping backup export");
        return false;
    }

    /// <summary>
    /// Deletes old backups, keeping only the most recent <see cref="BackupConstants.MaxBackupsToKeep"/> files.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The number of backups that were deleted.</returns>
    public async Task<int> RotateBackupsAsync(CancellationToken cancellationToken = default)
    {
        var allBlobs = await _storageService.ListBackupBlobsAsync(BackupConstants.BlobNamePrefix, cancellationToken);
        if (allBlobs.Count <= BackupConstants.MaxBackupsToKeep)
        {
            _logger.LogInformation("Backup count ({Count}) is within the retention limit", allBlobs.Count);
            return 0;
        }

        // Sort newest first, then delete everything past the retention limit
        var sortedBlobs = allBlobs
            .OrderByDescending(b => b.CreatedOn ?? DateTimeOffset.MinValue)
            .ThenByDescending(b => b.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var deletedCount = 0;
        foreach (var blob in sortedBlobs.Skip(BackupConstants.MaxBackupsToKeep))
        {
            try
            {
                _logger.LogDebug("Deleting old backup: {BlobName}", blob.Name);
                if (await _storageService.DeleteBackupAsync(blob.Name, cancellationToken))
                {
                    deletedCount++;
                }
            }
            catch (Exception ex)
            {
                // A failed delete should not fail the backup itself - continue with the next blob
                _logger.LogWarning(ex, "Failed to delete blob {BlobName}", blob.Name);
            }
        }

        _logger.LogInformation(
            "Backup rotation completed: kept {Kept}, deleted {Deleted}",
            BackupConstants.MaxBackupsToKeep,
            deletedCount);

        return deletedCount;
    }

    /// <summary>
    /// Builds the hierarchical blob name ({year}/{month}/dadabase-backup-{timestamp}.json.gz) for a backup.
    /// </summary>
    /// <param name="timestampUtc">The UTC timestamp of the export.</param>
    /// <returns>The blob name.</returns>
    public static string BuildBlobName(DateTime timestampUtc)
    {
        return $"{timestampUtc:yyyy/MM}/{BackupConstants.BlobNamePrefix}{timestampUtc:yyyy-MM-ddTHH-mm-ss}Z.json.gz";
    }

    /// <summary>
    /// Serializes the backup data, adding a metadata header describing the export.
    /// </summary>
    /// <param name="backupData">The backup payload.</param>
    /// <returns>The serialized JSON content.</returns>
    public string SerializeBackup(BackupData backupData)
    {
        ArgumentNullException.ThrowIfNull(backupData);

        var envelope = new
        {
            metadata = new
            {
                exportedAt = DateTime.UtcNow,
                exportType = ExportType,
                dataCount = new
                {
                    jokes = backupData.Jokes.Count,
                    categories = backupData.Categories.Count,
                    ratings = backupData.Ratings.Count
                },
                version = "1.0"
            },
            backupData
        };

        return JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Computes the SHA256 checksum of the specified content.
    /// </summary>
    /// <param name="content">The content to hash.</param>
    /// <returns>The hash as an uppercase hexadecimal string.</returns>
    public static string ComputeSha256(string content)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(content ?? string.Empty));
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Compresses the specified content using gzip.
    /// </summary>
    /// <param name="content">The content to compress.</param>
    /// <returns>The compressed bytes.</returns>
    public static byte[] GzipCompress(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }

    private async Task RecordSkippedExportAsync(string reason, CancellationToken cancellationToken)
    {
        await _metadataRepository.SaveExportMetadataAsync(
            new BackupMetadata
            {
                ExportType = ExportType,
                LastExportedAtUtc = DateTime.UtcNow,
                Status = BackupConstants.StatusSkipped,
                ErrorMessage = reason
            },
            cancellationToken);
    }

    private async Task RecordFailedExportAsync(string errorMessage, CancellationToken cancellationToken)
    {
        try
        {
            await _metadataRepository.SaveExportMetadataAsync(
                new BackupMetadata
                {
                    ExportType = ExportType,
                    LastExportedAtUtc = DateTime.UtcNow,
                    Status = BackupConstants.StatusFailed,
                    ErrorMessage = errorMessage
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Never mask the original failure with a metadata logging failure
            _logger.LogError(ex, "Unable to record the failed backup export metadata");
        }
    }
}
