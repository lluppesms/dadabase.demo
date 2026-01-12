/*
Post-Deployment Script for DadABase SQL Database
This script is run after the database schema deployment completes.
Use SQLCMD syntax to reference external scripts or variables.
*/

PRINT 'Post-deployment script started.'
PRINT 'Database: DadABase'
PRINT 'Schema deployment completed successfully.'

-- Note: To populate the database with sample jokes, use the InsertDefaultData.sql script
-- located in the Docs/sql folder of the repository. This script can be run manually
-- after deployment or integrated into your deployment pipeline.

PRINT 'To populate with sample jokes, run the InsertDefaultData.sql script from Docs/sql folder.'
PRINT 'Post-deployment script completed.'
GO
