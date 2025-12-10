#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
# Build in context of the src/Database.Web folder:
#   docker build -t dbw -f Dockerfile . -t yymmddXX
# Run the app
#   docker run --rm -it -p 8000:8080 dbw yymmddXX
# Browse or curl to http://localhost:8000

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
# 1. Added Azure CLI installation following Microsoft's official installation instructions for Debian-based containers
# 2. Added required dependencies: ca-certificates, curl, apt-transport-https, lsb-release, gnupg
# 3. Added libdbus-1-3 for the Azure Identity MSAL runtime
# 4. Cleaned up apt cache with rm -rf /var/lib/apt/lists/* to keep the image size smaller
RUN apt-get update && apt-get install -y \
    ca-certificates \
    curl \
    apt-transport-https \
    lsb-release \
    gnupg \
    libdbus-1-3 \
    libx11-6 \
    libglib2.0-0 \
    libgssapi-krb5-2 \
    && mkdir -p /etc/apt/keyrings \
    && curl -sLS https://packages.microsoft.com/keys/microsoft.asc | \
       gpg --dearmor -o /etc/apt/keyrings/microsoft.gpg \
    && chmod go+r /etc/apt/keyrings/microsoft.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/repos/azure-cli/ $(lsb_release -cs) main" | \
       tee /etc/apt/sources.list.d/azure-cli.list \
    && apt-get update \
    && apt-get install -y azure-cli \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/web
COPY ["Tests/", "Tests/"]
COPY ["Website/", "Website/"]
WORKDIR /src/web/Website
RUN dotnet restore "DadABase.Web.csproj"
RUN dotnet build "DadABase.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/web/Website
RUN dotnet publish "./DadABase.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DadABase.Web.dll"]