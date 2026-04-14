FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DadJokeMCPSSE/DadJokeMCPSSE.csproj", "DadJokeMCPSSE/"]
COPY ["DadJokeMCP.Shared/DadJokeMCP.Shared.csproj", "DadJokeMCP.Shared/"]
RUN dotnet restore "DadJokeMCPSSE/DadJokeMCPSSE.csproj"

COPY . .
WORKDIR "/src/DadJokeMCPSSE"
RUN dotnet publish "DadJokeMCPSSE.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:PublishAot=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:3001
EXPOSE 3001

ENTRYPOINT ["dotnet", "DadJokeMCPSSE.dll"]