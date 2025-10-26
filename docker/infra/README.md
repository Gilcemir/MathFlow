# MathFlow Observability Infrastructure

This directory contains the configuration files for the MathFlow observability stack.

## Components

### OTEL Collector
- **Location:** `otel/otel-collector-config.yaml`
- **Purpose:** Receives telemetry data via OTLP protocol and routes it to Loki (logs) and Prometheus (metrics)
- **Ports:**
  - 4317: OTLP gRPC
  - 4318: OTLP HTTP
  - 8888: Prometheus metrics (collector self-monitoring)
  - 8889: Prometheus exporter

### Loki
- **Location:** `loki/`
- **Purpose:** Log aggregation system
- **Configuration:** `loki-local-config.yml`
- **Port:** 3100
- **Retention:** 168 hours (7 days)

### Prometheus
- **Location:** `prometheus/`
- **Purpose:** Metrics collection and storage
- **Configuration:** `prometheus.yml`
- **Port:** 9090
- **Scrape Interval:** 15s (5s for OTEL Collector)

### Grafana
- **Location:** `grafana/`
- **Purpose:** Visualization and dashboards
- **Configuration:** `provisioning/datasources/datasources.yml`
- **Port:** 3000
- **Default Credentials:** admin/admin (configurable via environment variables)

## Testing the Infrastructure Stack

### Prerequisites
- Docker and Docker Compose installed
- Docker daemon running

### Start the Stack
```bash
# From the project root
docker compose -f docker-compose.infra.yml up -d
```

### Verify Services
```bash
# Check container status
docker ps

# Check logs
docker compose -f docker-compose.infra.yml logs

# Verify individual services
curl http://localhost:3100/ready  # Loki
curl http://localhost:9090/-/ready  # Prometheus
curl http://localhost:3000  # Grafana
```

### Access Services
- **Grafana:** http://localhost:3000 (admin/admin)
- **Prometheus:** http://localhost:9090
- **Loki:** http://localhost:3100

### Stop the Stack
```bash
docker compose -f docker-compose.infra.yml down
```

### Clean Up (including volumes)
```bash
docker compose -f docker-compose.infra.yml down -v
```

## Configuration Details

### OTEL Collector Pipeline
- **Logs:** OTLP → Batch → Loki + Console
- **Traces:** OTLP → Batch → Console
- **Metrics:** OTLP → Batch → Prometheus + Console

### Prometheus Scrape Targets
- `otel-collector:8889` - OTEL Collector metrics
- `otel-collector:8888` - OTEL Collector self-monitoring
- `localhost:9090` - Prometheus self-monitoring

### Grafana Datasources
- **Prometheus** (default): http://prometheus:9090
- **Loki**: http://loki:3100

## Environment Variables

### Grafana
- `GRAFANA_USER`: Admin username (default: admin)
- `GRAFANA_PASSWORD`: Admin password (default: admin)

Example:
```bash
GRAFANA_USER=myuser GRAFANA_PASSWORD=mypassword docker compose -f docker-compose.infra.yml up -d
```

## Network

All services are connected via the `mathflow-network` bridge network, allowing them to communicate using service names as hostnames.

## Volumes

Persistent data is stored in Docker volumes:
- `loki-data`: Loki logs and indexes
- `prometheus-data`: Prometheus time-series data
- `grafana-data`: Grafana dashboards and settings
