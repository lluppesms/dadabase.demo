CREATE PROCEDURE [dbo].[usp_Joke_Query] (
    @category AS VARCHAR(255) = NULL,
    @searchTxt AS VARCHAR(255) = NULL
)
AS
/* 
Example Usage:
  exec usp_Joke_Query @category = 'Chuck Norris'
  exec usp_Joke_Query @SearchTxt = 'Sun'
  exec usp_Joke_Query @category = 'Chuck Norris', @SearchTxt = 'Sun'
*/
BEGIN
    SET @category = '%' + ISNULL(@category, '') + '%'
    SET @searchTxt = '%' + ISNULL(@searchTxt, '') + '%'

    SELECT
        j.JokeId,
        j.JokeCategoryId,
        j.JokeCategoryTxt,
        j.JokeTxt,
        j.ImageTxt,
        j.Rating
    FROM Joke j
    WHERE JokeCategoryTxt LIKE @category
      AND JokeTxt LIKE @searchTxt
    ORDER BY j.JokeCategoryTxt, j.JokeTxt
END
