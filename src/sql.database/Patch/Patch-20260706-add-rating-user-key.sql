-- =============================================
-- Migration Patch: Add RatingUserKey to JokeRating
-- Date: 2026-07-06
-- Description: Adds the RatingUserKey column and unique index needed for the
--              per-user joke rating feature.  Backfills existing rows with
--              a legacy sentinel so the unique constraint can be applied.
--              Script is idempotent: safe to run multiple times.
-- =============================================

-- 1. Add RatingUserKey column if it does not already exist
IF NOT EXISTS (
    SELECT 1
    FROM   sys.columns c
    JOIN   sys.objects o ON o.object_id = c.object_id
    JOIN   sys.schemas s ON s.schema_id = o.schema_id
    WHERE  s.name = N'Dad'
      AND  o.name = N'JokeRating'
      AND  c.name = N'RatingUserKey'
)
BEGIN
    ALTER TABLE [Dad].[JokeRating]
        ADD [RatingUserKey] NVARCHAR(255) NOT NULL
            CONSTRAINT [DF_JokeRating_RatingUserKey] DEFAULT (N'UNKNOWN');

    PRINT 'Column RatingUserKey added.';
END
ELSE
BEGIN
    PRINT 'Column RatingUserKey already exists — skipping ALTER TABLE.';
END
GO

-- 2. Backfill existing rows that still carry the default 'UNKNOWN' key.
--    Rows that came from the old schema all collide on the same key; make
--    each one unique so the constraint below can be applied cleanly.
--    New key format:  ANON_LEGACY_<JokeRatingId>
UPDATE [Dad].[JokeRating]
SET    [RatingUserKey] = N'ANON_LEGACY_' + CAST([JokeRatingId] AS NVARCHAR(20))
WHERE  [RatingUserKey] = N'UNKNOWN';

PRINT CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' legacy rows backfilled.';
GO

-- 3. Add the unique index if it does not already exist
IF NOT EXISTS (
    SELECT 1
    FROM   sys.indexes i
    JOIN   sys.objects o ON o.object_id = i.object_id
    JOIN   sys.schemas s ON s.schema_id = o.schema_id
    WHERE  s.name = N'Dad'
      AND  o.name = N'JokeRating'
      AND  i.name = N'IX_JokeRating_JokeId_RatingUserKey'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_JokeRating_JokeId_RatingUserKey]
        ON [Dad].[JokeRating] ([JokeId] ASC, [RatingUserKey] ASC);

    PRINT 'Unique index IX_JokeRating_JokeId_RatingUserKey created.';
END
ELSE
BEGIN
    PRINT 'Unique index already exists — skipping.';
END
GO

PRINT 'Patch Patch-20260706-add-rating-user-key complete.';
GO
