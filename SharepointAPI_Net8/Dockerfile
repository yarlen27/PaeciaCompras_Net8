# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SharepointAPI_Net8.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install LibreOffice for DOCX to PDF conversion
RUN apt-get update && \
    apt-get install -y libreoffice && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Copy certificate
COPY graphapi-certificate.pfx /app/graphapi-certificate.pfx

# Set environment variables
ENV ASPNETCORE_URLS=http://+:1037
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 1037
ENTRYPOINT ["dotnet", "SharepointAPI_Net8.dll"]