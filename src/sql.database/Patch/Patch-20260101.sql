BEGIN TRAN

INSERT INTO [Dad].[Joke] (JokeCategoryTxt, JokeTxt) VALUES
  ('Facts', 'Humans with two legs have an above average number of legs, compared to the entire population.')

Select * FROM [Dad].[Joke] Where JokeTxt like '%average number%'

ROLLBACK TRAN