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

PRINT 'To grant rights to your CICD pipeline to use the DACPAC to create the schema:'
PRINT '  Creating user [your_cicd_pipeline_sp] from external provider...';
PRINT '  ALTER ROLE db_owner ADD MEMBER [your_cicd_pipeline_sp];'

PRINT 'To grant rights to your application to use database:'
PRINT '  Creating user [your_managed_identity] from external provider...';
PRINT '  ALTER ROLE db_datareader ADD MEMBER [your_managed_identity];'
PRINT '  ALTER ROLE db_datawriter ADD MEMBER [your_managed_identity];'

GO
