-- =============================================
-- Migration Script: Multiple Categories Support
-- Date: 2026-01-14
-- Description: Migrates from single category per joke to multiple categories
-- =============================================

PRINT 'Starting migration to multiple categories support...'
GO

-- Step 1: Create junction table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JokeJokeCategory]') AND type in (N'U'))
BEGIN
    PRINT 'Creating JokeJokeCategory junction table...'
    
    CREATE TABLE [dbo].[JokeJokeCategory](
        [JokeId] [int] NOT NULL,
        [JokeCategoryId] [int] NOT NULL,
        [CreateDateTime] [datetime] NOT NULL,
        [CreateUserName] [nvarchar](255) NOT NULL,
     CONSTRAINT [PK_JokeJokeCategory] PRIMARY KEY CLUSTERED ([JokeId] ASC, [JokeCategoryId] ASC)
    )
    
    ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [DF_JokeJokeCategory_CreateDateTime] DEFAULT (getdate()) FOR [CreateDateTime]
    ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [DF_JokeJokeCategory_CreateUserName] DEFAULT (N'UNKNOWN') FOR [CreateUserName]
    
    ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [FK_JokeJokeCategory_Joke] FOREIGN KEY([JokeId])
    REFERENCES [dbo].[Joke] ([JokeId])
        ON DELETE CASCADE
    
    ALTER TABLE [dbo].[JokeJokeCategory] CHECK CONSTRAINT [FK_JokeJokeCategory_Joke]
    
    ALTER TABLE [dbo].[JokeJokeCategory] ADD CONSTRAINT [FK_JokeJokeCategory_JokeCategory] FOREIGN KEY([JokeCategoryId])
    REFERENCES [dbo].[JokeCategory] ([JokeCategoryId])
        ON DELETE CASCADE
    
    ALTER TABLE [dbo].[JokeJokeCategory] CHECK CONSTRAINT [FK_JokeJokeCategory_JokeCategory]
    
    PRINT 'JokeJokeCategory table created successfully.'
END
ELSE
BEGIN
    PRINT 'JokeJokeCategory table already exists.'
END
GO

-- Step 2: Migrate existing data from Joke table to junction table (if legacy fields exist)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Joke]') AND name = 'JokeCategoryId')
BEGIN
    PRINT 'Migrating existing category data to junction table...'
    
    INSERT INTO [dbo].[JokeJokeCategory] (JokeId, JokeCategoryId, CreateDateTime, CreateUserName)
    SELECT 
        j.JokeId, 
        j.JokeCategoryId,
        GETDATE(),
        'MIGRATION'
    FROM [dbo].[Joke] j
    WHERE j.JokeCategoryId IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM [dbo].[JokeJokeCategory] jjc 
            WHERE jjc.JokeId = j.JokeId AND jjc.JokeCategoryId = j.JokeCategoryId
        )
    
    PRINT 'Data migration completed.'
END
ELSE
BEGIN
    PRINT 'Legacy JokeCategoryId field does not exist, skipping data migration.'
END
GO

-- Step 3: Drop foreign key constraint on Joke table if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Joke_JokeCategory]') AND parent_object_id = OBJECT_ID(N'[dbo].[Joke]'))
BEGIN
    PRINT 'Dropping FK_Joke_JokeCategory constraint...'
    ALTER TABLE [dbo].[Joke] DROP CONSTRAINT [FK_Joke_JokeCategory]
    PRINT 'FK_Joke_JokeCategory constraint dropped.'
END
GO

-- Step 4: Drop legacy columns from Joke table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Joke]') AND name = 'JokeCategoryId')
BEGIN
    PRINT 'Dropping JokeCategoryId column...'
    ALTER TABLE [dbo].[Joke] DROP COLUMN [JokeCategoryId]
    PRINT 'JokeCategoryId column dropped.'
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Joke]') AND name = 'JokeCategoryTxt')
BEGIN
    PRINT 'Dropping JokeCategoryTxt column...'
    ALTER TABLE [dbo].[Joke] DROP COLUMN [JokeCategoryTxt]
    PRINT 'JokeCategoryTxt column dropped.'
END
GO

PRINT 'Migration to multiple categories support completed successfully.'
GO
