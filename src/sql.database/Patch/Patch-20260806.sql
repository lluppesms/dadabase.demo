-- =============================================
-- Patch Script: Scheduled Backup Support
-- Date: 2026-08-06
-- Description: Adds the BackupMetadata audit table and the stored procedures used by the
--              weekly backup export WebJob (change detection and metadata recording).
--              Safe to run repeatedly.
-- =============================================

PRINT 'Starting scheduled backup support patch...'
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Dad].[BackupMetadata]') AND type in (N'U'))
BEGIN
    PRINT 'Creating BackupMetadata table...'

    CREATE TABLE [Dad].[BackupMetadata](
        [BackupMetadataId] [int] IDENTITY(1,1) NOT NULL,
        [ExportType] [nvarchar](50) NOT NULL,
        [LastExportedAtUtc] [datetime2](3) NOT NULL,
        [LastExportedMaxChangeDateTimeUtc] [datetime2](3) NULL,
        [LastExportedJokeCount] [int] NOT NULL CONSTRAINT [DF_BackupMetadata_LastExportedJokeCount] DEFAULT ((0)),
        [BackupBlobUri] [nvarchar](2048) NULL,
        [Checksum] [nvarchar](256) NULL,
        [Status] [nvarchar](50) NOT NULL,
        [ErrorMessage] [nvarchar](max) NULL,
        [CreatedAtUtc] [datetime2](3) NOT NULL CONSTRAINT [DF_BackupMetadata_CreatedAtUtc] DEFAULT (sysutcdatetime()),
     CONSTRAINT [PK_BackupMetadata] PRIMARY KEY CLUSTERED ([BackupMetadataId] ASC)
    )

    ALTER TABLE [Dad].[BackupMetadata] ADD CONSTRAINT [CK_BackupMetadata_Status] CHECK ([Status] IN ('Success', 'Skipped', 'Failed'))

    CREATE INDEX [IX_BackupMetadata_ExportType_CreatedAt]
        ON [Dad].[BackupMetadata]([ExportType] ASC, [CreatedAtUtc] DESC)

    PRINT 'BackupMetadata table created.'
END
ELSE
BEGIN
    PRINT 'BackupMetadata table already exists - skipping.'
END
GO

PRINT 'Creating/updating backup stored procedures...'
GO

CREATE OR ALTER PROCEDURE [Dad].[usp_Get_Last_Joke_Change_Snapshot]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MAX(j.ChangeDateTime) AS MaxChangeDateTimeUtc,
        COUNT(*) AS JokeCount,
        (SELECT COUNT(DISTINCT jjc.JokeCategoryId) FROM [Dad].[JokeJokeCategory] jjc) AS CategoryCount
    FROM [Dad].[Joke] j
    WHERE j.ActiveInd = 'Y'
END
GO

CREATE OR ALTER PROCEDURE [Dad].[usp_Get_Last_Successful_Backup]
    @ExportType nvarchar(50) = 'Weekly'
AS
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

CREATE OR ALTER PROCEDURE [Dad].[usp_Upsert_Backup_Metadata]
    @ExportType nvarchar(50),
    @LastExportedAtUtc datetime2(3),
    @LastExportedMaxChangeDateTimeUtc datetime2(3) = NULL,
    @LastExportedJokeCount int = 0,
    @BackupBlobUri nvarchar(2048) = NULL,
    @Checksum nvarchar(256) = NULL,
    @Status nvarchar(50),
    @ErrorMessage nvarchar(max) = NULL
AS
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

PRINT 'Scheduled backup support patch completed.'
GO
