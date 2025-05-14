FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["TimeTrackerApi/TimeTrackerApi.csproj", "TimeTrackerApi/"]
RUN dotnet restore "TimeTrackerApi/TimeTrackerApi.csproj"

COPY . .
WORKDIR "/src/TimeTrackerApi"
RUN dotnet build "TimeTrackerApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TimeTrackerApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TimeTrackerApi.dll"]