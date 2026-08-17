-- =============================================
-- Migration Script: Add unique rating user keys
-- Date: 2026-08-17
-- =============================================

IF OBJECT_ID(N'[Dad].[JokeRating]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'Dad.JokeRating', N'RatingUserKey') IS NULL
    BEGIN
        ALTER TABLE [Dad].[JokeRating]
            ADD [RatingUserKey] NVARCHAR(255) NULL;
    END

    UPDATE [Dad].[JokeRating]
    SET [RatingUserKey] = N'ANON_LEGACY_' + CONVERT(NVARCHAR(11), [JokeRatingId])
    WHERE [RatingUserKey] IS NULL
       OR LEN(LTRIM(RTRIM([RatingUserKey]))) = 0;

    IF EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE [object_id] = OBJECT_ID(N'[Dad].[JokeRating]')
          AND [name] = N'RatingUserKey'
          AND [is_nullable] = 1)
    BEGIN
        ALTER TABLE [Dad].[JokeRating]
            ALTER COLUMN [RatingUserKey] NVARCHAR(255) NOT NULL;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE [object_id] = OBJECT_ID(N'[Dad].[JokeRating]')
          AND [name] = N'UX_JokeRating_JokeId_RatingUserKey')
    BEGIN
        CREATE UNIQUE INDEX [UX_JokeRating_JokeId_RatingUserKey]
            ON [Dad].[JokeRating] ([JokeId], [RatingUserKey]);
    END
END
GO
