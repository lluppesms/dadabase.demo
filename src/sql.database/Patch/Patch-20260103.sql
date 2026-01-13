BEGIN TRAN

INSERT INTO Joke (JokeCategoryTxt, JokeTxt) VALUES
  ('Random', 'What do you call a Jedi with an anxiety disorder?  P-anakin Skywalker')

Select * From Joke Where JokeTxt like '%Skywalker%'

ROLLBACK TRAN