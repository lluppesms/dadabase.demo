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

-- Step 2: Migrate existing data from Joke table to junction table
PRINT 'Migrating existing category data to junction table...'
GO

-- Insert existing joke-category relationships into junction table
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
GO

PRINT 'Data migration completed.'
GO

-- Step 3: Drop foreign key constraint on Joke table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Joke_JokeCategory]') AND parent_object_id = OBJECT_ID(N'[dbo].[Joke]'))
BEGIN
    PRINT 'Dropping FK_Joke_JokeCategory constraint...'
    ALTER TABLE [dbo].[Joke] DROP CONSTRAINT [FK_Joke_JokeCategory]
    PRINT 'FK_Joke_JokeCategory constraint dropped.'
END
GO

-- Step 4: Drop old columns from Joke table (keeping them for now as backup, can be removed later)
-- Note: Keeping JokeCategoryId and JokeCategoryTxt for backward compatibility temporarily
-- They can be removed in a future migration once all code is updated

PRINT 'Migration to multiple categories support completed successfully.'
PRINT 'Note: JokeCategoryId and JokeCategoryTxt columns retained for backward compatibility.'
PRINT 'These columns can be removed in a future migration.'
GO
