/*
DROP TABLE JokeRating
DROP TABLE Joke
DROP TABLE JokeCategory
DROP TABLE JokeJokeCategory
*/

CREATE TABLE [dbo].[Joke](
	[JokeId] [int] IDENTITY(1,1) NOT NULL,
	[JokeTxt] [nvarchar](max) NOT NULL,
	[Attribution] [nvarchar](500) NULL,
	[ImageTxt] [nvarchar](max) NULL,
	[SortOrderNbr] [int] NOT NULL,
	[Rating] [decimal](3, 1) NULL,
	[VoteCount] [int] NULL,
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
	[ActiveInd] [nchar](1) NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
	[ChangeDateTime] [datetime] NOT NULL,
	[ChangeUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_JokeCategory] PRIMARY KEY CLUSTERED ([JokeCategoryId] ASC) 
)
GO

CREATE TABLE [dbo].[JokeJokeCategory](
	[JokeId] [int] NOT NULL,
	[JokeCategoryId] [int] NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[CreateUserName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_JokeJokeCategory] PRIMARY KEY CLUSTERED ([JokeId] ASC, [JokeCategoryId] ASC)
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
ALTER TABLE [dbo].[Joke] ADD CONSTRAINT [DF_Joke_ChangeDateTime] DEFAULT (getdate()) FOR [ChangeDateTime]
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
ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [DF_JokeJokeCategory_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [DF_JokeJokeCategory_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO
ALTER TABLE [dbo].[JokeRating] ADD  CONSTRAINT [DF_JokeRating_CreateDateTime]  DEFAULT (getdate()) FOR [CreateDateTime]
GO
ALTER TABLE [dbo].[JokeRating] ADD  CONSTRAINT [DF_JokeRating_CreateUserName]  DEFAULT (N'UNKNOWN') FOR [CreateUserName]
GO
ALTER TABLE [dbo].[JokeJokeCategory]  WITH CHECK ADD  CONSTRAINT [FK_JokeJokeCategory_Joke] FOREIGN KEY([JokeId])
REFERENCES [dbo].[Joke] ([JokeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JokeJokeCategory] CHECK CONSTRAINT [FK_JokeJokeCategory_Joke]
GO
ALTER TABLE [dbo].[JokeJokeCategory]  WITH CHECK ADD  CONSTRAINT [FK_JokeJokeCategory_JokeCategory] FOREIGN KEY([JokeCategoryId])
REFERENCES [dbo].[JokeCategory] ([JokeCategoryId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JokeJokeCategory] CHECK CONSTRAINT [FK_JokeJokeCategory_JokeCategory]
GO
ALTER TABLE [dbo].[JokeRating]  WITH CHECK ADD  CONSTRAINT [FK_JokeRating_Joke] FOREIGN KEY([JokeId])
REFERENCES [dbo].[Joke] ([JokeId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JokeRating] CHECK CONSTRAINT [FK_JokeRating_Joke]
GO
ALTER TABLE [dbo].[Joke] WITH CHECK ADD  CONSTRAINT [CK_Joke_ActiveInd] CHECK  (([ActiveInd]='N' OR [ActiveInd]='Y'))
GO
ALTER TABLE [dbo].[Joke] CHECK CONSTRAINT [CK_Joke_ActiveInd]
GO
ALTER TABLE [dbo].[JokeCategory] WITH CHECK ADD  CONSTRAINT [CK_JokeCategory_ActiveInd] CHECK  (([ActiveInd]='N' OR [ActiveInd]='Y'))
GO
ALTER TABLE [dbo].[JokeCategory] CHECK CONSTRAINT [CK_JokeCategory_ActiveInd]
GO
ALTER TABLE [dbo].[JokeRating] WITH CHECK ADD  CONSTRAINT [CK_JokeRating_UserRating] CHECK  (([UserRating]>=(1) AND [UserRating]<=(5)))
GO
ALTER TABLE [dbo].[JokeRating] CHECK CONSTRAINT [CK_JokeRating_UserRating]
GO

CREATE PROCEDURE [dbo].[usp_Get_Random_Joke]
AS
/*
Example Usage:
  exec usp_Get_Random_Joke
*/
BEGIN
    DECLARE @MinId int = 1
    DECLARE @MaxId int = 0
    DECLARE @RandomId int = 0

	SELECT @MaxId = Max(JokeId) From Joke
	SET @MinId = 1
	SELECT @RandomId = FLOOR(RAND() * (@MaxId - @MinId + 1)) + @MinId

	SELECT TOP 1 j.JokeId, 
	  -- Multiple categories field (comma-separated)
	  STUFF((SELECT ', ' + c.JokeCategoryTxt
	         FROM JokeJokeCategory jjc
	         INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
	         WHERE jjc.JokeId = j.JokeId
	         ORDER BY c.JokeCategoryTxt
	         FOR XML PATH('')), 1, 2, '') AS Categories,
	  j.JokeTxt, j.ImageTxt,
	  j.Rating, j.ActiveInd, j.Attribution, j.VoteCount, j.SortOrderNbr,
	  j.CreateDateTime, j.CreateUserName, j.ChangeDateTime, j.ChangeUserName
	  -- , @MinId as MinId, @MaxId as MaxId, @RandomId as RandomId
	FROM Joke j
	WHERE JokeId >= @RandomId
END
GO

CREATE PROCEDURE [dbo].[usp_Joke_Search] (
  @category as varchar(255) = NULL,
  @searchTxt as varchar(255) = NULL
) AS
/*
Example Usage:
  exec usp_Joke_Search @category = 'Chuck Norris'
  exec usp_Joke_Search @SearchTxt = 'Sun'
  exec usp_Joke_Search @category = 'Chuck Norris', @SearchTxt = 'Sun'
*/
BEGIN
  SET @category = '%' + ISNULL(@category, '') + '%'
  SET @searchTxt = '%' + ISNULL(@searchTxt, '') + '%'
	SELECT DISTINCT j.JokeId, 
	  -- Multiple categories field (comma-separated)
	  STUFF((SELECT ', ' + c.JokeCategoryTxt
	         FROM JokeJokeCategory jjc
	         INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
	         WHERE jjc.JokeId = j.JokeId
	         ORDER BY c.JokeCategoryTxt
	         FOR XML PATH('')), 1, 2, '') AS Categories,
	  j.JokeTxt, j.ImageTxt, j.Rating, j.ActiveInd, j.Attribution, j.VoteCount, j.SortOrderNbr,
	  j.CreateDateTime, j.CreateUserName, j.ChangeDateTime, j.ChangeUserName
	FROM Joke j 
	LEFT JOIN JokeJokeCategory jjc ON j.JokeId = jjc.JokeId
	LEFT JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
	WHERE c.JokeCategoryTxt LIKE @category
	  AND j.JokeTxt LIKE @searchTxt
	ORDER BY j.JokeTxt 
END
GO

-- =============================================
-- View: vw_Jokes
-- Description: Simplified view of jokes with key information including multiple categories
-- =============================================
CREATE VIEW [dbo].[vw_Jokes] AS
SELECT 
    j.JokeId, 
    -- Multiple categories field (comma-separated)
    STUFF((SELECT ', ' + c.JokeCategoryTxt
           FROM JokeJokeCategory jjc
           INNER JOIN JokeCategory c ON jjc.JokeCategoryId = c.JokeCategoryId
           WHERE jjc.JokeId = j.JokeId
           ORDER BY c.JokeCategoryTxt
           FOR XML PATH('')), 1, 2, '') AS Categories,
    j.JokeTxt, 
    j.ImageTxt, 
    j.Rating
FROM [dbo].[Joke] j 
WHERE j.ActiveInd = 'Y'
GO
