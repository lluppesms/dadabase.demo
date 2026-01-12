-- =============================================
-- View: vw_Jokes
-- Description: Simplified view of jokes with key information
-- =============================================
CREATE VIEW [dbo].[vw_Jokes] AS
SELECT TOP (100) PERCENT 
    j.JokeId, 
    j.JokeCategoryId, 
    j.JokeCategoryTxt, 
    j.JokeTxt, 
    j.ImageTxt, 
    j.Rating
FROM [dbo].[Joke] j 
WHERE j.ActiveInd = 'Y'
ORDER BY j.JokeCategoryTxt, j.JokeTxt
GO
