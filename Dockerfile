FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY /src .
RUN dotnet restore "CrowdParlay.Social.Api/CrowdParlay.Social.Api.csproj"
RUN dotnet publish "CrowdParlay.Social.Api/CrowdParlay.Social.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
COPY /neo4j/migrations ./neo4j/migrations

RUN apt-get update && apt-get install -y curl unzip
RUN curl -LO https://github.com/michael-simons/neo4j-migrations/releases/download/2.9.3/neo4j-migrations-2.9.3-linux-x86_64.zip
RUN unzip -j neo4j-migrations-2.9.3-linux-x86_64.zip neo4j-migrations-2.9.3-linux-x86_64/bin/neo4j-migrations
RUN rm neo4j-migrations-2.9.3-linux-x86_64.zip

ENTRYPOINT ["sh", "-c", "./neo4j-migrations -a $NEO4J_URI -u $NEO4J_USERNAME -p $NEO4J_PASSWORD migrate && dotnet CrowdParlay.Social.Api.dll"]
