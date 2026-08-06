CREATE PROCEDURE [Dad].[usp_Joke_Update_ImageTxt] (
  @jokeId as int,
  @imageTxt as nvarchar(max)
) AS
/*
Example Usage:
  exec usp_Joke_Update_ImageTxt @jokeId = 1, @imageTxt = 'A cartoon scene showing...'
*/
BEGIN
  UPDATE [Dad].[Joke] 
  SET ImageTxt = @imageTxt,
      ChangeDateTime = GETDATE(),
      ChangeUserName = 'AI_IMAGE_GENERATOR'
  WHERE JokeId = @jokeId
END
GO
