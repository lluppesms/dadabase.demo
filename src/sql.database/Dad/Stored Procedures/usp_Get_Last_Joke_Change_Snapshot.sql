CREATE PROCEDURE [Dad].[usp_Get_Last_Joke_Change_Snapshot]
AS
/*
Returns a snapshot of the current active joke data used to detect whether
anything has changed since the last backup export.

Example Usage:
  exec [Dad].[usp_Get_Last_Joke_Change_Snapshot]
*/
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
