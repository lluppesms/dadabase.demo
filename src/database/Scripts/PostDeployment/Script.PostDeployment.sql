/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to reference a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

-- Note: To populate the database with sample jokes, use the InsertDefaultData.sql script
-- located in the Docs/sql folder of the repository. This script can be run manually
-- after deployment or integrated into your deployment pipeline.

PRINT 'Post-deployment script completed.'
PRINT 'To populate with sample jokes, run the InsertDefaultData.sql script from Docs/sql folder.'
GO
