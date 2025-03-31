# Use a smaller base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 11399

# Use a smaller SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy only project file first (to cache dependencies)
COPY Outwar-regular-server/Outwar-regular-server.csproj ./
RUN dotnet restore Outwar-regular-server.csproj

# Copy the rest of the project and build
COPY . .
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

# Final runtime image
FROM runtime AS final
WORKDIR /app
COPY --from=build /app .

# Ensure Data folder is copied if it's missing  Explicitly copy the Data folder to the app directory
COPY Outwar-regular-server/Data/ /app/Data/

CMD ["dotnet", "Outwar-regular-server.dll"]
