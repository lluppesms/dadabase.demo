//-----------------------------------------------------------------------
// <copyright file="BackupExportJob_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Backup Export Job Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

using System.IO.Compression;
using System.Text;
using DadABase.Data.Models;
using DadABase.Data.Repositories;
using DadABase.Data.Services;
using Microsoft.Extensions.Logging.Abstractions;

[ExcludeFromCodeCoverage]
public class BackupExportJob_Tests
{
    private readonly Mock<IBackupExportService> exportService = new();
    private readonly Mock<IBackupStorageService> storageService = new();
    private readonly Mock<IBackupMetadataRepository> metadataRepository = new();

    private BackupExportJob CreateJob()
    {
        return new BackupExportJob(
            exportService.Object,
            storageService.Object,
            metadataRepository.Object,
            NullLogger<BackupExportJob>.Instance);
    }

    private static BackupData SampleBackupData()
    {
        return new BackupData
        {
            Jokes = [new Joke("Why did the chicken cross the road?")],
            Categories = [new JokeCategory(1, "Animals")],
            Ratings = []
        };
    }

    [Fact]
    public async Task HasDataChanged_WhenNoPreviousMetadata_ReturnsTrue()
    {
        metadataRepository
            .Setup(r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BackupMetadata)null);

        var snapshot = new JokeChangeSnapshot { MaxChangeDateTimeUtc = new DateTime(2026, 5, 1), JokeCount = 25 };

        Assert.True(await CreateJob().HasDataChangedAsync(snapshot, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HasDataChanged_WhenNothingChanged_ReturnsFalse()
    {
        var lastRun = new DateTime(2026, 5, 1);
        metadataRepository
            .Setup(r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackupMetadata { LastExportedMaxChangeDateTimeUtc = lastRun, LastExportedJokeCount = 25 });

        var snapshot = new JokeChangeSnapshot { MaxChangeDateTimeUtc = lastRun, JokeCount = 25 };

        Assert.False(await CreateJob().HasDataChangedAsync(snapshot, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HasDataChanged_WhenTimestampIsNewer_ReturnsTrue()
    {
        metadataRepository
            .Setup(r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackupMetadata
            {
                LastExportedMaxChangeDateTimeUtc = new DateTime(2026, 5, 1),
                LastExportedJokeCount = 25
            });

        var snapshot = new JokeChangeSnapshot { MaxChangeDateTimeUtc = new DateTime(2026, 5, 8), JokeCount = 25 };

        Assert.True(await CreateJob().HasDataChangedAsync(snapshot, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HasDataChanged_WhenJokeCountDrifts_ReturnsTrue()
    {
        var lastRun = new DateTime(2026, 5, 1);
        metadataRepository
            .Setup(r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackupMetadata { LastExportedMaxChangeDateTimeUtc = lastRun, LastExportedJokeCount = 25 });

        var snapshot = new JokeChangeSnapshot { MaxChangeDateTimeUtc = lastRun, JokeCount = 24 };

        Assert.True(await CreateJob().HasDataChangedAsync(snapshot, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HasDataChanged_WhenNoJokesExist_ReturnsFalse()
    {
        var snapshot = new JokeChangeSnapshot { MaxChangeDateTimeUtc = null, JokeCount = 0 };

        Assert.False(await CreateJob().HasDataChangedAsync(snapshot, TestContext.Current.CancellationToken));
        metadataRepository.Verify(
            r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RotateBackups_KeepsOnlyTenNewestBackups()
    {
        var blobs = Enumerable.Range(1, 14)
            .Select(i => new BackupBlobInfo(
                $"2026/05/{BackupConstants.BlobNamePrefix}{i:00}.json.gz",
                new DateTimeOffset(2026, 5, i, 3, 0, 0, TimeSpan.Zero)))
            .ToList();

        storageService
            .Setup(s => s.ListBackupBlobsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobs);
        storageService
            .Setup(s => s.DeleteBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var deletedCount = await CreateJob().RotateBackupsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(4, deletedCount);

        // The four oldest backups are removed and the ten newest are retained
        foreach (var oldest in new[] { "01", "02", "03", "04" })
        {
            storageService.Verify(
                s => s.DeleteBackupAsync($"2026/05/{BackupConstants.BlobNamePrefix}{oldest}.json.gz", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        storageService.Verify(
            s => s.DeleteBackupAsync($"2026/05/{BackupConstants.BlobNamePrefix}05.json.gz", It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RotateBackups_WhenWithinRetentionLimit_DeletesNothing()
    {
        var blobs = Enumerable.Range(1, BackupConstants.MaxBackupsToKeep)
            .Select(i => new BackupBlobInfo($"{BackupConstants.BlobNamePrefix}{i:00}.json.gz", DateTimeOffset.UtcNow.AddDays(-i)))
            .ToList();

        storageService
            .Setup(s => s.ListBackupBlobsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobs);

        Assert.Equal(0, await CreateJob().RotateBackupsAsync(TestContext.Current.CancellationToken));
        storageService.Verify(s => s.DeleteBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RotateBackups_WhenDeleteFails_ContinuesWithOtherBackups()
    {
        var blobs = Enumerable.Range(1, 12)
            .Select(i => new BackupBlobInfo(
                $"{BackupConstants.BlobNamePrefix}{i:00}.json.gz",
                new DateTimeOffset(2026, 5, i, 3, 0, 0, TimeSpan.Zero)))
            .ToList();

        storageService
            .Setup(s => s.ListBackupBlobsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobs);
        storageService
            .Setup(s => s.DeleteBackupAsync($"{BackupConstants.BlobNamePrefix}01.json.gz", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("blob is leased"));
        storageService
            .Setup(s => s.DeleteBackupAsync($"{BackupConstants.BlobNamePrefix}02.json.gz", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Assert.Equal(1, await CreateJob().RotateBackupsAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Run_WhenDataUnchanged_SkipsExportAndRecordsSkippedStatus()
    {
        var lastRun = new DateTime(2026, 5, 1);
        exportService
            .Setup(s => s.GetLastJokeChangeSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JokeChangeSnapshot { MaxChangeDateTimeUtc = lastRun, JokeCount = 25 });
        metadataRepository
            .Setup(r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackupMetadata { LastExportedMaxChangeDateTimeUtc = lastRun, LastExportedJokeCount = 25 });

        await CreateJob().RunAsync(TestContext.Current.CancellationToken);

        storageService.Verify(
            s => s.UploadBackupAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        metadataRepository.Verify(
            r => r.SaveExportMetadataAsync(
                It.Is<BackupMetadata>(m => m.Status == BackupConstants.StatusSkipped && m.ErrorMessage == "NoChanges"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenDataChanged_UploadsCompressedBackupAndRecordsSuccess()
    {
        var snapshot = new JokeChangeSnapshot { MaxChangeDateTimeUtc = new DateTime(2026, 5, 8), JokeCount = 1 };
        exportService.Setup(s => s.GetLastJokeChangeSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(snapshot);
        exportService.Setup(s => s.BuildBackupDataAsync(It.IsAny<CancellationToken>())).ReturnsAsync(SampleBackupData());
        metadataRepository
            .Setup(r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BackupMetadata)null);
        storageService
            .Setup(s => s.ListBackupBlobsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        byte[] uploadedBytes = null;
        string uploadedName = null;
        storageService
            .Setup(s => s.UploadBackupAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], string, CancellationToken>((name, bytes, checksum, token) =>
            {
                uploadedName = name;
                uploadedBytes = bytes;
            })
            .ReturnsAsync("https://storage.blob.core.windows.net/backup-data/test.json.gz");

        await CreateJob().RunAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(uploadedName);
        Assert.EndsWith(".json.gz", uploadedName);
        Assert.Contains(BackupConstants.BlobNamePrefix, uploadedName);

        Assert.NotNull(uploadedBytes);
        var json = Decompress(uploadedBytes);
        Assert.Contains("Why did the chicken cross the road?", json);
        Assert.Contains("\"exportType\": \"Weekly\"", json);

        metadataRepository.Verify(
            r => r.SaveExportMetadataAsync(
                It.Is<BackupMetadata>(m =>
                    m.Status == BackupConstants.StatusSuccess
                    && m.LastExportedJokeCount == 1
                    && m.LastExportedMaxChangeDateTimeUtc == snapshot.MaxChangeDateTimeUtc
                    && !string.IsNullOrEmpty(m.Checksum)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenNoJokesReturned_SkipsExportWithEmptyDatasetReason()
    {
        exportService
            .Setup(s => s.GetLastJokeChangeSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JokeChangeSnapshot { MaxChangeDateTimeUtc = DateTime.UtcNow, JokeCount = 5 });
        exportService.Setup(s => s.BuildBackupDataAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new BackupData());
        metadataRepository
            .Setup(r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BackupMetadata)null);

        await CreateJob().RunAsync(TestContext.Current.CancellationToken);

        storageService.Verify(
            s => s.UploadBackupAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        metadataRepository.Verify(
            r => r.SaveExportMetadataAsync(
                It.Is<BackupMetadata>(m => m.Status == BackupConstants.StatusSkipped && m.ErrorMessage == "EmptyDataset"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenUploadFails_RecordsFailedStatusAndRethrows()
    {
        exportService
            .Setup(s => s.GetLastJokeChangeSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JokeChangeSnapshot { MaxChangeDateTimeUtc = DateTime.UtcNow, JokeCount = 1 });
        exportService.Setup(s => s.BuildBackupDataAsync(It.IsAny<CancellationToken>())).ReturnsAsync(SampleBackupData());
        metadataRepository
            .Setup(r => r.GetLastSuccessfulExportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BackupMetadata)null);
        storageService
            .Setup(s => s.UploadBackupAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage is unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateJob().RunAsync(TestContext.Current.CancellationToken));

        metadataRepository.Verify(
            r => r.SaveExportMetadataAsync(
                It.Is<BackupMetadata>(m => m.Status == BackupConstants.StatusFailed),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void BuildBlobName_UsesHierarchicalYearMonthFolders()
    {
        var blobName = BackupExportJob.BuildBlobName(new DateTime(2026, 5, 21, 3, 0, 0, DateTimeKind.Utc));

        Assert.Equal($"2026/05/{BackupConstants.BlobNamePrefix}2026-05-21T03-00-00Z.json.gz", blobName);
    }

    [Fact]
    public void ComputeSha256_IsStableAndChangesWithContent()
    {
        var hashOne = BackupExportJob.ComputeSha256("dad joke");
        var hashTwo = BackupExportJob.ComputeSha256("dad joke");
        var hashThree = BackupExportJob.ComputeSha256("dad jokes");

        Assert.Equal(hashOne, hashTwo);
        Assert.NotEqual(hashOne, hashThree);
        Assert.Equal(64, hashOne.Length);
    }

    [Fact]
    public void GzipCompress_RoundTripsContent()
    {
        var content = string.Concat(Enumerable.Repeat("Why do dads tell dad jokes? ", 100));

        var compressed = BackupExportJob.GzipCompress(content);

        Assert.True(compressed.Length < Encoding.UTF8.GetByteCount(content));
        Assert.Equal(content, Decompress(compressed));
    }

    private static string Decompress(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
