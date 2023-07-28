FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy project files
COPY /src .
# Restore as distinct layers
RUN dotnet restore "CrowdParlay.Social.Api/CrowdParlay.Social.Api.csproj"
# Build and publish a release
RUN dotnet publish "CrowdParlay.Social.Api/CrowdParlay.Social.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "CrowdParlay.Social.Api.dll"]