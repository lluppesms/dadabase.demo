-- =============================================
-- Table: JokeJokeCategory
-- Description: Junction table for many-to-many relationship between jokes and categories
-- =============================================
CREATE TABLE [Dad].[JokeJokeCategory](
	[JokeId] [int] NOT NULL,
	[JokeCategoryId] [int] NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_JokeJokeCategory] PRIMARY KEY CLUSTERED ([JokeId] ASC, [JokeCategoryId] ASC)
)
GO

-- Default constraints
ALTER TABLE [Dad].[JokeJokeCategory] ADD CONSTRAINT [DF_JokeJokeCategory_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [Dad].[JokeJokeCategory] ADD CONSTRAINT [DF_JokeJokeCategory_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO

-- Foreign key constraint to Joke
ALTER TABLE [Dad].[JokeJokeCategory] ADD CONSTRAINT [FK_JokeJokeCategory_Joke] FOREIGN KEY([JokeId])
REFERENCES [Dad].[Joke] ([JokeId])
	ON DELETE CASCADE
GO
ALTER TABLE [Dad].[JokeJokeCategory] CHECK CONSTRAINT [FK_JokeJokeCategory_Joke]
GO

-- Foreign key constraint to JokeCategory
ALTER TABLE [Dad].[JokeJokeCategory] ADD CONSTRAINT [FK_JokeJokeCategory_JokeCategory] FOREIGN KEY([JokeCategoryId])
REFERENCES [Dad].[JokeCategory] ([JokeCategoryId])
	ON DELETE CASCADE
GO
ALTER TABLE [Dad].[JokeJokeCategory] CHECK CONSTRAINT [FK_JokeJokeCategory_JokeCategory]
GO
