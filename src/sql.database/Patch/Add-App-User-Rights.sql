CREATE USER [llldadabase1-app-id] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [llldadabase1-app-id];
ALTER ROLE db_datawriter ADD MEMBER [llldadabase1-app-id];
