# COMBINED OPTIMAL APPROACH: Weekly Export Job - Database-Primary Hybrid Pattern

**Status**: Final Implementation Blueprint  
**Synthesis Date**: 2026-05-21  
**Combining**: Opus (Cloud-Native) + GPT-5.4-Codex (Hybrid Efficiency)  
**Target Effort**: 15 days (3 weeks) development + testing

---

## Overview

This document represents the **optimal synthesis** of two AI-generated architectural proposals. It takes Opus's audit-trail discipline and GPT-5.4's implementation simplicity, creating a production-ready weekly backup system that:

- ✅ Exports jokes data to Azure Blob Storage automatically every Sunday at 03:00 UTC
- ✅ Detects data changes efficiently using timestamp comparison
- ✅ Maintains exactly 10 most recent backups with automatic cleanup
- ✅ Stores metadata in SQL for audit trail and compliance
- ✅ Uses shared service for code reuse across export methods
- ✅ Implements comprehensive error handling and logging

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    Azure Functions (Timer Trigger)              │
│                    Weekly: Sunday 03:00 UTC                      │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  BackupExportFunction.ExecuteWeeklyExportAsync()          │  │
│  │                                                             │  │
│  │  1. Check if data changed (query → DB metadata)           │  │
│  │  2. If changed: Build backup data (shared service)        │  │
│  │  3. Compress & upload to Blob Storage                     │  │
│  │  4. Update DB metadata with export details                │  │
│  │  5. Rotate backups (keep 10, delete old)                  │  │
│  │  6. Log success/skip/failure to App Insights              │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
         ↓                           ↓                        ↓
    ┌─────────┐        ┌──────────────────────┐    ┌──────────────┐
    │   SQL   │        │  Azure Blob Storage  │    │ App Insights │
    │ Database│        │ (dadabase-backups)   │    │  (Logging)   │
    │         │        │                      │    │              │
    │Tables:  │        │  2025/05/            │    │ - Metrics    │
    │─────────│        │  dadabase-backup-    │    │ - Exceptions │
    │Jokes    │        │  20250521T030000Z... │    │ - Custom KPI │
    │Categories       │  .json.gz (Gzip)     │    └──────────────┘
    │Ratings  │        │                      │
    │─────────│        │  10 backups max      │
    │BackupMetadata    │                      │
    │  (NEW)  │        └──────────────────────┘
    └─────────┘
     Stores:
    ✓ LastExportTimestamp
    ✓ MaxChangeDateTimeUtc
    ✓ JokeCount
    ✓ BlobUri + Checksum
    ✓ Status
```

---

## Core Components

### 1. Database Layer

#### BackupMetadata Table
```sql
CREATE TABLE [dbo].[BackupMetadata] (
    [BackupMetadataId] INT IDENTITY(1,1) PRIMARY KEY,
    [ExportType] NVARCHAR(50) NOT NULL,          -- 'Weekly', 'Manual', etc.
    [LastExportedAtUtc] DATETIME2(3) NOT NULL,
    [LastExportedMaxChangeDateTimeUtc] DATETIME2(3) NULL,
    [LastExportedJokeCount] INT NOT NULL,
    [BackupBlobUri] NVARCHAR(2048) NULL,
    [Checksum] NVARCHAR(256) NULL,                -- SHA256 hex string
    [Status] NVARCHAR(50) NOT NULL,               -- 'Success', 'Skipped', 'Failed'
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [CreatedAtUtc] DATETIME2(3) NOT NULL,
    [RowVersion] ROWVERSION NOT NULL              -- For optimistic concurrency
);

CREATE INDEX IX_BackupMetadata_ExportType_CreatedAt 
    ON [dbo].[BackupMetadata]([ExportType], [CreatedAtUtc] DESC);
```

#### Stored Procedure: GetLastJokeChangeSnapshot
```sql
CREATE PROCEDURE [dbo].[usp_GetLastJokeChangeSnapshot]
AS
BEGIN
    SELECT 
        MAX(ChangeDateTime) AS MaxChangeDateTimeUtc,
        COUNT(*) AS JokeCount,
        (SELECT COUNT(DISTINCT JokeCategoryId) FROM Joke) AS CategoryCount
    FROM [dbo].[Joke]
    WHERE ActiveInd = 'Y';
END;
```

#### Stored Procedure: UpsertBackupMetadata
```sql
CREATE PROCEDURE [dbo].[usp_UpsertBackupMetadata]
    @ExportType NVARCHAR(50),
    @LastExportedAtUtc DATETIME2(3),
    @LastExportedMaxChangeDateTimeUtc DATETIME2(3),
    @LastExportedJokeCount INT,
    @BackupBlobUri NVARCHAR(2048),
    @Checksum NVARCHAR(256),
    @Status NVARCHAR(50),
    @ErrorMessage NVARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO [dbo].[BackupMetadata] 
        ([ExportType], [LastExportedAtUtc], [LastExportedMaxChangeDateTimeUtc], 
         [LastExportedJokeCount], [BackupBlobUri], [Checksum], [Status], 
         [ErrorMessage], [CreatedAtUtc])
    VALUES 
        (@ExportType, @LastExportedAtUtc, @LastExportedMaxChangeDateTimeUtc,
         @LastExportedJokeCount, @BackupBlobUri, @Checksum, @Status,
         @ErrorMessage, GETUTCDATE());
END;
```

---

### 2. Service Layer

#### IBackupExportService Interface
```csharp
/// <summary>
/// Service for building backup data from repository
/// Shared by both manual exports and scheduled backup job
/// </summary>
public interface IBackupExportService
{
    /// <summary>
    /// Build complete backup data (Jokes, Categories, Ratings)
    /// </summary>
    Task<BackupData> BuildBackupDataAsync();
    
    /// <summary>
    /// Get current joke table snapshot for change detection
    /// </summary>
    Task<JokeChangeSnapshot> GetLastJokeChangeSnapshotAsync();
}

public class JokeChangeSnapshot
{
    public DateTime MaxChangeDateTimeUtc { get; set; }
    public int JokeCount { get; set; }
    public int CategoryCount { get; set; }
}
```

#### IBackupStorageService Interface
```csharp
/// <summary>
/// Service for blob storage operations
/// </summary>
public interface IBackupStorageService
{
    /// <summary>
    /// Upload backup to Azure Blob Storage
    /// </summary>
    Task<string> UploadBackupAsync(
        string blobName,
        byte[] compressedContent,
        string checksum,
        CancellationToken ct = default);
    
    /// <summary>
    /// List all backup blobs matching pattern
    /// </summary>
    Task<List<BlobItem>> ListBackupBlobsAsync(string prefix, CancellationToken ct = default);
    
    /// <summary>
    /// Delete a specific backup blob
    /// </summary>
    Task<bool> DeleteBackupAsync(string blobName, CancellationToken ct = default);
}
```

#### IBackupMetadataRepository Interface
```csharp
/// <summary>
/// Repository for backup metadata persistence
/// </summary>
public interface IBackupMetadataRepository
{
    /// <summary>
    /// Get the last successful export metadata
    /// </summary>
    Task<BackupMetadata> GetLastSuccessfulExportAsync(string exportType);
    
    /// <summary>
    /// Save export metadata after backup completes
    /// </summary>
    Task SaveExportMetadataAsync(BackupMetadata metadata);
}
```

---

### 3. Azure Function Implementation

#### BackupExportFunction.cs
```csharp
[Functions("WeeklyBackupExportFunction")]
public class BackupExportFunction
{
    private readonly IBackupExportService _exportService;
    private readonly IBackupStorageService _storageService;
    private readonly IBackupMetadataRepository _metadataRepo;
    private readonly ILogger _logger;
    
    public BackupExportFunction(
        IBackupExportService exportService,
        IBackupStorageService storageService,
        IBackupMetadataRepository metadataRepo,
        ILoggerFactory loggerFactory)
    {
        _exportService = exportService;
        _storageService = storageService;
        _metadataRepo = metadataRepo;
        _logger = loggerFactory.CreateLogger<BackupExportFunction>();
    }
    
    /// <summary>
    /// Timer trigger: Every Sunday at 03:00 UTC
    /// CRON: "0 0 3 * * 0"
    /// </summary>
    [TimerTrigger("0 0 3 * * 0")] TimerInfo timer)
    public async Task Run(TimerInfo timer)
    {
        _logger.LogInformation("Weekly backup export started at {UtcNow}", DateTime.UtcNow);
        
        try
        {
            // Step 1: Check for changes
            var hasChanged = await HasDataChangedAsync();
            if (!hasChanged)
            {
                _logger.LogInformation("No data changes detected - skipping backup export");
                await RecordSkippedExportAsync();
                return;
            }
            
            // Step 2: Build backup data
            _logger.LogInformation("Building backup data...");
            var backupData = await _exportService.BuildBackupDataAsync();
            
            if (backupData.Jokes.Count == 0)
            {
                _logger.LogWarning("No jokes found - skipping to avoid empty backup");
                await RecordSkippedExportAsync("EmptyDataset");
                return;
            }
            
            // Step 3: Serialize and enrich with metadata
            var metadata = new
            {
                exportedAt = DateTime.UtcNow,
                exportType = "weekly",
                dataCount = new
                {
                    jokes = backupData.Jokes.Count,
                    categories = backupData.Categories.Count,
                    ratings = backupData.Ratings.Count
                },
                version = "1.0"
            };
            
            var enrichedBackup = JsonSerializer.Serialize(
                new { _metadata = metadata, backupData }, 
                new JsonSerializerOptions { WriteIndented = true });
            
            // Step 4: Compute checksum and compress
            var checksum = ComputeSHA256(enrichedBackup);
            var compressedBytes = GzipCompress(enrichedBackup);
            
            _logger.LogInformation(
                "Backup prepared: {OriginalSize} bytes → {CompressedSize} bytes ({CompressionRatio:P1})",
                Encoding.UTF8.GetByteCount(enrichedBackup),
                compressedBytes.Length,
                (1 - (double)compressedBytes.Length / Encoding.UTF8.GetByteCount(enrichedBackup)));
            
            // Step 5: Upload to blob storage
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ");
            var blobName = $"{DateTime.UtcNow:yyyy/MM}/dadabase-backup-{timestamp}.json.gz";
            
            _logger.LogInformation("Uploading backup to blob: {BlobName}", blobName);
            var blobUri = await _storageService.UploadBackupAsync(
                blobName, 
                compressedBytes, 
                checksum);
            
            // Step 6: Update metadata and record success
            var snapshot = await _exportService.GetLastJokeChangeSnapshotAsync();
            await _metadataRepo.SaveExportMetadataAsync(new BackupMetadata
            {
                ExportType = "Weekly",
                LastExportedAtUtc = DateTime.UtcNow,
                LastExportedMaxChangeDateTimeUtc = snapshot.MaxChangeDateTimeUtc,
                LastExportedJokeCount = snapshot.JokeCount,
                BackupBlobUri = blobUri,
                Checksum = checksum,
                Status = "Success"
            });
            
            // Step 7: Rotate old backups
            _logger.LogInformation("Starting backup rotation...");
            var deletedCount = await RotateBackupsAsync();
            
            _logger.LogInformation(
                "Weekly backup export completed: {BlobUri} (deleted {DeletedCount} old backups)",
                blobUri, deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Weekly backup export failed");
            await RecordFailedExportAsync(ex.Message);
            throw;  // Rethrow for Azure Functions error handling
        }
    }
    
    /// <summary>
    /// Determine if jokes table has changed since last export
    /// </summary>
    private async Task<bool> HasDataChangedAsync()
    {
        var currentSnapshot = await _exportService.GetLastJokeChangeSnapshotAsync();
        
        if (currentSnapshot.JokeCount == 0)
        {
            _logger.LogWarning("No jokes found in database");
            return false;
        }
        
        var lastMetadata = await _metadataRepo.GetLastSuccessfulExportAsync("Weekly");
        
        if (lastMetadata == null)
        {
            _logger.LogInformation("No previous backup metadata found - performing initial export");
            return true;
        }
        
        // Compare timestamps
        if (currentSnapshot.MaxChangeDateTimeUtc > lastMetadata.LastExportedMaxChangeDateTimeUtc)
        {
            _logger.LogInformation(
                "Detected data changes: {PreviousMax} → {CurrentMax}",
                lastMetadata.LastExportedMaxChangeDateTimeUtc,
                currentSnapshot.MaxChangeDateTimeUtc);
            return true;
        }
        
        // Compare row counts (detect drift)
        if (currentSnapshot.MaxChangeDateTimeUtc == lastMetadata.LastExportedMaxChangeDateTimeUtc
            && currentSnapshot.JokeCount != lastMetadata.LastExportedJokeCount)
        {
            _logger.LogInformation(
                "Detected joke count drift: {PreviousCount} → {CurrentCount}",
                lastMetadata.LastExportedJokeCount,
                currentSnapshot.JokeCount);
            return true;
        }
        
        _logger.LogInformation("No data changes detected");
        return false;
    }
    
    /// <summary>
    /// Delete old backups, keeping only the 10 most recent
    /// </summary>
    private async Task<int> RotateBackupsAsync()
    {
        const int maxBackups = 10;
        
        try
        {
            var allBlobs = await _storageService.ListBackupBlobsAsync("dadabase-backup-");
            
            if (allBlobs.Count <= maxBackups)
            {
                _logger.LogInformation("Backup count ({Count}) within retention limit", allBlobs.Count);
                return 0;
            }
            
            // Sort by creation time descending (newest first)
            var sortedBlobs = allBlobs
                .OrderByDescending(b => b.Properties.CreatedOn)
                .ToList();
            
            // Identify blobs to delete
            var blobsToDelete = sortedBlobs.Skip(maxBackups).ToList();
            
            int deletedCount = 0;
            foreach (var blob in blobsToDelete)
            {
                try
                {
                    _logger.LogDebug("Deleting old backup: {BlobName}", blob.Name);
                    var success = await _storageService.DeleteBackupAsync(blob.Name);
                    if (success) deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete blob {BlobName}", blob.Name);
                    // Continue with next blob
                }
            }
            
            _logger.LogInformation(
                "Backup rotation completed: kept {Kept}, deleted {Deleted}, oldest retained {OldestDate}",
                maxBackups,
                deletedCount,
                sortedBlobs.Skip(maxBackups - 1).FirstOrDefault()?.Properties.CreatedOn);
            
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup rotation failed");
            throw;
        }
    }
    
    private async Task RecordSkippedExportAsync(string reason = "NoChanges")
    {
        await _metadataRepo.SaveExportMetadataAsync(new BackupMetadata
        {
            ExportType = "Weekly",
            LastExportedAtUtc = DateTime.UtcNow,
            Status = "Skipped",
            ErrorMessage = reason
        });
    }
    
    private async Task RecordFailedExportAsync(string errorMessage)
    {
        await _metadataRepo.SaveExportMetadataAsync(new BackupMetadata
        {
            ExportType = "Weekly",
            LastExportedAtUtc = DateTime.UtcNow,
            Status = "Failed",
            ErrorMessage = errorMessage
        });
    }
    
    private static string ComputeSHA256(string content)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Convert.ToHexString(hashBytes);
        }
    }
    
    private static byte[] GzipCompress(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        using (var output = new MemoryStream())
        {
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }
            return output.ToArray();
        }
    }
}
```

---

## Deployment & Infrastructure

### Bicep Module: Managed Identity RBAC

```bicep
// modules/function-backup-identity.bicep
param functionAppName string
param storageAccountName string
param backupContainerName string

// Get existing resources
resource functionApp 'Microsoft.Web/sites@2023-01-01' existing = {
  name: functionAppName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

// Get system-assigned identity
var functionIdentityId = functionApp.identity.principalId

// Assign Storage Blob Data Contributor role
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2023-04-01-preview' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionIdentityId, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'ba92f5b4-2d11-453d-a403-e96b0029c9fe'  // Storage Blob Data Contributor
    )
    principalId: functionIdentityId
    principalType: 'ServicePrincipal'
  }
}

// Create backup container if not exists
resource backupContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccountName}/default/${backupContainerName}'
  properties: {
    publicAccess: 'None'
  }
}

output storageAccountId string = storageAccount.id
output containerName string = backupContainer.name
```

---

## Monitoring & Alerting

### Application Insights Queries

**Query 1: Backup Job Success Rate**
```kusto
customMetrics
| where name == "BackupExportSuccess"
| summarize SuccessCount = count() by bin(timestamp, 1w)
| render timechart
```

**Query 2: Failed Backups Alert**
```kusto
traces
| where message contains "Weekly backup export failed"
| summarize FailureCount = count() by timestamp
| where FailureCount > 0
```

**Query 3: Change Detection Anomalies**
```kusto
customMetrics
| where name == "JokeCountDropped"
| project timestamp, tostring(customDimensions.PreviousCount), tostring(customDimensions.CurrentCount)
```

### Alert Rules

1. **Failed Export Alert**
   - Trigger: 1 failed backup in last 7 days
   - Action: Email team + Slack notification

2. **Consecutive Skipped Exports Alert**
   - Trigger: 5+ consecutive weeks skipped
   - Action: Email team (possible data freeze investigation needed)

3. **Storage Capacity Alert**
   - Trigger: >80% of backup container quota used
   - Action: Email team (may need to increase retention or archive old backups)

---

## Testing Strategy

### Unit Tests

```csharp
[TestClass]
public class BackupExportFunctionTests
{
    [TestMethod]
    public async Task HasDataChangedAsync_WhenNoMetadata_ReturnsTrue()
    {
        // First export scenario
    }
    
    [TestMethod]
    public async Task HasDataChangedAsync_WhenTimestampUnchanged_ReturnsFalse()
    {
        // Skipped export scenario
    }
    
    [TestMethod]
    public async Task HasDataChangedAsync_WhenTimestampNewer_ReturnsTrue()
    {
        // Data modified scenario
    }
    
    [TestMethod]
    public async Task RotateBackupsAsync_KeepsOnlyTenNewest()
    {
        // Retention cleanup scenario
    }
}
```

### Integration Tests

- Test with Azure Storage Emulator (Azurite)
- Test with in-memory database
- Verify end-to-end: Database → Backup Data → Blob Upload → Metadata Record → Rotation

---

## Recovery Procedures

### Restore from Backup

```csharp
public async Task RestoreFromBackupAsync(string blobName)
{
    // 1. Download blob from storage
    var backupBytes = await _storageService.DownloadBackupAsync(blobName);
    
    // 2. Decompress gzip
    var jsonContent = GzipDecompress(backupBytes);
    
    // 3. Extract metadata and verify checksum
    var backup = JsonSerializer.Deserialize<BackupContainer>(jsonContent);
    ValidateChecksum(backup._metadata.checksum, jsonContent);
    
    // 4. Import via existing import pipeline
    await _importService.ImportBackupDataAsync(backup.BackupData);
    
    _logger.LogInformation("Restore from {BlobName} completed successfully", blobName);
}
```

---

## Success Metrics

| Metric | Target | Monitoring |
|--------|--------|-----------|
| **Weekly Export Completion** | 100% (52/52 weeks) | Application Insights |
| **Change Detection Accuracy** | 100% (no false negatives) | Manual weekly review |
| **Backup Rotation Reliability** | 10 backups maintained | Blob count query |
| **Export-to-Upload Time** | <30 seconds | App Insights duration |
| **Storage Compression Ratio** | ≥70% reduction | Blob size metrics |
| **Failed Export Recovery Time** | <1 hour | SLA target |

---

## Next Steps

1. **Week 1**: Database schema migration + service interfaces
2. **Week 2**: Azure Function implementation + Bicep infrastructure
3. **Week 3**: Testing, monitoring setup, documentation
4. **Deployment**: Staging validation, then production rollout

