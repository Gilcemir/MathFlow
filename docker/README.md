# Docker Orchestration

This directory contains Docker orchestration files organized by environment.

## Directory Structure

```
docker/
├── app/                          # Application Dockerfile
├── infra/                        # Infrastructure configurations (OTEL, Loki, Prometheus, Grafana)
├── local/                        # Local development compose files
│   ├── docker-compose.yml        # Application
│   ├── docker-compose.infra.yml  # Infrastructure (PostgreSQL)
│   └── docker-compose.metrics.yml # Observability stack
└── production/                   # Production compose files (future)
    └── (to be created when deploying)
```

## Local Development

### Quick Start

```bash
# From project root
cd docker/local

# Start infrastructure (creates network)
docker compose -f docker-compose.infra.yml up -d

# Start observability
docker compose -f docker-compose.metrics.yml up -d

# Start application
docker compose up -d
```

### Stop Everything

```bash
# From docker/local
docker compose down
docker compose -f docker-compose.metrics.yml down
docker compose -f docker-compose.infra.yml down
```

## Production

Production compose files will be created when needed, with differences such as:

- External database connections (not containerized PostgreSQL)
- External observability services (managed Prometheus/Grafana)
- Different network configurations
- Environment-specific secrets management
- Resource limits and health checks
- Scaling configurations

## Environment Differences

| Aspect | Local | Production |
|--------|-------|------------|
| **PostgreSQL** | Container | External managed service |
| **OTEL Collector** | Container | External service or sidecar |
| **Prometheus** | Container | Managed service (e.g., Grafana Cloud) |
| **Grafana** | Container | Managed service |
| **Network** | Shared `mathflow` | Cloud provider network |
| **Secrets** | `.env` file | Secrets manager (e.g., AWS Secrets Manager) |
| **Scaling** | Single instance | Multiple replicas |

## Best Practices

1. **Never commit `.env` files** - Use `.env.example` as template
2. **Test locally first** - Always test changes in local environment
3. **Document changes** - Update this README when adding new services
4. **Keep environments in sync** - Production should mirror local architecture
5. **Use version tags** - Pin specific versions in production

## Adding New Services

When adding a new service (e.g., Redis, RabbitMQ):

1. Add to appropriate compose file in `local/`
2. Update `.env.example` with new variables
3. Document in `DOCKER_USAGE.md`
4. Create production equivalent when needed

## Related Documentation

- `/DOCKER_USAGE.md` - Detailed usage guide
- `/docker/app/Dockerfile` - Application container definition
- `/docker/infra/` - Infrastructure service configurations
