CREATE PROCEDURE [Dad].[usp_Joke_Rate]
    @JokeId INT,
    @UserRating INT,
    @RatingUserKey NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @UserRating NOT BETWEEN 1 AND 5
        THROW 50001, 'UserRating must be between 1 and 5.', 1;

    IF @RatingUserKey IS NULL OR LEN(LTRIM(RTRIM(@RatingUserKey))) = 0
        THROW 50002, 'RatingUserKey is required.', 1;

    DECLARE @WasInsert BIT = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM [Dad].[Joke] WITH (UPDLOCK, HOLDLOCK)
            WHERE [JokeId] = @JokeId)
            THROW 50003, 'The specified joke does not exist.', 1;

        IF EXISTS (
            SELECT 1
            FROM [Dad].[JokeRating] WITH (UPDLOCK, HOLDLOCK)
            WHERE [JokeId] = @JokeId
              AND [RatingUserKey] = @RatingUserKey)
        BEGIN
            UPDATE [Dad].[JokeRating]
            SET [UserRating] = @UserRating
            WHERE [JokeId] = @JokeId
              AND [RatingUserKey] = @RatingUserKey;
        END
        ELSE
        BEGIN
            INSERT INTO [Dad].[JokeRating]
                ([JokeId], [UserRating], [RatingUserKey], [CreateUserName])
            VALUES
                (@JokeId, @UserRating, @RatingUserKey, @RatingUserKey);

            SET @WasInsert = 1;
        END

        UPDATE [Dad].[Joke]
        SET [Rating] = ratingSummary.[AverageRating],
            [VoteCount] = ratingSummary.[VoteCount]
        FROM [Dad].[Joke] AS joke
        CROSS APPLY (
            SELECT
                CAST(AVG(CAST([UserRating] AS DECIMAL(10, 2))) AS DECIMAL(3, 1)) AS [AverageRating],
                COUNT(*) AS [VoteCount]
            FROM [Dad].[JokeRating]
            WHERE [JokeId] = @JokeId
        ) AS ratingSummary
        WHERE joke.[JokeId] = @JokeId;

        SELECT
            joke.[JokeId],
            @UserRating AS [UserRating],
            joke.[Rating] AS [AverageRating],
            joke.[VoteCount],
            @WasInsert AS [WasInsert]
        FROM [Dad].[Joke] AS joke
        WHERE joke.[JokeId] = @JokeId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO
