# DadABase Web Application - Production Dockerfile
# Build context: src/web
# Build command: docker build -t dadabase-web:latest -f Dockerfile .
# Run command: docker run --rm -p 8000:8080 -p 8001:8081 dadabase-web:latest
# Browse to: http://localhost:8000

# ============================================
# Stage 1: Base runtime image
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Install required dependencies for Azure Identity MSAL runtime
RUN apt-get update && apt-get install -y \
    libdbus-1-3 \
    libx11-6 \
    libglib2.0-0 \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080

# ============================================
# Stage 2: Build image
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy project files for restore (in order of dependencies)
COPY ["Data/DadABase.Data.csproj", "Data/"]
COPY ["Website/DadABase.Web.csproj", "Website/"]

# Restore dependencies
RUN dotnet restore "Website/DadABase.Web.csproj"

# Copy all source files
COPY ["Data/", "Data/"]
COPY ["Website/", "Website/"]

# Build the application
WORKDIR /src/Website
RUN dotnet build "DadABase.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# ============================================
# Stage 3: Publish image
# ============================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "DadABase.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ============================================
# Stage 4: Final runtime image
# ============================================
FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
# Note: HTTPS is disabled in container. Use a reverse proxy (nginx, Azure App Service, etc.) for HTTPS termination.
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "DadABase.Web.dll"]