#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
# Build in context of the src/Database.Web folder:
#   docker build -t dbw -f Dockerfile . -t yymmddXX
# Run the app
#   docker run --rm -it -p 8000:8080 dbw yymmddXX
# Browse or curl to http://localhost:8000

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DadABase.Tests/", "DadABase.Tests/"]
COPY ["DadABase.Web/", "DadABase.Web/"]
WORKDIR /src/DadABase.Web
RUN dotnet restore "DadABase.Web.csproj"
RUN dotnet build "DadABase.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/DadABase.Web
RUN dotnet publish "./DadABase.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DadABase.Web.dll"]