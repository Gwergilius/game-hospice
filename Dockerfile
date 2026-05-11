FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY GameHospice.slnx .
COPY src/GameHospice.Api/GameHospice.Api.csproj src/GameHospice.Api/
COPY src/GameHospice.Web/GameHospice.Web.csproj src/GameHospice.Web/
RUN dotnet restore src/GameHospice.Api/GameHospice.Api.csproj

COPY src/ src/
RUN dotnet publish src/GameHospice.Api/GameHospice.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GameHospice.Api.dll"]
