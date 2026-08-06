BEGIN TRAN

INSERT INTO [Dad].[Joke] (JokeCategoryTxt, JokeTxt) VALUES
  ('Random', 'What do you call a Jedi with an anxiety disorder?  P-anakin Skywalker')

Select * FROM [Dad].[Joke] Where JokeTxt like '%Skywalker%'

ROLLBACK TRAN