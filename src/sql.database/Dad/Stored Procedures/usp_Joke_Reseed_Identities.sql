CREATE PROCEDURE [Dad].[usp_Joke_Reseed_Identities]
WITH EXECUTE AS OWNER
AS
/*
  Description: Reseeds identity values for Dad joke tables after a full data reset.
               Designed to be called from import and admin workflows.
*/
BEGIN
    SET NOCOUNT ON;

    DBCC CHECKIDENT('[Dad].[JokeRating]', RESEED, 0);
    DBCC CHECKIDENT('[Dad].[JokeCategory]', RESEED, 0);
    DBCC CHECKIDENT('[Dad].[Joke]', RESEED, 0);
END
