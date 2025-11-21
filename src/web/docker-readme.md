# Docker Sample Commands

## Build the app

``` bash
cd src\Dadabase
cd /mnt/c/Projects/Dadabase/dadabase.net8.web.demo/src/Dadabase
docker build -t dbw -f Dockerfile . -t 020401
```

## Run the app

The docker run command creates and runs the container as a single command. This command eliminates the need to run docker create and then docker start. You can also set this command to automatically delete the container when the container stops by adding --rm

``` bash
docker run --rm -it -p 8000:8080 dbw 020401
curl http://localhost:8000
```

## WSL Tips

If you are in WSL and it says `ERROR: Cannot connect to the Docker daemon... Is the docker daemon running?`, then run this command:

``` bash
sudo service docker start
```

## .NET 8 Notes

I tried lots of things that didn't work...  lots of errors...

Found this note: .NET 8 changed the default port from 80 to 8080...  things have changed...?
[https://learn.microsoft.com/en-us/dotnet/core/compatibility/containers/8.0/aspnet-port](https://learn.microsoft.com/en-us/dotnet/core/compatibility/containers/8.0/aspnet-port)

## Run with a parameter

``` bash
docker run -it --rm dbw AzureStorageAccountEndpoint="https://xxxxxx.blob.core.windows.net/"
```

## Inspect the container

``` bash
docker inspect dbw
```

## View list of images

``` bash
docker images 
```

## View current usage

``` bash
docker stats
```

## Create a new container (that is stopped)

``` bash
docker create --name dbw-container dbw
```

## To see a list of all containers

``` bash
docker ps -a
```

## Connect to a running container to see the output and peek at the output stream

``` bash
docker attach --sig-proxy=false dbw-container
```

## Start the container and show only containers that are running

``` bash
docker start dbw-container
docker ps
```

## Stop the container

``` bash
docker stop dbw-container
```

## Delete the container and check for existence

``` bash
docker ps -a
docker rm dbw-container
docker ps -a
```

## Delete images you no longer want

 You can delete any images that you no longer want on your machine.  Delete the image created by your Dockerfile and then delete the .NET image the Dockerfile was based on. You can use the IMAGE ID or the REPOSITORY:TAG formatted string.

``` bash
  docker rmi dbw:latest
  docker rmi mcr.microsoft.com/dotnet/aspnet:9.0
```

## Debugging commands - things I tried to make this work when it was failing

``` bash
docker run --rm -it dbw
   Now listening on: http://[::]:8080

docker run -it --rm dbw -p 7273:443 -p 5178:80 -e ASPNETCORE_HTTPS_PORT=https://+7273
   Now listening on: http://[::]:8080

docker run -it --rm dbw 012102

docker run --rm -it -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_ENVIRONMENT=Development -v %APPDATA%\microsoft\UserSecrets\:/root/.microsoft/usersecrets -v %USERPROFILE%\.aspnet\https:/root/.aspnet/https/ dbw
docker run --rm -it -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_ENVIRONMENT=Development dbw
docker run --rm -it -p 8000:80             -e ASPNETCORE_URLS="http://+"           -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_ENVIRONMENT=Development dbw

docker run --rm -it -p 8000:80 -e ASPNETCORE_URLS="http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_ENVIRONMENT=Development dbw

docker run --rm -it -p 8080:32768 -p 8081:32769 dbw 012106

dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p crypticpassword
dotnet dev-certs https --trust
dotnet user-secrets -p  Dadabase.Web/Dadabase.Web.csproj set "Kestrel:Certificates:Development:Password" "crypticpassword"
docker build -t dbw -f Dockerfile .
docker run --rm -it -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_ENVIRONMENT=Development -v %APPDATA%\microsoft\UserSecrets\:/root/.microsoft/usersecrets -v %USERPROFILE%\.aspnet\https:/root/.aspnet/https/ dbw
```
