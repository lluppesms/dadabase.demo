-- =============================================
-- Stored Procedure: usp_Joke_Rate
-- Description: Upserts a joke rating for a given user key and recomputes
--              the aggregate Rating and VoteCount on the parent Joke row.
--              One rating per (JokeId, RatingUserKey) is enforced both by
--              this procedure and by the unique index on JokeRating.
-- Parameters:
--   @jokeId        - The joke to rate.
--   @userRating    - Star value 1–5.
--   @ratingUserKey - Opaque user identity:
--                    authenticated → identity claim value
--                    anonymous     → "ANON_IP_<SHA256 hash>"
--   @userName      - Display name stored in CreateUserName / UpdateUserName.
-- Returns (single-row result set):
--   JokeId, UserRating, AverageRating, VoteCount, WasInsert
-- =============================================
CREATE OR ALTER PROCEDURE [Dad].[usp_Joke_Rate]
    @jokeId        INT,
    @userRating    INT,
    @ratingUserKey NVARCHAR(255),
    @userName      NVARCHAR(255) = N'UNKNOWN'
AS
BEGIN
    SET NOCOUNT ON;

    -- ---- Validate parameters -----------------------------------------------
    IF @jokeId IS NULL OR @jokeId <= 0
    BEGIN
        RAISERROR(N'@jokeId must be a positive integer.', 16, 1);
        RETURN;
    END

    IF @userRating < 1 OR @userRating > 5
    BEGIN
        RAISERROR(N'@userRating must be between 1 and 5.', 16, 1);
        RETURN;
    END

    IF @ratingUserKey IS NULL OR LEN(LTRIM(RTRIM(@ratingUserKey))) = 0
    BEGIN
        RAISERROR(N'@ratingUserKey must not be empty.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM [Dad].[Joke] WHERE [JokeId] = @jokeId)
    BEGIN
        RAISERROR(N'Joke with the specified @jokeId does not exist.', 16, 1);
        RETURN;
    END

    -- ---- Declare output variables ------------------------------------------
    DECLARE @wasInsert BIT = 0;

    -- ---- Upsert rating ------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM [Dad].[JokeRating]
        WHERE [JokeId] = @jokeId
          AND [RatingUserKey] = @ratingUserKey
    )
    BEGIN
        -- Update the existing row
        UPDATE [Dad].[JokeRating]
        SET    [UserRating]    = @userRating,
               [CreateUserName] = @userName
        WHERE  [JokeId]        = @jokeId
          AND  [RatingUserKey] = @ratingUserKey;

        SET @wasInsert = 0;
    END
    ELSE
    BEGIN
        -- Insert a new row
        INSERT INTO [Dad].[JokeRating]
            ([JokeId], [UserRating], [RatingUserKey], [CreateUserName])
        VALUES
            (@jokeId, @userRating, @ratingUserKey, @userName);

        SET @wasInsert = 1;
    END

    -- ---- Recompute aggregates on the parent Joke row -----------------------
    UPDATE [Dad].[Joke]
    SET    [Rating]    = (
               SELECT AVG(CAST([UserRating] AS DECIMAL(5,2)))
               FROM   [Dad].[JokeRating]
               WHERE  [JokeId] = @jokeId
           ),
           [VoteCount] = (
               SELECT COUNT(*)
               FROM   [Dad].[JokeRating]
               WHERE  [JokeId] = @jokeId
           )
    WHERE  [JokeId] = @jokeId;

    -- ---- Return result row -------------------------------------------------
    SELECT
        @jokeId                                         AS [JokeId],
        @userRating                                     AS [UserRating],
        (
            SELECT AVG(CAST([UserRating] AS DECIMAL(5,2)))
            FROM   [Dad].[JokeRating]
            WHERE  [JokeId] = @jokeId
        )                                               AS [AverageRating],
        (
            SELECT COUNT(*)
            FROM   [Dad].[JokeRating]
            WHERE  [JokeId] = @jokeId
        )                                               AS [VoteCount],
        @wasInsert                                      AS [WasInsert];
END
GO
