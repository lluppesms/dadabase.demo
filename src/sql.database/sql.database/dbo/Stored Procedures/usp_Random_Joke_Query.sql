CREATE PROC [dbo].[usp_Random_Joke_Query] AS
/*
Example Usage:
  exec usp_Random_Joke_Query
*/
BEGIN
    DECLARE @MinId int = 1
    DECLARE @MaxId int = 0
    DECLARE @RandomId int = 0

	SELECT @MaxId = Max(JokeId) From Joke
	SET @MinId = 1
	SELECT @RandomId = FLOOR(RAND() * (@MaxId - @MinId + 1)) + @MinId

	SELECT TOP 1 j.JokeId, j.JokeCategoryId, j.JokeCategoryTxt, j.JokeTxt, j.ImageTxt, j.Rating, @MinId as MinId, @MaxId as MaxId, @RandomId as RandomId
	FROM Joke j
	WHERE JokeId >= @RandomId
END
GO
