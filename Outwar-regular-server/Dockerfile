FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Outwar-regular-server/Outwar-regular-server.csproj", "Outwar-regular-server/"]
RUN dotnet restore "Outwar-regular-server/Outwar-regular-server.csproj"
COPY . .
WORKDIR "/src/Outwar-regular-server"
RUN dotnet build "Outwar-regular-server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Outwar-regular-server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Outwar-regular-server.dll"]
