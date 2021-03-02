FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source

COPY Ejector.csproj .
RUN dotnet restore "./Ejector.csproj"

COPY . .
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        tzdata \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "Ejector.dll"]