CREATE PROCEDURE [Dad].[usp_Joke_Rate]
    @jokeId         INT,
    @userRating     INT,
    @ratingUserKey  NVARCHAR(255),
    @requestingUserName NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM [Dad].[Joke] WHERE [JokeId] = @jokeId)
        THROW 50001, 'The requested joke does not exist.', 1;

    IF @userRating NOT BETWEEN 1 AND 5
        THROW 50002, 'UserRating must be between 1 and 5.', 1;

    IF NULLIF(LTRIM(RTRIM(@ratingUserKey)), N'') IS NULL
        THROW 50003, 'RatingUserKey is required.', 1;

    IF LEN(@ratingUserKey) > 255
        THROW 50004, 'RatingUserKey cannot exceed 255 characters.', 1;

    DECLARE @wasInsert BIT = 0;
    DECLARE @createUserName NVARCHAR(255) = COALESCE(NULLIF(@requestingUserName, N''), N'RATING');

    BEGIN TRANSACTION;

    IF EXISTS (
        SELECT 1
        FROM [Dad].[JokeRating] WITH (UPDLOCK, HOLDLOCK)
        WHERE [JokeId] = @jokeId AND [RatingUserKey] = @ratingUserKey
    )
    BEGIN
        UPDATE [Dad].[JokeRating]
        SET [UserRating] = @userRating,
            [CreateDateTime] = GETUTCDATE(),
            [CreateUserName] = @createUserName
        WHERE [JokeId] = @jokeId AND [RatingUserKey] = @ratingUserKey;
    END
    ELSE
    BEGIN
        INSERT INTO [Dad].[JokeRating]
            ([JokeId], [UserRating], [CreateDateTime], [CreateUserName], [RatingUserKey])
        VALUES
            (@jokeId, @userRating, GETUTCDATE(), @createUserName, @ratingUserKey);

        SET @wasInsert = 1;
    END;

    DECLARE @averageRating DECIMAL(3,1);
    DECLARE @voteCount INT;

    SELECT
        @averageRating = CAST(AVG(CAST([UserRating] AS DECIMAL(3,1))) AS DECIMAL(3,1)),
        @voteCount = COUNT(*)
    FROM [Dad].[JokeRating]
    WHERE [JokeId] = @jokeId;

    UPDATE [Dad].[Joke]
    SET [Rating] = COALESCE(@averageRating, 0),
        [VoteCount] = COALESCE(@voteCount, 0)
    WHERE [JokeId] = @jokeId;

    COMMIT TRANSACTION;

    SELECT
        @jokeId AS [JokeId],
        @userRating AS [UserRating],
        COALESCE(@averageRating, 0) AS [AverageRating],
        COALESCE(@voteCount, 0) AS [VoteCount],
        @wasInsert AS [WasInsert];
END
GO