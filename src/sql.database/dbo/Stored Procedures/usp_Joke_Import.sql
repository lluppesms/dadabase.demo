CREATE PROCEDURE [dbo].[usp_Joke_Import]
    @tsvData              NVARCHAR(MAX),  -- Full tab-separated export (with header row), one joke per line
    @RemovePreviousJokes  BIT = 0         -- When 1: DELETE all existing jokes/categories and RESEED identities before importing
AS
/*
  Description: Imports jokes from a tab-delimited (TSV) string.
               Expected column order (header row is automatically skipped):
                 JokeId  Categories  JokeTxt  ImageTxt  Attribution  Rating  VoteCount
               The proc parses the data into a temp table, then does batch INSERTs for
               new categories, new jokes, and their category associations.
               Duplicate jokes (matched by JokeTxt) are silently skipped.
               No permanent tables are created.
               When @RemovePreviousJokes = 1, all existing joke data is removed and
               identity columns are reseeded to 1 before the import runs.
  Returns:     A single-row result set with ImportedCount INT.
  Example Usage:
    -- Merge import (keep existing jokes):
    EXEC usp_Joke_Import @tsvData = N'JokeId	Categories	JokeTxt	ImageTxt	Attribution	Rating	VoteCount
1	Chuck Norris	Chuck Norris can divide by zero.			0	0'

    -- Replace import (wipe all existing jokes first):
    EXEC usp_Joke_Import @RemovePreviousJokes = 1, @tsvData = N'JokeId	Categories	JokeTxt	ImageTxt	Attribution	Rating	VoteCount
1	Chuck Norris	Chuck Norris can divide by zero.			0	0'
*/
BEGIN
    SET NOCOUNT ON;

    -- ── Step 0 (optional): Wipe all existing joke data and reseed identities ────
    IF @RemovePreviousJokes = 1
    BEGIN
        DELETE FROM JokeRating
        DELETE FROM JokeJokeCategory
        DELETE FROM JokeCategory
        DELETE FROM Joke
        DBCC CHECKIDENT('JokeRating',  RESEED, 1)
        DBCC CHECKIDENT('JokeCategory', RESEED, 1)
        DBCC CHECKIDENT('Joke',         RESEED, 1)
    END

    -- ── Step 1: Parse the TSV into a temp table ────────────────────────────────
    CREATE TABLE #ImportRows
    (
        LineNum     INT            IDENTITY(1,1),
        JokeId      INT            NULL,
        Categories  NVARCHAR(500)  NULL,
        JokeTxt     NVARCHAR(MAX)  NULL,
        ImageTxt    NVARCHAR(MAX)  NULL,
        Attribution NVARCHAR(500)  NULL,
        Rating      DECIMAL(3,1)   NULL,
        VoteCount   INT            NULL
    );

    -- Split the input robustly on CRLF, LF, or CR, then trim and parse each line
    ;WITH RawLines AS (
      SELECT value AS line
      FROM STRING_SPLIT(REPLACE(REPLACE(@tsvData, CHAR(13) + CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10))
    )
    INSERT INTO #ImportRows (JokeId, Categories, JokeTxt, ImageTxt, Attribution, Rating, VoteCount)
    SELECT
      TRY_CAST(NULLIF(LTRIM(RTRIM(JSON_VALUE(CONCAT('["', REPLACE(REPLACE(REPLACE(REPLACE(line, '"', '\"'), CHAR(9), '","'), CHAR(13), ''), CHAR(10), ''), '"]'), '$[0]'))), '') AS INT),
      NULLIF(LTRIM(RTRIM(JSON_VALUE(CONCAT('["', REPLACE(REPLACE(REPLACE(REPLACE(line, '"', '\"'), CHAR(9), '","'), CHAR(13), ''), CHAR(10), ''), '"]'), '$[1]'))), ''),
      NULLIF(LTRIM(RTRIM(JSON_VALUE(CONCAT('["', REPLACE(REPLACE(REPLACE(REPLACE(line, '"', '\"'), CHAR(9), '","'), CHAR(13), ''), CHAR(10), ''), '"]'), '$[2]'))), ''),
      NULLIF(LTRIM(RTRIM(JSON_VALUE(CONCAT('["', REPLACE(REPLACE(REPLACE(REPLACE(line, '"', '\"'), CHAR(9), '","'), CHAR(13), ''), CHAR(10), ''), '"]'), '$[3]'))), ''),
      NULLIF(LTRIM(RTRIM(JSON_VALUE(CONCAT('["', REPLACE(REPLACE(REPLACE(REPLACE(line, '"', '\"'), CHAR(9), '","'), CHAR(13), ''), CHAR(10), ''), '"]'), '$[4]'))), ''),
      TRY_CAST(NULLIF(LTRIM(RTRIM(JSON_VALUE(CONCAT('["', REPLACE(REPLACE(REPLACE(REPLACE(line, '"', '\"'), CHAR(9), '","'), CHAR(13), ''), CHAR(10), ''), '"]'), '$[5]'))), '') AS DECIMAL(3,1)),
      TRY_CAST(NULLIF(LTRIM(RTRIM(JSON_VALUE(CONCAT('["', REPLACE(REPLACE(REPLACE(REPLACE(line, '"', '\"'), CHAR(9), '","'), CHAR(13), ''), CHAR(10), ''), '"]'), '$[6]'))), '') AS INT)
    FROM RawLines
    WHERE LTRIM(RTRIM(line)) <> ''
      -- Skip the header row
      AND LEFT(LTRIM(line), 6) <> 'JokeId';

    -- ── Step 2: Ensure all referenced categories exist ─────────────────────────
    INSERT INTO JokeCategory (JokeCategoryTxt, ActiveInd, SortOrderNbr, CreateDateTime, CreateUserName, ChangeDateTime, ChangeUserName)
    SELECT DISTINCT
        LTRIM(RTRIM(cat.value)) AS JokeCategoryTxt,
        'Y', 50, GETUTCDATE(), 'IMPORT', GETUTCDATE(), 'IMPORT'
    FROM #ImportRows r
    CROSS APPLY STRING_SPLIT(ISNULL(r.Categories, ''), ',') cat
    WHERE LTRIM(RTRIM(cat.value)) <> ''
      AND LTRIM(RTRIM(cat.value)) NOT IN (SELECT JokeCategoryTxt FROM JokeCategory WHERE JokeCategoryTxt IS NOT NULL);

    -- ── Step 3: Insert new jokes (skip duplicates matched by JokeTxt) ──────────
    INSERT INTO Joke
        (JokeTxt, Attribution, ImageTxt, ActiveInd, SortOrderNbr, Rating, VoteCount,
         CreateDateTime, CreateUserName, ChangeDateTime, ChangeUserName)
    SELECT
        r.JokeTxt,
        r.Attribution,
        r.ImageTxt,
        'Y',
        50,
        ISNULL(r.Rating, 0),
        ISNULL(r.VoteCount, 0),
        GETUTCDATE(), 'IMPORT', GETUTCDATE(), 'IMPORT'
    FROM #ImportRows r
    WHERE r.JokeTxt IS NOT NULL
      AND r.JokeTxt NOT IN (SELECT JokeTxt FROM Joke WHERE JokeTxt IS NOT NULL);

    DECLARE @ImportedCount INT = @@ROWCOUNT;

    -- ── Step 4: Wire up joke-category associations for the newly inserted jokes ─
    INSERT INTO JokeJokeCategory (JokeId, JokeCategoryId)
    SELECT DISTINCT jk.JokeId, c.JokeCategoryId
    FROM Joke jk
    INNER JOIN #ImportRows r ON jk.JokeTxt = r.JokeTxt
    CROSS APPLY STRING_SPLIT(ISNULL(r.Categories, ''), ',') cat
    INNER JOIN JokeCategory c ON c.JokeCategoryTxt = LTRIM(RTRIM(cat.value))
    WHERE LTRIM(RTRIM(cat.value)) <> ''
      AND NOT EXISTS (
          SELECT 1 FROM JokeJokeCategory jjc
          WHERE jjc.JokeId = jk.JokeId AND jjc.JokeCategoryId = c.JokeCategoryId
      );

    DROP TABLE #ImportRows;

    -- Return count of newly inserted jokes
    SELECT @ImportedCount AS ImportedCount;
END
