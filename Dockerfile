# Use .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7998
EXPOSE 7999

# Use .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["API/API.csproj", "API/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["AspNetCore.Identity.MongoDB/AspNetCore.Identity.MongoDB.csproj", "AspNetCore.Identity.MongoDB/"]
COPY ["Cobalto.MongoDB.Core/Cobalto.MongoDB.Core.csproj", "Cobalto.MongoDB.Core/"]
COPY ["Cobalto.SQL.Core/Cobalto.SQL.Core.csproj", "Cobalto.SQL.Core/"]

RUN dotnet restore "API/API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy email templates
COPY --from=build /src/Core/EmailTemplates ./EmailTemplates/

ENTRYPOINT ["dotnet", "API.dll"]