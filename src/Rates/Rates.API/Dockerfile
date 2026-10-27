FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/Rates/Rates.API/Rates.API.csproj Rates/Rates.API/
RUN dotnet restore Rates/Rates.API/Rates.API.csproj
COPY src/Rates/Rates.API/ Rates/Rates.API/
RUN dotnet build Rates/Rates.API/Rates.API.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Rates/Rates.API/Rates.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rates.API.dll"]
