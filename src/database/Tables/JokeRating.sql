-- =============================================
-- Table: JokeRating
-- Description: Stores user ratings for jokes
-- =============================================
CREATE TABLE [dbo].[JokeRating](
	[JokeRatingId] [int] IDENTITY(1,1) NOT NULL,
	[JokeId] [int] NOT NULL,
	[UserRating] [int] NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_JokeRating] PRIMARY KEY CLUSTERED ([JokeRatingId] ASC)
)
GO

-- Default constraints
ALTER TABLE [dbo].[JokeRating] ADD CONSTRAINT [DF_JokeRating_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [dbo].[JokeRating] ADD CONSTRAINT [DF_JokeRating_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO

-- Foreign key constraint to Joke
ALTER TABLE [dbo].[JokeRating] WITH CHECK ADD CONSTRAINT [FK_JokeRating_Joke] FOREIGN KEY([JokeId])
REFERENCES [dbo].[Joke] ([JokeId])
	ON UPDATE CASCADE
	ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JokeRating] CHECK CONSTRAINT [FK_JokeRating_Joke]
GO
