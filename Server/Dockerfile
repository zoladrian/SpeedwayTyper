FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY SpeedwayTyperApp.sln ./
COPY Client/ ./Client/
COPY Server/ ./Server/
COPY Shared/ ./Shared/

RUN dotnet restore Server/SpeedwayTyperApp.Server.csproj
RUN dotnet publish Server/SpeedwayTyperApp.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SpeedwayTyperApp.Server.dll"]
