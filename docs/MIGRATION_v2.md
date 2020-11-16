# Migration Guide: v1.x to v2.0

This document covers all breaking changes and required steps when upgrading from v1.x to v2.0.

## Breaking Changes

### 1. Container Port Changed from 80 to 8080

The default container port has been changed from `80` to `8080` to support running as a non-root user.

**Before (v1.x):**
```yaml
ports:
  - "5000:80"
```

**After (v2.0):**
```yaml
ports:
  - "8080:8080"
```

If you use a reverse proxy (Nginx, Caddy, Traefik), update the upstream target port:

```nginx
# Before
proxy_pass http://featureflags-api:80;

# After
proxy_pass http://featureflags-api:8080;
```

**Kubernetes deployments** - update `containerPort`, `livenessProbe`, and `readinessProbe` from port `80` to `8080`.

### 2. Non-Root Container User

The container now runs as `appuser` instead of `root`. This improves security but may affect volume mounts:

- Ensure mounted directories are writable by UID 100 (appuser)
- If you mount custom config files, set appropriate permissions

```bash
# Fix permissions for log volume
chown -R 100:101 ./logs
```

### 3. Docker Compose Schema Update

The `docker-compose.yml` no longer uses `version: '3.8'` (deprecated in Compose v2). If you use Docker Compose v1 (docker-compose), upgrade to v2 (docker compose) or add the version field back manually.

### 4. SQL Server Health Check Updated

The health check command now uses `mssql-tools18` instead of `mssql-tools` to match the current SQL Server 2022 image.

### 5. Environment Variable ASPNETCORE_URLS

If you override `ASPNETCORE_URLS`, use port `8080`:

```bash
# Before
ASPNETCORE_URLS=http://+:80

# After
ASPNETCORE_URLS=http://+:8080
```

## Migration Steps

### Step 1: Update Docker Compose File

Replace your existing `docker-compose.yml` with the new version, or apply the port changes manually.

### Step 2: Update Reverse Proxy Configuration

Change the backend port from `80` to `8080` in your reverse proxy config.

### Step 3: Update CI/CD Pipelines

If your CI/CD pipelines reference the container port (health checks, smoke tests), update them to use `8080`.

### Step 4: Fix Volume Permissions

If you use bind-mounted volumes (e.g., `./logs`), ensure they are writable by the non-root user:

```bash
mkdir -p logs
chmod 777 logs
```

### Step 5: Rebuild and Deploy

```bash
docker compose down
docker compose build --no-cache
docker compose up -d
```

Verify the health endpoint:

```bash
curl http://localhost:8080/health
```

## Non-Breaking Changes (No Action Required)

- `restart: unless-stopped` added to all services
- `DOTNET_EnableDiagnostics=0` set for smaller memory footprint in production
- Published with `/p:UseAppHost=false` for smaller image size
- SQL Server health check `start_period` increased to 30s for slower environments

## Rollback

To roll back to v1.x behavior without downgrading:

1. Change `ASPNETCORE_URLS` back to `http://+:80`
2. Change `EXPOSE` to `80` in Dockerfile
3. Update port mapping to `"5000:80"`
4. Remove `USER appuser` line from Dockerfile

---

For questions, see [Deployment Guide](./deployment.md) or open an issue on GitHub.
