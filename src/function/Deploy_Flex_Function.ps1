# Create the function app in the portal
az functionapp create --resource-group rg_dadabase_full-dev --name ll-flex-test-2 --storage-account lfldadabasedevstorefunc --flexconsumption-location centralus --runtime dotnet-isolated --runtime-version 10

# build the zip file
# then deploy the zip file to the function app
cd src\function
dotnet publish -c Release -o ./publish
cd publish
Compress-Archive -Path * -DestinationPath ../deploy.zip -Force
cd ..
az functionapp deployment source config-zip --resource-group rg_dadabase_full-dev --name ll-flex-test-2 --src ./deploy.zip
