BEGIN TRAN

INSERT INTO Joke (JokeCategoryTxt, JokeTxt) VALUES
  ('Facts', 'Humans with two legs have an above average number of legs, compared to the entire population.')

Select * From JOke Where JokeTxt like '%average number%'

ROLLBACK TRAN