-- =============================================
-- Migration: Add durable per-user joke ratings
-- This script is safe to run repeatedly.
-- =============================================

IF COL_LENGTH(N'Dad.JokeRating', N'RatingUserKey') IS NULL
BEGIN
    ALTER TABLE [Dad].[JokeRating] ADD [RatingUserKey] NVARCHAR(255) NULL;
END
GO

;WITH RankedRatings AS
(
    SELECT
        [JokeRatingId],
        [JokeId],
        BaseKey = COALESCE(NULLIF(LTRIM(RTRIM([CreateUserName])), N''), N'ANON_LEGACY'),
        DuplicateCount = COUNT(*) OVER (
            PARTITION BY [JokeId], COALESCE(NULLIF(LTRIM(RTRIM([CreateUserName])), N''), N'ANON_LEGACY')
        )
    FROM [Dad].[JokeRating]
    WHERE [RatingUserKey] IS NULL
)
UPDATE RankedRatings
SET [RatingUserKey] = LEFT(
    CASE WHEN DuplicateCount > 1
         THEN BaseKey + N'_LEGACY_' + CONVERT(NVARCHAR(20), [JokeRatingId])
         ELSE BaseKey
    END,
    255
);
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'[Dad].[JokeRating]')
      AND [name] = N'RatingUserKey'
      AND [is_nullable] = 1
)
BEGIN
    ALTER TABLE [Dad].[JokeRating] ALTER COLUMN [RatingUserKey] NVARCHAR(255) NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints
    WHERE [parent_object_id] = OBJECT_ID(N'[Dad].[JokeRating]')
      AND [name] = N'DF_JokeRating_RatingUserKey'
)
BEGIN
    ALTER TABLE [Dad].[JokeRating]
        ADD CONSTRAINT [DF_JokeRating_RatingUserKey] DEFAULT (N'ANON_LEGACY') FOR [RatingUserKey];
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Dad].[JokeRating]')
      AND [name] = N'UX_JokeRating_JokeId_RatingUserKey'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_JokeRating_JokeId_RatingUserKey]
        ON [Dad].[JokeRating] ([JokeId], [RatingUserKey]);
END
GO