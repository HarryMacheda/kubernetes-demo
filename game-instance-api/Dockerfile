FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /game-instance-api

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
RUN dotnet --info
# Build and publish a release
RUN dotnet publish -o out

# Expose the port your application will run on
EXPOSE 8080

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /game-instance-api
COPY --from=build /game-instance-api/out .
ENTRYPOINT ["dotnet", "game-instance-api.dll"]