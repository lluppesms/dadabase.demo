BEGIN TRAN

INSERT INTO Joke (JokeCategoryTxt, JokeTxt) VALUES
  ('Chuck Norris', 'Time waits for no man. Unless that man is Chuck Norris.')

Select * From Joke Where JokeTxt like '%Time waits for no man%'

ROLLBACK TRAN