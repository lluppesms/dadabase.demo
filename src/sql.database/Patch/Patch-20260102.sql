BEGIN TRAN

INSERT INTO [Dad].[Joke] (JokeCategoryTxt, JokeTxt) VALUES
  ('Chuck Norris', 'Time waits for no man. Unless that man is Chuck Norris.')

Select * FROM [Dad].[Joke] Where JokeTxt like '%Time waits for no man%'

ROLLBACK TRAN