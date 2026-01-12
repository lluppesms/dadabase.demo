-- =============================================
-- Table: JokeCategory
-- Description: Stores joke categories
-- =============================================
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

-- Default constraints
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
