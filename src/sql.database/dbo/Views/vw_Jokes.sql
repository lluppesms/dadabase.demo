-- =============================================
-- View: vw_Jokes
-- Description: Simplified view of jokes with key information including multiple categories
-- =============================================
CREATE VIEW [dbo].[vw_Jokes] AS
SELECT 
    j.JokeId, 
    -- Multiple categories field (comma-separated)
    STUFF((SELECT ', ' + c.JokeCategoryTxt
           FROM JokeJokeCategory jjc
           INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
           WHERE jjc.JokeId = j.JokeId
           ORDER BY c.JokeCategoryTxt
           FOR XML PATH('')), 1, 2, '') AS Categories,
    j.JokeTxt, 
    j.ImageTxt, 
    j.Rating
FROM [dbo].[Joke] j 
WHERE j.ActiveInd = 'Y'
GO
