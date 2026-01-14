-- =============================================
-- Table: Joke
-- Description: Stores dad jokes with metadata
-- =============================================
CREATE TABLE [dbo].[Joke](
	[JokeId] [int] IDENTITY(1,1) NOT NULL,
	[JokeTxt] [nvarchar](max) NOT NULL,
	[Attribution] [nvarchar](500) NULL,
	[ImageTxt]  [nvarchar](max) NULL,
	[SortOrderNbr] [int] NOT NULL,
	[Rating] [decimal](3,1)  NULL,
	[VoteCount] [int]  NULL,
	[ActiveInd] [nchar](1) NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
	[ChangeDateTime] [datetime] NOT NULL,
	[ChangeUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Joke] PRIMARY KEY CLUSTERED ([JokeId] ASC)
)
GO

-- Default constraints
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_SortOrderNbr] DEFAULT ((50)) FOR [SortOrderNbr]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_ActiveInd] DEFAULT (N'Y') FOR [ActiveInd]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_ChangeDateTime] DEFAULT (getdate()) FOR [ChangeDateTime]
GO
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_ChangeUserName] DEFAULT (N'UNKNOWN') FOR [ChangeUserName]
GO

-- Check constraints
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [CK_Joke_ActiveInd] CHECK ([ActiveInd] IN ('Y', 'N'))
GO
