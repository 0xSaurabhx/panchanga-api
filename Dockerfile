# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/PanchangaApi/PanchangaApi.csproj", "src/PanchangaApi/"]
COPY ["src/PanchangaApi.Core/PanchangaApi.Core.csproj", "src/PanchangaApi.Core/"]
RUN dotnet restore "src/PanchangaApi/PanchangaApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/PanchangaApi"
RUN dotnet build "PanchangaApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PanchangaApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy the Sanskrit names file
COPY sanskrit-names.json .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PanchangaApi.dll"]
