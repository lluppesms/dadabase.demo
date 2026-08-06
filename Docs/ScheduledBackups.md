# Scheduled Backups

## Overview

A triggered Azure WebJob (`BackupExportJob`) exports all joke data to Azure Blob Storage every Sunday at 03:00 UTC.
The job is hosted **inside the existing Dadabase App Service** — no Function App or additional compute resource is
required. It shares the web app's Managed Identity, app settings, and Application Insights instrumentation.

## How It Works

1. **Change detection** — the job calls `Dad.usp_Get_Last_Joke_Change_Snapshot` and compares the current
   `MAX(ChangeDateTime)` and active joke count against the last successful export recorded in `Dad.BackupMetadata`.
   If nothing changed, the run is recorded as `Skipped` and no file is created.
2. **Build backup data** — jokes, categories, and ratings are read through the shared `IBackupExportService`.
   An empty dataset is skipped rather than overwriting good backups with an empty file.
3. **Serialize, checksum, compress** — the payload is serialized with a metadata header (export timestamp, export
   type, record counts, version), hashed with SHA256, and gzip-compressed.
4. **Upload** — the blob is written to `{year}/{month}/dadabase-backup-{timestamp}Z.json.gz` in the backup container
   using Managed Identity. The checksum is stored as blob metadata.
5. **Record metadata** — an audit row (`Success`, `Skipped`, or `Failed`) is inserted into `Dad.BackupMetadata`
   via `Dad.usp_Upsert_Backup_Metadata`.
6. **Rotate** — only the 10 most recent backups are retained; older blobs are deleted. A failed delete is logged
   and does not fail the backup.

## Components

| Component | Location |
|-----------|----------|
| WebJob console app | `src/web/Website.BackupWebJob` |
| Schedule (CRON) | `src/web/Website.BackupWebJob/settings.job` (`0 0 3 * * 0`) |
| Job orchestration | `src/web/Data/Services/BackupExportJob.cs` |
| Data/storage/metadata services | `src/web/Data/Services`, `src/web/Data/Repositories/BackupMetadataRepository.cs` |
| Database objects | `src/sql.database/Dad/Tables/BackupMetadata.sql`, `src/sql.database/Dad/Stored Procedures/usp_*Backup*.sql` |
| Patch script for existing databases | `src/sql.database/Patch/Patch-20260806.sql` |

## Deployment

The WebJob is published into the website's `App_Data/jobs/triggered/BackupExportJob` folder by the
`PublishBackupWebJob` MSBuild target in `DadABase.Web.csproj`, so the existing web app build and deploy pipeline
ships the job automatically. Azure App Service discovers the job and schedules it from `settings.job`.
`AlwaysOn` is already enabled on the App Service (`infra/Bicep/modules/webapp/website.bicep`), which is required
for the triggered WebJob scheduler to stay active.

## Configuration

| Setting | Description | Default |
|---------|-------------|---------|
| `AppSettings:DefaultConnection` | SQL connection string used to read jokes and record metadata | (required) |
| `AppSettings:BlobStorageAccountName` | Storage account holding the backups | (required) |
| `AppSettings:BackupContainerName` | Blob container that holds the backups | `backup-data` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Enables Application Insights logging for the job | (optional) |

The App Service Managed Identity already has Storage Blob Data Contributor rights on the storage account
(`infra/Bicep/modules/iam/roleassignments.bicep`), and the `backup-data` container is created by
`infra/Bicep/modules/storage/storageaccount.bicep`.

## Monitoring

The job logs progress, skips, and failures to Application Insights. Useful queries:

```kusto
traces
| where message contains "Weekly backup export failed"
| summarize FailureCount = count() by bin(timestamp, 7d)
```

```kusto
traces
| where message contains "Backup export completed"
| summarize Backups = count() by bin(timestamp, 7d)
| render timechart
```

The `Dad.BackupMetadata` table provides the same history in SQL:

```sql
SELECT TOP 20 * FROM [Dad].[BackupMetadata] ORDER BY [CreatedAtUtc] DESC;
```

## Restoring From a Backup

1. Find the desired backup in `Dad.BackupMetadata` (or list the blobs in the backup container).
2. Download the `.json.gz` blob and decompress it (`gzip -d`).
3. Verify the content against the `checksum` blob metadata value (SHA256 of the uncompressed JSON).
4. The file contains a `metadata` header and a `backupData` object with `Jokes`, `Categories`, and `Ratings`
   arrays, which can be re-imported using the existing import tooling on the Admin pages.
