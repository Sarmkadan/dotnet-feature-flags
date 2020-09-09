# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/FeatureFlags/FeatureFlags.csproj", "src/FeatureFlags/"]
COPY ["src/FeatureFlags.Tests/FeatureFlags.Tests.csproj", "src/FeatureFlags.Tests/"]

RUN dotnet restore "src/FeatureFlags/FeatureFlags.csproj"

COPY . .

WORKDIR "/src/src/FeatureFlags"
RUN dotnet build "FeatureFlags.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FeatureFlags.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app

RUN apk add --no-cache curl

COPY --from=publish /app/publish .

RUN chown -R appuser:appgroup /app

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FeatureFlags.dll"]
