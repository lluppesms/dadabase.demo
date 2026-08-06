# Weekly Automated Export Proposal: Cloud-Native Change Detection Pattern

**Document Status**: Design Proposal  
**Model**: Opus  
**Pattern**: Cloud-Native Change Detection  
**Target**: Dadabase Weekly Backup System

---

## APPROACH: Opus Model - Cloud-Native Change Detection Pattern

### Architecture Overview

This proposal implements a **lightweight, cloud-native scheduled backup system** that leverages Azure Functions for weekly execution, uses a **timestamp-based change detection** mechanism with a stored procedure to track the last successful export, and integrates seamlessly with Dadabase's existing repository export methods. The system uploads backups to Azure Blob Storage with automatic retention management (keeping 10 most recent), enabling cost-effective disaster recovery without burdening the web application tier. By storing change metadata in the database itself, the solution achieves stateless change detection across multiple instances while maintaining zero external dependencies beyond Azure storage.

---

## Core Components

### 1. Change Detection Mechanism

**Decision**: Timestamp-based change detection using a stored procedure that tracks the maximum `ChangeDateTime` from the Jokes table.

**Rationale**: 
- Every `Joke` record has `ChangeDateTime` tracking when it was last modified
- A stored procedure query (`usp_GetLastJokeChangeDateTime`) can efficiently return the latest modification timestamp
- Simpler than hash-based approaches; avoids computing expensive full-dataset hashes
- Reuses existing database infrastructure without requiring additional tables
- Allows detecting granular changes (adds, edits, deletes via change tracking if enabled)

**Key Points**:
- Compare current `MAX(ChangeDateTime)` against stored metadata in a new `BackupMetadata` table
- If current MAX > stored timestamp, data has changed → proceed with export
- If equal or older, skip export (no changes) and log as skipped
- Thread-safe: SQL queries are atomic and concurrent-safe

---

### 2. Scheduling Layer

**Decision**: Azure Timer Trigger Function (Time-based Schedule).

**Trigger Mechanism**:
- Azure Functions Timer Trigger using CRON expression: `0 0 ? * MON *` (every Monday at 00:00 UTC)
- Runs weekly without external dependencies
- Automatic retry handling and monitoring via Application Insights
- Stateless: function can run on any Azure Functions instance

**Why This Approach**:
- Azure Functions are already provisioned for Dadabase API
- Timer Triggers are lightweight, cost-effective, and require no scheduler infrastructure
- CRON expressions are industry-standard for scheduling
- Built-in Azure integration: storage, secrets from Key Vault, logging

**Alternative Considered**: Azure Logic Apps (overkill for simple scheduling) or Azure Automation Runbooks (adds operational overhead)

---

### 3. Export Pipeline

**Decision**: Leverage existing `IJokeRepository` export methods; create a new `BackupFormat` enum to select output format.

**Approach**:
- Use `ExportToJson()` as the primary format (largest ecosystem support, human-readable, schema-flexible)
- Backup filename pattern: `backup-dadabase-{timestamp}-{format}.{ext}`
  - Example: `backup-dadabase-20250301-json.json`
  - Timestamp allows easy sorting and identification
  - Format indicates backup type for recovery procedures
- Serialize `BackupData` object (Categories, Jokes, Ratings lists) to JSON
- Include metadata header in the backup file:
  ```json
  {
    "_metadata": {
      "exportedAt": "2025-03-01T00:00:00Z",
      "exportType": "weekly",
      "dataSize": { "jokeCount": 425, "categoryCount": 18, "ratingCount": 8743 },
      "checksum": "sha256:abc123...",
      "version": "1.0"
    },
    "categories": [...],
    "jokes": [...],
    "ratings": [...]
  }
  ```
- Gzip compress before upload to save storage costs (~70% compression typical for JSON)

---

### 4. Blob Storage Integration

**Decision**: Use Azure Blob Storage Container with hierarchical naming and managed identity authentication.

**Connection Approach**:
- Use **Managed Identity** (System-Assigned) for Azure Functions
- No connection strings in config; leverage Azure RBAC `Storage Blob Data Contributor` role
- Container name: `dadabase-backups` (created via Bicep during provisioning)
- Folder structure: `{year}/{month}/{filename}`
  - Example: `2025/03/backup-dadabase-20250301-json.json.gz`
  - Enables efficient date-based queries and retention policies

**Azure SDK Usage**:
```csharp
// Initialize client using Managed Identity
var blobContainerClient = new BlobContainerClient(
    new Uri($"https://{storageAccountName}.blob.core.windows.net/dadabase-backups"),
    new DefaultAzureCredential()
);
```

**Upload Process**:
- Upload with metadata tags for easy filtering:
  - `backup-type: weekly`
  - `export-date: 20250301`
  - `retention-age: 0` (calculated during rotation)
- Set Content-Type: `application/json+gzip`
- Set Content-Encoding: `gzip`

---

### 5. Retention Management

**Decision**: Maintain exactly 10 most recent backups; delete older ones after rotation check.

**Strategy**:
- After each successful upload, list all backups in container
- Sort by creation time (blob properties `CreatedOn`)
- Identify backups to delete (keep 10, delete remainder)
- Delete old backups in reverse creation order
- Log retention actions for audit trail

**Lifecycle Policy Consideration**:
- Azure Storage Lifecycle Management could auto-delete blobs, but manual deletion provides better control and logging
- Example: lifecycle policy for monthly archives (keep backups >30 days in cool tier) separate from weekly hot tier

**Data Retention Logic**:
- Keep 10 weekly backups ≈ ~2.5 months of history
- Oldest backup is ~70 days old
- Automatically delete when 11th backup is created

---

## Pseudocode: Main Export Job Handler

```
function ExecuteWeeklyExport():
  try:
    logger.Info("Weekly export job started")
    
    // Step 1: Initialize dependencies
    jokeRepository = GetJokeRepository()  // DI or factory
    blobClient = CreateBlobContainerClient()
    dbMetadataStore = GetBackupMetadataStore()
    
    // Step 2: Check if data has changed
    if NOT HasDataChanged():
      logger.Info("No changes detected since last export - skipping backup")
      metrics.LogSkippedExport()
      return SUCCESS
    
    // Step 3: Generate backup filename with timestamp
    timestamp = DateTime.UtcNow  // e.g., 20250301_T000000Z
    backupFilename = ConstructBackupFilename(timestamp)  // backup-dadabase-{ts}-json
    
    // Step 4: Export data using existing repository method
    try:
      exportedJsonString = jokeRepository.ExportToJson()  // leverages existing method
      
      // Step 5: Enrich with metadata
      backupWithMetadata = EnrichBackupWithMetadata(
        jsonData: exportedJsonString,
        exportedAt: timestamp,
        dataSource: "JokeRepository.ExportToJson()"
      )
      
      // Step 6: Calculate checksum for integrity verification
      checksum = CalculateSHA256(backupWithMetadata)
      
      // Step 7: Compress backup (gzip)
      compressedBytes = GzipCompress(backupWithMetadata)
      logger.Debug($"Compressed from {Encoding.UTF8.GetByteCount(backupWithMetadata)} to {compressedBytes.Length} bytes")
      
      // Step 8: Upload to Blob Storage with retry logic
      uploadedUri = UploadBlobWithRetry(
        containerClient: blobClient,
        blobName: ConstructBlobPath(timestamp, backupFilename),  // 2025/03/backup-...
        content: compressedBytes,
        metadata: BuildBlobMetadata(timestamp, checksum),
        maxRetries: 3,
        retryDelayMs: 1000
      )
      logger.Info($"Backup uploaded successfully: {uploadedUri}")
      
    catch (RepositoryException ex):
      logger.Error($"Export failed: {ex.Message}", ex)
      metrics.LogExportFailure("RepositoryError")
      return FAILURE
    
    // Step 9: Update change detection metadata in database
    newMaxChangeDateTime = jokeRepository.GetLastJokeChangeDateTime()
    dbMetadataStore.UpdateLastSuccessfulExportTimestamp(
      exportType: "weekly",
      timestamp: timestamp,
      lastDataChangeDateTime: newMaxChangeDateTime,
      backupBlobUri: uploadedUri,
      checksum: checksum
    )
    logger.Info($"Updated backup metadata in database: {timestamp}")
    
    // Step 10: Perform retention cleanup (keep 10 most recent)
    try:
      deletedCount = RotateBackups(blobClient, maxBackups: 10)
      if deletedCount > 0:
        logger.Info($"Retention cleanup: deleted {deletedCount} old backup(s)")
        metrics.LogDeletedBackups(deletedCount)
    
    catch (RetentionException ex):
      logger.Warn($"Retention cleanup failed (backup still exists): {ex.Message}")
      // Don't fail the job; backup exists and is usable
    
    // Step 11: Log success and metrics
    logger.Info("Weekly export job completed successfully")
    metrics.LogExportSuccess(
      exportDurationMs: timer.ElapsedMilliseconds,
      backupSizeBytes: compressedBytes.Length,
      dataCount: ParseDataCounts(backupWithMetadata)
    )
    
    return SUCCESS

  catch (Exception ex):
    logger.Error($"Unexpected error in weekly export: {ex.Message}", ex)
    metrics.LogCriticalFailure("UnexpectedError", ex)
    // Notify administrators via Alert
    SendAlert("Weekly Export Failed", $"Error: {ex.Message}")
    return FAILURE
```

---

## Pseudocode: Change Detection

```
function HasDataChanged():
  try:
    logger.Debug("Checking for data changes...")
    
    // Step 1: Query current maximum change timestamp in Jokes table
    currentMaxChangeDateTime = ExecuteSql(
      "SELECT MAX(ChangeDateTime) FROM dbo.Joke WHERE ActiveInd = 'Y'"
    )
    
    if currentMaxChangeDateTime == null:
      logger.Warn("No jokes found in database - skipping export")
      return false
    
    // Step 2: Retrieve last successful export timestamp from metadata store
    lastExportMetadata = dbMetadataStore.GetLastSuccessfulExport(
      exportType: "weekly"
    )
    
    if lastExportMetadata == null or lastExportMetadata.LastChangeDateTime == null:
      logger.Info("No previous export metadata found - performing initial export")
      return true  // First export, always proceed
    
    lastRecordedChangeDateTime = lastExportMetadata.LastChangeDateTime
    
    // Step 3: Compare timestamps
    hasChanged = currentMaxChangeDateTime > lastRecordedChangeDateTime
    
    logger.Debug($"Last recorded change: {lastRecordedChangeDateTime}")
    logger.Debug($"Current max change: {currentMaxChangeDateTime}")
    logger.Debug($"Data changed: {hasChanged}")
    
    // Step 4: Additional validation - check row count for safety
    if hasChanged:
      currentJokeCount = ExecuteSql("SELECT COUNT(*) FROM dbo.Joke")
      lastExportJokeCount = lastExportMetadata.JokeCount
      
      if currentJokeCount < (lastExportJokeCount * 0.5):
        logger.Warn($"Joke count dropped significantly: {lastExportJokeCount} → {currentJokeCount}")
        // Still proceed but log as anomaly for manual review
        metrics.LogAnomaly("JokeCountDropped", lastExportJokeCount, currentJokeCount)
    
    return hasChanged

  catch (DatabaseException ex):
    logger.Error($"Error querying change detection metadata: {ex.Message}", ex)
    metrics.LogChangeDetectionError(ex)
    // Fail-safe: assume data changed to prevent data loss
    return true
  
  catch (Exception ex):
    logger.Error($"Unexpected error in change detection: {ex.Message}", ex)
    return true  // Conservative: export if unsure
```

---

## Pseudocode: Backup Rotation

```
function RotateBackups(containerClient, maxBackups):
  try:
    logger.Debug($"Starting backup rotation - maintaining {maxBackups} most recent")
    
    // Step 1: List all backup blobs in container
    allBackups = new List<BlobItem>()
    asyncPages = containerClient.GetBlobsAsync(
      prefix: "",  // Get all blobs
      traits: BlobTraits.Metadata
    )
    
    foreach page in asyncPages:
      foreach blob in page.Values:
        // Filter only backup blobs (named pattern: backup-dadabase-*)
        if blob.Name.StartsWith("backup-dadabase-"):
          allBackups.Add(blob)
    
    logger.Debug($"Found {allBackups.Count} total backup blobs")
    
    // Step 2: Sort by creation time descending (newest first)
    sortedBackups = allBackups
      .OrderByDescending(b => b.Properties.CreatedOn)
      .ToList()
    
    // Step 3: Identify backups to delete (keep maxBackups, delete rest)
    backupsToDelete = sortedBackups
      .Skip(maxBackups)  // Keep first N items
      .ToList()
    
    if backupsToDelete.Count == 0:
      logger.Info("All backups within retention policy - no deletion needed")
      return 0
    
    logger.Info($"Backups to delete: {backupsToDelete.Count} (total: {sortedBackups.Count})")
    
    // Step 4: Delete old backups with retry
    deletedCount = 0
    foreach blob in backupsToDelete:
      try:
        logger.Debug($"Deleting old backup: {blob.Name} (created: {blob.Properties.CreatedOn})")
        
        DeleteBlobWithRetry(
          containerClient: containerClient,
          blobName: blob.Name,
          maxRetries: 2,
          retryDelayMs: 500
        )
        
        deletedCount += 1
        logger.Debug($"Successfully deleted: {blob.Name}")
        
      catch (DeleteException ex):
        logger.Warn($"Failed to delete blob {blob.Name}: {ex.Message}")
        metrics.LogDeleteError(blob.Name, ex)
        // Continue with next blob; don't fail entire rotation
    
    // Step 5: Verify deletion and log final state
    logger.Info($"Backup rotation completed: deleted {deletedCount} old backup(s)")
    
    // Log retention state for monitoring
    metrics.LogRetentionState(
      totalBackups: sortedBackups.Count,
      retainedBackups: Math.Min(sortedBackups.Count, maxBackups),
      deletedBackups: deletedCount,
      oldestRetainedDate: sortedBackups
        .Skip(maxBackups - 1)
        .FirstOrDefault()?.Properties.CreatedOn ?? DateTime.UtcNow
    )
    
    return deletedCount

  catch (ContainerException ex):
    logger.Error($"Container operation failed during rotation: {ex.Message}", ex)
    metrics.LogRotationError("ContainerError", ex)
    throw new RetentionException($"Unable to access blob container: {ex.Message}", ex)
  
  catch (Exception ex):
    logger.Error($"Unexpected error during backup rotation: {ex.Message}", ex)
    metrics.LogRotationError("UnexpectedError", ex)
    throw new RetentionException($"Backup rotation failed: {ex.Message}", ex)
```

---

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Scheduled Trigger** | Azure Functions Timer Trigger (CRON: `0 0 ? * MON *`) | Lightweight, serverless, no external scheduler needed; built-in Azure integration; cost-effective for weekly runs |
| **Change Detection** | Timestamp-based (MAX(ChangeDateTime) vs. stored metadata) | Simple, efficient, leverages existing Joke.ChangeDateTime; atomic SQL queries; avoids expensive hash computations |
| **Export Format** | JSON (primary); SQL/TSV/BulletedList as alternative formats | JSON is schema-flexible, widely supported, human-readable; easier to extend for future schema changes; existing ExportToJson() method already optimized |
| **Compression** | Gzip | Standard, widely supported; ~70% compression ratio typical for JSON; built-in .NET support; reduces storage costs and transfer time |
| **Authentication** | Managed Identity (System-Assigned) | No secrets in code; Azure RBAC control; automatic credential rotation; secure by default; aligns with Azure best practices |
| **Blob Storage Structure** | Hierarchical naming: `{year}/{month}/{filename}` | Efficient date-based queries; supports future lifecycle policies; human-readable organization; enables cost optimization (move old backups to cool tier) |
| **Retention Policy** | Manual deletion (keep 10 most recent) | Provides control and logging; allows gradual migration to archive tier; easier to test and validate; avoids silent deletions |
| **Metadata Storage** | New `BackupMetadata` table in Dadabase DB | Single source of truth; integrates with existing database; enables correlation with production changes; no external state store needed |
| **Monitoring** | Application Insights via Azure Functions integration | Native Azure integration; automatic exception tracking; custom metrics for business-level monitoring; cost-effective |

---

## Advantages

1. **Zero Impact on Web Application**: Scheduled export runs independently via Azure Functions; no burden on web tier; existing ExportToJson() method remains unchanged

2. **Efficient Change Detection**: Timestamp-based detection using SQL MAX() avoids expensive full-dataset comparisons; typically detects changes in <100ms; reduces unnecessary exports for read-only weeks

3. **Cost-Optimized Storage**: Gzip compression saves ~70% storage; hierarchical naming enables lifecycle policies (move 60+ day old backups to cool tier); deletion of old backups prevents unbounded growth

4. **Stateless & Scalable**: Leverages database for change metadata; Azure Functions can run on any instance; no single point of failure; supports horizontal scaling if needed

5. **Disaster Recovery Ready**: 10-week rolling backup window (~70 days); JSON format is schema-agnostic (easier recovery if schema changes); metadata checksums enable integrity verification; blob URI stored in database for quick recovery lookup

6. **Auditability & Compliance**: All export attempts logged (success/skip/failure) with timestamps; retention metadata tracked in database; blob metadata includes export date, type, and checksum; supports regulatory requirements

7. **Minimal Dependencies**: Uses only Azure native services (Functions, Blob Storage, Managed Identity); no third-party services; integrates seamlessly with existing Bicep infrastructure

---

## Considerations

1. **Initial Implementation Effort**: Requires creating `BackupMetadata` table, stored procedure `usp_GetLastJokeChangeDateTime`, new Azure Function code, and Bicep updates for managed identity RBAC; estimated 4-5 days development + testing

2. **Database Schema Change**: New `BackupMetadata` table and stored procedures introduce minor schema overhead; requires database migration strategy (backward-compatible); consider migration scripting in DacPac or manual SQL deployment

3. **Testing Complexity**: Change detection logic requires test scenarios: first export (no metadata), data modifications, large updates, concurrent export attempts; consider transactional test setup to reset state between runs

4. **Timestamp Precision**: If Joke.ChangeDateTime has minute-level granularity (not milliseconds), multiple changes within same minute could miss detection; verify database column definition and update metadata comparison logic if needed

5. **Blob Listing Performance**: Listing 10 backups (with retention) is fast; if rotation is kept for 100+ backups, consider pagination; for typical 10-backup retention, negligible impact

6. **Recovery Procedure Documentation**: Backup format JSON includes metadata header; recovery procedures must handle decompression (gzip), metadata extraction, and validation; document recovery steps upfront

7. **Monitoring & Alerting**: Set up Application Insights alerts for: failed exports, skipped exports (anomaly if >4 consecutive weeks), backup size anomalies; consider email/SMS alerts for critical failures

8. **Storage Account Permissions**: Ensure Function's System-Assigned Managed Identity has `Storage Blob Data Contributor` role on storage account; test RBAC assignment during deployment; verify cross-subscription access if storage is in different subscription

---

## Summary

This **Cloud-Native Change Detection Pattern** provides a production-ready blueprint for automating Dadabase weekly exports. By leveraging Azure Functions for scheduling, timestamp-based change detection for efficiency, and managed identity for security, the solution is **cost-effective, auditable, and disaster-recovery ready**. The design integrates seamlessly with existing repository export methods while introducing minimal schema changes and operational overhead.

**Next Steps**:
1. Review and refine pseudocode with team
2. Create `BackupMetadata` table schema
3. Implement `usp_GetLastJokeChangeDateTime` stored procedure
4. Develop Azure Function timer trigger code
5. Create Bicep module for managed identity RBAC
6. Implement comprehensive unit and integration tests
7. Document backup recovery procedures
8. Deploy to staging environment for validation
