
-- CREATE PROC [dbo].[usp_Get_Random_Joke] AS
ALTER PROC [dbo].usp_Get_Random_Joke AS
/*
Example Usage:
  exec usp_Get_Random_Joke
*/
BEGIN
    DECLARE @MinId int = 1
    DECLARE @MaxId int = 0
    DECLARE @RandomId int = 0

	SELECT @MaxId = Max(JokeId) From Joke
	SET @MinId = 1
	SELECT @RandomId = FLOOR(RAND() * (@MaxId - @MinId + 1)) + @MinId

	SELECT TOP 1 j.JokeId, j.JokeCategoryId,
	  j.JokeCategoryTxt, j.JokeTxt, j.ImageTxt,
	  j.Rating, j.ActiveInd, j.Attribution, j.VoteCount, j.SortOrderNbr,
	  j.CreateDateTime, j.CreateUserName, j.ChangeDateTime, j.ChangeUserName
	  -- , @MinId as MinId, @MaxId as MaxId, @RandomId as RandomId
	FROM Joke j
	WHERE JokeId >= @RandomId
END
GO
