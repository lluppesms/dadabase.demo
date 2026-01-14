CREATE PROCEDURE [dbo].[usp_Joke_Search] (
  @category as varchar(255) = NULL,
  @searchTxt as varchar(255) = NULL
) AS
/*
Example Usage:
  exec usp_Joke_Search @category = 'Chuck Norris'
  exec usp_Joke_Search @SearchTxt = 'Sun'
  exec usp_Joke_Search @category = 'Chuck Norris', @SearchTxt = 'Sun'
*/
BEGIN
  SET @category = '%' + ISNULL(@category, '') + '%'
  SET @searchTxt = '%' + ISNULL(@searchTxt, '') + '%'
	SELECT DISTINCT j.JokeId, 
	  -- Multiple categories field (comma-separated)
	  STUFF((SELECT ', ' + c.JokeCategoryTxt
	         FROM JokeJokeCategory jjc
	         INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
	         WHERE jjc.JokeId = j.JokeId
	         ORDER BY c.JokeCategoryTxt
	         FOR XML PATH('')), 1, 2, '') AS Categories,
	  j.JokeTxt, j.ImageTxt, j.Rating, j.ActiveInd, j.Attribution, j.VoteCount, j.SortOrderNbr,
	  j.CreateDateTime, j.CreateUserName, j.ChangeDateTime, j.ChangeUserName
	FROM Joke j 
	LEFT JOIN JokeJokeCategory jjc ON j.JokeId = jjc.JokeId
	LEFT JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
	WHERE c.JokeCategoryTxt LIKE @category
	  AND j.JokeTxt LIKE @searchTxt
	ORDER BY j.JokeTxt 
END
