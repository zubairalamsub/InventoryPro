# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy Directory.Build.props and Directory.Packages.props
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

# Copy project files
COPY src/InventoryPro.Domain/InventoryPro.Domain.csproj src/InventoryPro.Domain/
COPY src/InventoryPro.Application/InventoryPro.Application.csproj src/InventoryPro.Application/
COPY src/InventoryPro.Infrastructure/InventoryPro.Infrastructure.csproj src/InventoryPro.Infrastructure/
COPY src/InventoryPro.API/InventoryPro.API.csproj src/InventoryPro.API/
COPY src/InventoryPro.Shared/InventoryPro.Shared.csproj src/InventoryPro.Shared/

# Restore dependencies
RUN dotnet restore src/InventoryPro.API/InventoryPro.API.csproj

# Copy everything else
COPY . .

# Build the application
WORKDIR /src/src/InventoryPro.API
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Development stage (for hot reload)
FROM build AS development
WORKDIR /src/src/InventoryPro.API
ENTRYPOINT ["dotnet", "watch", "run", "--no-launch-profile"]

# Production runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryPro.API.dll"]
