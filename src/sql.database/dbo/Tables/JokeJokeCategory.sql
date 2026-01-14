-- =============================================
-- Table: JokeJokeCategory
-- Description: Junction table for many-to-many relationship between jokes and categories
-- =============================================
CREATE TABLE [dbo].[JokeJokeCategory](
	[JokeId] [int] NOT NULL,
	[JokeCategoryId] [int] NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_JokeJokeCategory] PRIMARY KEY CLUSTERED ([JokeId] ASC, [JokeCategoryId] ASC)
)
GO

-- Default constraints
ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [DF_JokeJokeCategory_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [DF_JokeJokeCategory_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO

-- Foreign key constraint to Joke
ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [FK_JokeJokeCategory_Joke] FOREIGN KEY([JokeId])
REFERENCES [dbo].[Joke] ([JokeId])
	ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JokeJokeCategory] CHECK CONSTRAINT [FK_JokeJokeCategory_Joke]
GO

-- Foreign key constraint to JokeCategory
ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [FK_JokeJokeCategory_JokeCategory] FOREIGN KEY([JokeCategoryId])
REFERENCES [dbo].[JokeCategory] ([JokeCategoryId])
	ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JokeJokeCategory] CHECK CONSTRAINT [FK_JokeJokeCategory_JokeCategory]
GO
