# Multi-stage build for the identity-server
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Copy solution and project files
COPY ["game-instance-api.sln", "./"]
COPY ["identity-server/identity-server.csproj", "identity-server/"]
COPY ["standard/standard.csproj", "standard/"]
COPY ["game-manager-api/game-manager-api.csproj", "game-manager-api/"]
COPY ["game-instance-api/game-instance-api.csproj", "game-instance-api/"]
COPY ["quiz-api/quiz-api.csproj", "quiz-api/"]

# Restore dependencies (from solution to handle all projects)
RUN dotnet restore "game-instance-api.sln"

# Copy source code
COPY ["identity-server/", "identity-server/"]
COPY ["standard/", "standard/"]
COPY ["game-manager-api/", "game-manager-api/"]
COPY ["game-instance-api/", "game-instance-api/"]
COPY ["quiz-api/", "quiz-api/"]

# Build the project
RUN dotnet build "quiz-api/quiz-api.csproj" -c Release -o /app/build

# Publish the project
RUN dotnet publish "quiz-api/quiz-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose ports
EXPOSE 80 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD dotnet-healthcheck || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "quiz-api.dll"]
