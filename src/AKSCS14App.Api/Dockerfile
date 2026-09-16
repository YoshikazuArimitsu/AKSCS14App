# syntax=docker/dockerfile:1

# ビルドコンテキストはリポジトリルート（docker-compose.yml と同じ階層）を想定。
# 例: docker build -f src/AKSCS14App.Api/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/AKSCS14App.Api/AKSCS14App.Api.csproj", "src/AKSCS14App.Api/"]
COPY ["src/AKSCS14App.ServiceDefaults/AKSCS14App.ServiceDefaults.csproj", "src/AKSCS14App.ServiceDefaults/"]
RUN dotnet restore "src/AKSCS14App.Api/AKSCS14App.Api.csproj"

COPY ["src/AKSCS14App.Api/", "src/AKSCS14App.Api/"]
COPY ["src/AKSCS14App.ServiceDefaults/", "src/AKSCS14App.ServiceDefaults/"]

WORKDIR /src/src/AKSCS14App.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AKSCS14App.Api.dll"]
