PRINT 'Pre-deployment cleanup for legacy dbo DadABase objects started.'

IF OBJECT_ID(N'[dbo].[vw_Jokes]', N'V') IS NOT NULL
    DROP VIEW [dbo].[vw_Jokes]
GO

IF OBJECT_ID(N'[dbo].[usp_Get_Random_Joke]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Get_Random_Joke]
GO

IF OBJECT_ID(N'[dbo].[usp_Joke_Search]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Joke_Search]
GO

IF OBJECT_ID(N'[dbo].[usp_Joke_Import]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Joke_Import]
GO

IF OBJECT_ID(N'[dbo].[usp_Joke_Update_ImageTxt]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Joke_Update_ImageTxt]
GO

IF EXISTS (
    SELECT 1
    FROM sys.change_tracking_tables AS ctt
    INNER JOIN sys.tables AS t ON ctt.object_id = t.object_id
    INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id
    WHERE s.name = N'dbo'
      AND t.name = N'JokeRating'
)
    ALTER TABLE [dbo].[JokeRating] DISABLE CHANGE_TRACKING
GO

IF EXISTS (
    SELECT 1
    FROM sys.change_tracking_tables AS ctt
    INNER JOIN sys.tables AS t ON ctt.object_id = t.object_id
    INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id
    WHERE s.name = N'dbo'
      AND t.name = N'JokeJokeCategory'
)
    ALTER TABLE [dbo].[JokeJokeCategory] DISABLE CHANGE_TRACKING
GO

IF EXISTS (
    SELECT 1
    FROM sys.change_tracking_tables AS ctt
    INNER JOIN sys.tables AS t ON ctt.object_id = t.object_id
    INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id
    WHERE s.name = N'dbo'
      AND t.name = N'Joke'
)
    ALTER TABLE [dbo].[Joke] DISABLE CHANGE_TRACKING
GO

IF EXISTS (
    SELECT 1
    FROM sys.change_tracking_tables AS ctt
    INNER JOIN sys.tables AS t ON ctt.object_id = t.object_id
    INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id
    WHERE s.name = N'dbo'
      AND t.name = N'JokeCategory'
)
    ALTER TABLE [dbo].[JokeCategory] DISABLE CHANGE_TRACKING
GO

IF OBJECT_ID(N'[dbo].[JokeRating]', N'U') IS NOT NULL
    DROP TABLE [dbo].[JokeRating]
GO

IF OBJECT_ID(N'[dbo].[JokeJokeCategory]', N'U') IS NOT NULL
    DROP TABLE [dbo].[JokeJokeCategory]
GO

IF OBJECT_ID(N'[dbo].[Joke]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Joke]
GO

IF OBJECT_ID(N'[dbo].[JokeCategory]', N'U') IS NOT NULL
    DROP TABLE [dbo].[JokeCategory]
GO

PRINT 'Pre-deployment cleanup for legacy dbo DadABase objects completed.'
GO