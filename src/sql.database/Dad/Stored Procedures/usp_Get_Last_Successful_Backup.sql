CREATE PROCEDURE [Dad].[usp_Get_Last_Successful_Backup]
	@ExportType nvarchar(50) = 'Weekly'
AS
/*
Returns the most recent successful backup export record for a given export type.

Example Usage:
  exec [Dad].[usp_Get_Last_Successful_Backup] @ExportType = 'Weekly'
*/
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		b.BackupMetadataId,
		b.ExportType,
		b.LastExportedAtUtc,
		b.LastExportedMaxChangeDateTimeUtc,
		b.LastExportedJokeCount,
		b.BackupBlobUri,
		b.Checksum,
		b.Status,
		b.ErrorMessage,
		b.CreatedAtUtc
	FROM [Dad].[BackupMetadata] b
	WHERE b.ExportType = @ExportType
	  AND b.Status = 'Success'
	ORDER BY b.CreatedAtUtc DESC, b.BackupMetadataId DESC
END
GO
