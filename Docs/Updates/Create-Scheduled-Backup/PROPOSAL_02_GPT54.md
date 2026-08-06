## APPROACH: GPT-5.4-Codex Model - Hybrid Efficiency Pattern

### Architecture Overview
Use an Azure Functions timer-triggered job as the weekly orchestrator, because the repository already includes an Azure Functions app and Bicep-managed Azure infrastructure. The job should compare the latest `Joke.ChangeDateTime` watermark against a persisted manifest in Blob Storage, and only build/upload a new full backup when joke data has changed. When a backup is created, the function should serialize the canonical `BackupData` payload, upload it to Blob Storage with managed identity, update the manifest, and prune older backups beyond the newest 10 files.

### Core Components
1. **Change Detection Mechanism**
   - Pseudocode for detecting changes in jokes table
   - Hash-based or timestamp-based approach decision
   - **Decision:** use a timestamp-based watermark with optional row-count verification.
   - Rationale:
     - `Joke.ChangeDateTime` already exists in the model, so no expensive full-table hashing is required.
     - Weekly execution does not need row-by-row diffing; it only needs a reliable “anything changed?” signal.
     - Persist `LastExportedMaxChangeDateTimeUtc` and `LastExportedJokeCount` in a small manifest blob for quick comparison.
   - Pseudocode sketch:
     - Query `MAX(ChangeDateTime)` from `Jokes`
     - Query `COUNT(*)` from `Jokes`
     - Read manifest from Blob Storage
     - If no manifest exists, export
     - If current max timestamp is greater than stored watermark, export
     - If timestamp is equal but count differs, export
     - Otherwise skip

2. **Scheduling Layer**
   - How to trigger weekly execution
   - What triggers this process
   - Use an Azure Functions **TimerTrigger** with a weekly CRON schedule such as `0 0 3 * * 0` (Sunday 03:00 UTC).
   - Trigger source:
     - Azure Functions runtime invokes the job automatically each week.
     - The same handler can optionally expose an admin-only HTTP/manual trigger later for on-demand backup testing.
   - Operational flow:
     - Timer fires
     - Function resolves repository/export services through DI
     - Function performs change detection
     - Function either exits cleanly with “no changes” log entry or continues into export/upload/rotation

3. **Export Pipeline**
   - How to leverage existing export functions
   - What format to use for backups
   - Reuse existing export logic by moving shared data-collection logic into a common export service or repository helper.
   - Recommended pattern:
     - Add a shared method such as `BuildBackupData()` / `BuildBackupDataAsync()`
     - Existing `ExportToJson()`, `ExportToSql()`, and `ExportToTabDelimited()` call the shared method or shared query builder
     - Weekly backup job also calls the shared method, then serializes `BackupData` to JSON
   - Backup format:
     - Store **JSON serialized `BackupData`** as the canonical backup artifact
     - Why:
       - matches the current `BackupData` structure with `Jokes`, `Categories`, and `Ratings`
       - easy to restore and inspect
       - smaller implementation surface than generating SQL for automated storage backups
   - Suggested blob naming:
     - `weekly-backups/dadabase-backup-2026-05-21T03-00-00Z.json`

4. **Blob Storage Integration**
   - How to upload to Azure Storage
   - Connection & authentication approach
   - Use `BlobServiceClient` with **managed identity** and `DefaultAzureCredential`.
   - Provision a dedicated container such as `weekly-backups` in Bicep.
   - Authentication pattern:
     - Function app system-assigned managed identity
     - RBAC role: `Storage Blob Data Contributor` on the backup storage account/container
     - App settings contain only storage account/blob endpoint metadata, not secrets
   - Upload steps:
     - Build blob path/name
     - Serialize `BackupData` to UTF-8 JSON
     - Upload with overwrite disabled for immutable backup naming
     - Upload/update `backup-manifest.json` after successful artifact write

5. **Retention Management**
   - How to identify and delete old backups
   - Pseudocode for cleanup logic
   - Retain newest 10 successful backup blobs only.
   - Cleanup algorithm:
     - List blobs with prefix `dadabase-backup-`
     - Sort descending by timestamp encoded in blob name or by blob properties
     - Keep first 10
     - Delete remaining blobs
     - Log each deletion and continue on per-blob failure so one failed delete does not fail the whole backup cycle

### Pseudocode: Main Export Job Handler
```text
function ExecuteWeeklyExport():
  logger.LogInformation("Weekly export job started", utcNow)
  try:
    manifest = manifestStore.LoadOrDefault()
    changed = HasDataChanged()
    if changed == false:
      logger.LogInformation("Skipping weekly export because jokes table has not changed")
      return Success("Skipped - no changes")

    backupData = exportService.BuildBackupData()
    if backupData.Jokes.Count == 0:
      logger.LogWarning("Backup job found zero jokes; skipping upload to avoid empty backup overwrite")
      return Success("Skipped - empty dataset")

    payloadJson = JsonSerialize(backupData, indented = true)
    backupTimestamp = utcNow formatted as "yyyy-MM-ddTHH-mm-ssZ"
    blobName = "dadabase-backup-" + backupTimestamp + ".json"

    containerClient = blobFactory.GetBackupContainerClient()
    uploadResult = containerClient.UploadBlob(blobName, payloadJson, contentType = "application/json")
    logger.LogInformation("Backup uploaded", blobName, uploadResult.ETag)

    currentSnapshot = queryService.GetJokeChangeSnapshot()
    manifestStore.Save(
      lastBackupBlobName = blobName,
      lastExportedAtUtc = utcNow,
      lastExportedMaxChangeDateTimeUtc = currentSnapshot.MaxChangeDateTimeUtc,
      lastExportedJokeCount = currentSnapshot.JokeCount)

    RotateBackups(containerClient, 10)
    logger.LogInformation("Weekly export job completed successfully", blobName)
    return Success(blobName)
  catch exception:
    logger.LogError(exception, "Weekly export job failed")
    telemetry.TrackException(exception)
    return Failure("Backup job failed")
```

### Pseudocode: Change Detection
```text
function HasDataChanged():
  manifest = manifestStore.LoadOrDefault()
  if manifest does not exist:
    logger.LogInformation("No manifest found; initial export required")
    return true

  currentSnapshot = queryService.GetJokeChangeSnapshot()
  if currentSnapshot.JokeCount == 0 and manifest.lastExportedJokeCount == 0:
    return false

  if currentSnapshot.MaxChangeDateTimeUtc > manifest.lastExportedMaxChangeDateTimeUtc:
    logger.LogInformation("Detected newer joke changes", currentSnapshot.MaxChangeDateTimeUtc)
    return true

  if currentSnapshot.MaxChangeDateTimeUtc == manifest.lastExportedMaxChangeDateTimeUtc
     and currentSnapshot.JokeCount != manifest.lastExportedJokeCount:
    logger.LogInformation("Detected joke count drift with same timestamp watermark")
    return true

  logger.LogInformation("No joke changes detected since last successful export")
  return false
```

### Pseudocode: Backup Rotation
```text
function RotateBackups(containerClient, maxBackups):
  logger.LogInformation("Starting backup rotation", maxBackups)
  backupBlobs = containerClient.ListBlobs(prefix = "dadabase-backup-")
  orderedBackups = backupBlobs
    .filter(blob => blob.name endsWith ".json")
    .sortDescending(blob => blob.name)

  if orderedBackups.count <= maxBackups:
    logger.LogInformation("Rotation not needed", orderedBackups.count)
    return

  blobsToDelete = orderedBackups.skip(maxBackups)
  for each blob in blobsToDelete:
    try:
      containerClient.DeleteBlobIfExists(blob.name)
      logger.LogInformation("Deleted old backup", blob.name)
    catch deleteException:
      logger.LogWarning(deleteException, "Failed to delete old backup", blob.name)
      continue

  logger.LogInformation("Backup rotation completed", kept = maxBackups, deleted = blobsToDelete.count)
```

### Technology Decisions
- **Scheduled Trigger**: Azure Functions TimerTrigger running weekly on a UTC CRON schedule
- **Change Detection**: Timestamp-based watermark using `MAX(Joke.ChangeDateTime)` plus `COUNT(*)` persisted in a manifest blob
- **Backup Format**: JSON serialization of the existing `BackupData` model stored in Azure Blob Storage

### Advantages
- Reuses the existing Azure Functions footprint and Bicep-managed hosting model already present in the repo.
- Avoids unnecessary backup generation by using a cheap SQL watermark check before export work begins.
- Preserves all related entities (`Jokes`, `Categories`, `Ratings`) in one restore-friendly artifact using the existing `BackupData` shape.
- Keeps operations safe and low-maintenance with managed identity, structured logging, and automatic retention cleanup.

### Considerations
- Timestamp-based detection assumes `ChangeDateTime` is reliably updated on every joke insert/update path.
- If categories or ratings change without any joke row update, the proposed trigger rule will still skip because the requirement is specifically tied to joke-table changes.
- The cleanest integration may require refactoring shared export/query logic so the scheduled job and existing export methods do not duplicate data-building code.
- Retention cleanup should run only after a successful upload and manifest update, so a failed backup never causes accidental loss of the newest valid artifacts.
