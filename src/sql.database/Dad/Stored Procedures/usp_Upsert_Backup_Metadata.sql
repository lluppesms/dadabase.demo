CREATE PROCEDURE [Dad].[usp_Upsert_Backup_Metadata]
	@ExportType nvarchar(50),
	@LastExportedAtUtc datetime2(3),
	@LastExportedMaxChangeDateTimeUtc datetime2(3) = NULL,
	@LastExportedJokeCount int = 0,
	@BackupBlobUri nvarchar(2048) = NULL,
	@Checksum nvarchar(256) = NULL,
	@Status nvarchar(50),
	@ErrorMessage nvarchar(max) = NULL
AS
/*
Records the outcome (Success / Skipped / Failed) of a backup export run.

Example Usage:
  exec [Dad].[usp_Upsert_Backup_Metadata] @ExportType = 'Weekly', @LastExportedAtUtc = '2026-05-21T03:00:00', @Status = 'Skipped'
*/
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [Dad].[BackupMetadata]
		([ExportType], [LastExportedAtUtc], [LastExportedMaxChangeDateTimeUtc],
		 [LastExportedJokeCount], [BackupBlobUri], [Checksum], [Status],
		 [ErrorMessage], [CreatedAtUtc])
	VALUES
		(@ExportType, @LastExportedAtUtc, @LastExportedMaxChangeDateTimeUtc,
		 @LastExportedJokeCount, @BackupBlobUri, @Checksum, @Status,
		 @ErrorMessage, SYSUTCDATETIME())
END
GO
