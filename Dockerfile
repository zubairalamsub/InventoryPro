# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY InventoryPro.sln ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY src/InventoryPro.Domain/InventoryPro.Domain.csproj src/InventoryPro.Domain/
COPY src/InventoryPro.Application/InventoryPro.Application.csproj src/InventoryPro.Application/
COPY src/InventoryPro.Infrastructure/InventoryPro.Infrastructure.csproj src/InventoryPro.Infrastructure/
COPY src/InventoryPro.API/InventoryPro.API.csproj src/InventoryPro.API/
COPY src/InventoryPro.Shared/InventoryPro.Shared.csproj src/InventoryPro.Shared/

# Restore dependencies
RUN dotnet restore src/InventoryPro.API/InventoryPro.API.csproj

# Copy everything else
COPY src/ src/

# Build and publish
RUN dotnet publish src/InventoryPro.API/InventoryPro.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port (Render uses 10000 by default)
EXPOSE 10000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:10000/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "InventoryPro.API.dll"]
