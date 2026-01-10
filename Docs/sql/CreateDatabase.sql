/*
DROP TABLE JokeRating
DROP TABLE Joke
DROP TABLE JokeCategory
*/

CREATE TABLE [dbo].[Joke](
	[JokeId] [int] IDENTITY(1,1) NOT NULL,
	[JokeTxt] [nvarchar](max) NOT NULL,
	[JokeCategoryId] [int] NULL,
	[JokeCategoryTxt] [nvarchar](500) NULL,
	[Attribution] [nvarchar](500) NULL,
	[ImageTxt]  [nvarchar](max) NULL,
	[SortOrderNbr] [int] NOT NULL,
	[Rating] [decimal(3,1)]  NULL,
	[VoteCount] [int]  NULL,
	[ActiveInd] [nchar](1) NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
	[ChangeDateTime] [datetime] NOT NULL,
	[ChangeUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Joke] PRIMARY KEY CLUSTERED ([JokeId] ASC)
)
GO

CREATE TABLE [dbo].[JokeCategory](
	[JokeCategoryId] [int] IDENTITY(1,1) NOT NULL,
	[JokeCategoryTxt] [nvarchar](500) NULL,
	[SortOrderNbr] [int] NOT NULL,
	[ActiveInd] [nvarchar](1) NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
	[ChangeDateTime] [datetime] NOT NULL,
	[ChangeUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_JokeCategory] PRIMARY KEY CLUSTERED ([JokeCategoryId] ASC) 
)
GO

CREATE TABLE [dbo].[JokeRating](
	[JokeRatingId] [int] IDENTITY(1,1) NOT NULL,
	[JokeId] [int] NOT NULL,
	[UserRating] [int] NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_JokeRating] PRIMARY KEY CLUSTERED ([JokeRatingId] ASC)
)
GO



ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_SortOrderNbr] DEFAULT ((50)) FOR [SortOrderNbr]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_ActiveInd] DEFAULT (N'Y') FOR [ActiveInd]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_CreateDateTime1] DEFAULT (getdate()) FOR [ChangeDateTime]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_ChangeUserName] DEFAULT (N'UNKNOWN') FOR [ChangeUserName]
GO
ALTER TABLE [dbo].[JokeCategory] ADD CONSTRAINT [DF_JokeCategory_SortOrderNbr] DEFAULT ((50)) FOR [SortOrderNbr]
GO
ALTER TABLE [dbo].[JokeCategory] ADD CONSTRAINT [DF_JokeCategory_ActiveInd] DEFAULT (N'Y') FOR [ActiveInd]
GO
ALTER TABLE [dbo].[JokeCategory] ADD CONSTRAINT [DF_JokeCategory_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [dbo].[JokeCategory] ADD CONSTRAINT [DF_JokeCategory_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO
ALTER TABLE [dbo].[JokeCategory] ADD CONSTRAINT [DF_JokeCategory_ChangeDateTime] DEFAULT (getdate()) FOR [ChangeDateTime]
GO
ALTER TABLE [dbo].[JokeCategory] ADD CONSTRAINT [DF_JokeCategory_ChangeUserName] DEFAULT (N'UNKNOWN') FOR [ChangeUserName]
GO
ALTER TABLE [dbo].[JokeRating] ADD CONSTRAINT [DF_JokeRating_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [dbo].[JokeRating] ADD CONSTRAINT [DF_JokeRating_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO

ALTER TABLE [dbo].[Joke] WITH CHECK ADD CONSTRAINT [FK_Joke_JokeCategory] FOREIGN KEY([JokeCategoryId])
REFERENCES [dbo].[JokeCategory] ([JokeCategoryId])
	ON UPDATE CASCADE
	ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Joke] CHECK CONSTRAINT [FK_Joke_JokeCategory]
GO

ALTER TABLE [dbo].[JokeRating] WITH CHECK ADD CONSTRAINT [FK_JokeRating_Joke] FOREIGN KEY([JokeId])
REFERENCES [dbo].[Joke] ([JokeId])
	ON UPDATE CASCADE
	ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JokeRating] CHECK CONSTRAINT [FK_JokeRating_Joke]
GO
