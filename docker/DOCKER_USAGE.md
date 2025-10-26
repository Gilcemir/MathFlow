# Docker Compose Usage Guide

## Directory Structure

```
docker/
├── app/                          # Application Dockerfile
├── infra/                        # Infrastructure configurations
├── local/                        # 🔧 Local development (YOU ARE HERE)
│   ├── docker-compose.yml        # Application
│   ├── docker-compose.infra.yml  # Infrastructure
│   └── docker-compose.metrics.yml # Observability
└── production/                   # 🚀 Production (future)
    └── (templates for production deployment)
```

**All commands in this guide assume you are in the `docker/local/` directory.**

---

## Architecture Overview

The MathFlow project uses a **three-tier Docker Compose architecture** with a shared network that separates concerns while maintaining proper container communication:

### 1. **docker-compose.infra.yml** - Infrastructure Layer (Base)
- **Creates the shared `mathflow` network**
- Contains infrastructure services (PostgreSQL, future: Redis, Mailpit, etc.)
- Must be started first to create the network
- Persistent data storage

### 2. **docker-compose.metrics.yml** - Observability Layer
- Uses `external: true` to connect to the `mathflow` network
- Contains the complete observability stack
- OpenTelemetry Collector, Loki, Prometheus, Grafana
- Independent monitoring infrastructure

### 3. **docker-compose.yml** - Application Layer
- Uses `external: true` to connect to the `mathflow` network
- Contains only the MathFlow application
- Communicates with other services via container names (e.g., `mathflow-postgres`, `mathflow-otel-collector`)
- Proper Docker networking (not host mode)

---

## Quick Start

**Note:** All compose files are located in `docker/local/` directory.

### Start Everything

```bash
# Navigate to local environment
cd docker/local

# Start infrastructure (creates network)
docker compose -f docker-compose.infra.yml up -d

# Start observability stack
docker compose -f docker-compose.metrics.yml up -d

# Start application
docker compose up -d
```

### Stop Everything

```bash
# From docker/local directory
docker compose down
docker compose -f docker-compose.metrics.yml down
docker compose -f docker-compose.infra.yml down
```

---

## Service URLs

| Service | URL | Default Credentials |
|---------|-----|---------------------|
| **Application** | http://localhost:5124 | - |
| **Grafana** | http://localhost:3000 | admin / admin |
| **Prometheus** | http://localhost:9090 | - |
| **PostgreSQL** | localhost:5432 | mathflow / mathflow_password_change_me |
| **OTLP Collector** | localhost:4317 (gRPC), localhost:4318 (HTTP) | - |
| **Loki** | http://localhost:3100 | - |

---

## Environment Configuration

Copy `.env.example` to `.env` and customize:

```bash
cp .env.example .env
```

### Key Variables

```env
# Application
ASPNETCORE_ENVIRONMENT=Development

# OpenTelemetry
# Use container name for Docker network communication
OTLP_ENDPOINT=http://mathflow-otel-collector:4317

# PostgreSQL
# Use container name for Docker network communication
POSTGRES_HOST=mathflow-postgres
POSTGRES_PORT=5432
POSTGRES_USER=mathflow
POSTGRES_PASSWORD=mathflow_password_change_me
POSTGRES_DB=mathflow_db

# Grafana
GRAFANA_USER=admin
GRAFANA_PASSWORD=admin
```

**Important:** The default values use container names (`mathflow-postgres`, `mathflow-otel-collector`) which work within the Docker network. For production, these would be replaced with actual hostnames or IPs.

---

## Common Commands

### Build Application Image

```bash
# From docker/local directory
docker compose build
```

### View Logs

```bash
# Application logs
docker compose logs -f app

# Infrastructure logs
docker compose -f docker-compose.infra.yml logs -f postgres

# Observability logs
docker compose -f docker-compose.metrics.yml logs -f otel-collector
docker compose -f docker-compose.metrics.yml logs -f grafana
```

### Check Status

```bash
docker ps
```

### Rebuild and Restart Application

```bash
# From docker/local directory
docker compose down
docker compose build
docker compose up -d
```

---

## Development Workflow

### Typical Development Session

1. **Navigate to local environment**:
   ```bash
   cd docker/local
   ```

2. **Start infrastructure and observability** (once per day):
   ```bash
   docker compose -f docker-compose.infra.yml up -d
   docker compose -f docker-compose.metrics.yml up -d
   ```

3. **Develop and test application** (multiple times):
   ```bash
   # Make code changes
   docker compose down
   docker compose build
   docker compose up -d
   
   # Check logs
   docker compose logs -f app
   ```

4. **End of day cleanup**:
   ```bash
   docker compose down
   docker compose -f docker-compose.metrics.yml down
   docker compose -f docker-compose.infra.yml down
   ```

---

## Network Architecture

The architecture uses a **shared Docker network** for proper container communication:

1. **Shared `mathflow` network**
   - Created by `docker-compose.infra.yml`
   - Used by all three compose files
   - Enables DNS resolution between containers

2. **Container name resolution**
   - Application connects to `mathflow-postgres` (not `localhost`)
   - Application connects to `mathflow-otel-collector` (not `localhost`)
   - Proper Docker networking with service discovery

3. **Configurable endpoints via environment variables**
   - `OTLP_ENDPOINT` defaults to `http://mathflow-otel-collector:4317`
   - `POSTGRES_HOST` defaults to `mathflow-postgres`
   - Can be overridden for production environments

4. **Independent compose files**
   - Each layer can be deployed separately
   - All connect to the same network
   - Mirrors microservices architecture

### Network Dependency Order

**Critical:** You must start services in this order:

1. **First:** `docker-compose.infra.yml` (creates the network)
2. **Second:** `docker-compose.metrics.yml` (joins the network)
3. **Third:** `docker-compose.yml` (joins the network)

---

## Troubleshooting

### Application cannot connect to PostgreSQL

```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# Check PostgreSQL logs
docker compose -f docker-compose.infra.yml logs postgres

# Test connection from application container
docker exec mathflow-app ping -c 2 mathflow-postgres

# Verify database exists
docker exec mathflow-postgres psql -U mathflow -d mathflow_db -c '\l'
```

### Application cannot send telemetry

```bash
# Check if OTEL Collector is running
docker ps | grep otel-collector

# Check OTEL Collector logs
docker compose -f docker-compose.metrics.yml logs otel-collector

# Test connection from application container
docker exec mathflow-app ping -c 2 mathflow-otel-collector

# Verify network connectivity
docker network inspect mathflow
```

### Port conflicts

```bash
# Check which process is using a port
lsof -i :5124
lsof -i :5432
lsof -i :4317

# Kill the process or change the port in docker-compose.yml
```

### Clean slate (remove all data)

```bash
# Stop all services
docker compose down
docker compose -f docker-compose.metrics.yml down
docker compose -f docker-compose.infra.yml down

# Remove volumes (WARNING: deletes all data)
docker volume rm mathflow_postgres-data
docker volume rm mathflow_loki-data
docker volume rm mathflow_prometheus-data
docker volume rm mathflow_grafana-data
```

---

## Future Enhancements

The infrastructure layer (`docker-compose.infra.yml`) is ready for additional services:

- **Redis**: Caching layer
- **Mailpit**: Email testing
- **RabbitMQ/Kafka**: Message queuing
- **MinIO**: Object storage

Simply add them to `docker-compose.infra.yml` and update `.env.example`.
