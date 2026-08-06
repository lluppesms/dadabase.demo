-- =============================================
-- Table: BackupMetadata
-- Description: Audit trail of scheduled and manual joke data backup exports
-- =============================================
CREATE TABLE [Dad].[BackupMetadata](
	[BackupMetadataId] [int] IDENTITY(1,1) NOT NULL,
	[ExportType] [nvarchar](50) NOT NULL,
	[LastExportedAtUtc] [datetime2](3) NOT NULL,
	[LastExportedMaxChangeDateTimeUtc] [datetime2](3) NULL,
	[LastExportedJokeCount] [int] NOT NULL,
	[BackupBlobUri] [nvarchar](2048) NULL,
	[Checksum] [nvarchar](256) NULL,
	[Status] [nvarchar](50) NOT NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[CreatedAtUtc] [datetime2](3) NOT NULL,
 CONSTRAINT [PK_BackupMetadata] PRIMARY KEY CLUSTERED ([BackupMetadataId] ASC)
)
GO

-- Default constraints
ALTER TABLE [Dad].[BackupMetadata] ADD CONSTRAINT [DF_BackupMetadata_LastExportedJokeCount] DEFAULT ((0)) FOR [LastExportedJokeCount]
GO
ALTER TABLE [Dad].[BackupMetadata] ADD CONSTRAINT [DF_BackupMetadata_CreatedAtUtc] DEFAULT (sysutcdatetime()) FOR [CreatedAtUtc]
GO

-- Check constraints
ALTER TABLE [Dad].[BackupMetadata] ADD CONSTRAINT [CK_BackupMetadata_Status] CHECK ([Status] IN ('Success', 'Skipped', 'Failed'))
GO

-- Indexes
CREATE INDEX [IX_BackupMetadata_ExportType_CreatedAt]
	ON [Dad].[BackupMetadata]([ExportType] ASC, [CreatedAtUtc] DESC)
GO
