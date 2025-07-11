﻿FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY /src .
RUN dotnet restore "CrowdParlay.Social.Api/CrowdParlay.Social.Api.csproj"
RUN dotnet publish "CrowdParlay.Social.Api/CrowdParlay.Social.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["sh", "-c", "dotnet CrowdParlay.Social.Api.dll"]
