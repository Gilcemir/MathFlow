# MathFlow

**MathFlow** is a modern ASP.NET Core 9.0 web application for mathematical document processing and conversion, featuring comprehensive observability with OpenTelemetry, Prometheus, Grafana, and Loki.

## 🚀 Features

- **Document Conversion**: Convert mathematical documents between formats using Node.js integration
- **Full Observability Stack**: Integrated telemetry with logs, traces, and metrics
- **Containerized Architecture**: Docker-based deployment with PostgreSQL database
- **Health Checks**: Application and database health monitoring
- **Modern Tech Stack**: Built with .NET 9.0, OpenTelemetry, and industry best practices

## 📁 Project Structure

```
MathFlow/
├── src/
│   └── MathFlow/                    # Main application
│       ├── Infrastructure/          # Infrastructure layer
│       │   ├── Converters/         # Document conversion logic
│       │   │   └── Scripts/        # Node.js conversion scripts
│       │   └── Observability/      # OpenTelemetry configuration
│       ├── Pages/                  # Razor Pages
│       └── Properties/             # Application properties
├── tests/
│   ├── MathFlow.UnitTests/         # Unit tests
│   └── MathFlow.IntegrationTests/  # Integration tests
├── docker/
│   ├── app/                        # Application Dockerfile
│   ├── infra/                      # Infrastructure configs (OTEL, Loki, Prometheus, Grafana)
│   ├── local/                      # Local development compose files
│   └── production/                 # Production templates
├── tasks/                          # Project tasks and documentation
├── Makefile                        # Build and deployment commands
└── README.md                       # This file
```

## 🛠️ Prerequisites

Before running MathFlow, ensure you have the following installed:

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker** and **Docker Compose** - [Download](https://www.docker.com/products/docker-desktop)
- **Make** (optional, for convenience commands) - Usually pre-installed on macOS/Linux
- **Node.js v22.16.0** (for local development) - [Download](https://nodejs.org/)

## 🏃 Quick Start

### Using Makefile (Recommended)

The easiest way to run MathFlow is using the provided Makefile:

```bash
# View all available commands
make help

# Start EVERYTHING (infrastructure + metrics + application) - RECOMMENDED
make docker-up-all

# View logs
make docker-logs

# Stop everything
make docker-down-all
```

### Manual Commands

If you prefer not to use Make:

```bash
# Start infrastructure only (PostgreSQL)
docker compose -f docker/local/docker-compose.infra.yml up -d

# Start observability stack (Prometheus, Grafana, Loki)
docker compose -f docker/local/docker-compose.metrics.yml up -d

# Start application
docker compose -f docker/local/docker-compose.yml up --build -d
```

### Local Development (Without Docker)

```bash
# Build the application
make build
# or
cd src/MathFlow && dotnet build

# Run the application
make run
# or
cd src/MathFlow && dotnet run

# Run tests
make test
# or
dotnet test
```

## 📋 Available Make Commands

### Build & Run
- `make build` - Compile the application
- `make run` - Run the application locally
- `make test` - Execute tests
- `make clean` - Clean build artifacts (bin/obj folders)

### Docker - Infrastructure
- `make docker-infra-up` - Start only infrastructure (PostgreSQL)
- `make docker-infra-down` - Stop infrastructure

### Docker - Complete Stack (Recommended)
- `make docker-up-all` - Start EVERYTHING (infra + metrics + app) 🚀
- `make docker-down-all` - Stop EVERYTHING (infra + metrics + app)

### Docker - Application Only
- `make docker-up` - Start application + infrastructure
- `make docker-down` - Stop application
- `make docker-restart` - Restart application container
- `make docker-logs` - Display container logs (Ctrl+C to exit)

### Aliases
- `make up` - Alias for `docker-up`
- `make down` - Alias for `docker-down`
- `make logs` - Alias for `docker-logs`
- `make restart` - Alias for `docker-restart`

## 🔍 Accessing Services

Once the stack is running, you can access the following services:

| Service | URL | Credentials |
|---------|-----|-------------|
| **Application** | http://localhost:5124 | N/A |
| **Grafana** | http://localhost:3000 | admin / admin |
| **Prometheus** | http://localhost:9090 | N/A |
| **Loki** | http://localhost:3100/ready | N/A |
| **PostgreSQL** | localhost:5432 | postgres / postgres |

### Health Checks

- **Application Health**: http://localhost:5124/health
- **Database Health**: http://localhost:5124/health/database
- **Prometheus Metrics**: http://localhost:5124/metrics

## 🏗️ Architecture & Observability

### Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL 17
- **Telemetry**: OpenTelemetry (OTLP)
- **Metrics**: Prometheus
- **Logs**: Loki
- **Visualization**: Grafana
- **Document Processing**: Node.js v22.16.0 + DocumentFormat.OpenXml

### Observability Components

MathFlow implements the **three pillars of observability**:

1. **Logs**: Structured logging exported to Loki via OTEL Collector
2. **Traces**: Distributed tracing for request flow analysis
3. **Metrics**: Application and runtime metrics exposed to Prometheus

#### OpenTelemetry Configuration

The application uses OpenTelemetry with the following instrumentations:

- ASP.NET Core (HTTP requests)
- HTTP Client (outgoing requests)
- Runtime metrics (GC, threads, memory)
- Process metrics (CPU, memory usage)

#### Grafana Dashboards

Pre-configured dashboards available in Grafana:

- **Application Overview**: Request rates, latency, error rates
- **Infrastructure**: CPU, memory, disk usage
- **Logs Explorer**: Real-time log analysis with Loki

### Docker Architecture

The application uses a **multi-stage Docker build**:

1. **Build Stage**: .NET SDK + Node.js for compilation
2. **Runtime Stage**: Minimal Alpine-based runtime image
3. **Security**: Non-root user (appuser:1001)
4. **Size**: ~425MB final image

## 🧪 Testing

### Running Tests

```bash
# Run all tests
make test

# Run specific test project
dotnet test tests/MathFlow.UnitTests
dotnet test tests/MathFlow.IntegrationTests
```

### End-to-End Testing

To validate the complete stack:

```bash
# 1. Start the stack
make docker-up

# 2. Wait for initialization (30 seconds)
sleep 30

# 3. Test application
curl http://localhost:5124

# 4. Test Grafana
curl http://localhost:3000

# 5. Test Prometheus
curl http://localhost:9090/-/ready

# 6. Test Loki
curl http://localhost:3100/ready

# 7. Stop the stack
make docker-down
```

## 🔧 Configuration

### Environment Variables

Create a `.env` file in the project root (use `.env.example` as template):

```bash
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=mathflow

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5124

# OpenTelemetry
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_SERVICE_NAME=mathflow

# Observability
GRAFANA_ADMIN_PASSWORD=admin
```

### Application Settings

Configuration files are located in `src/MathFlow/`:

- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development overrides

## 🐛 Troubleshooting

### Common Issues

#### Application does not start

```bash
# Check container logs
make docker-logs

# Verify all containers are running
docker ps

# Restart the application
make docker-restart
```

#### Cannot connect to database

```bash
# Verify PostgreSQL is running
docker ps | grep postgres

# Check database logs
docker logs mathflow-postgres

# Restart infrastructure
make docker-infra-down
make docker-infra-up
```

#### Grafana shows no data

1. Verify Prometheus is scraping metrics: http://localhost:9090/targets
2. Check Loki is receiving logs: http://localhost:3100/ready
3. Ensure OTEL Collector is running: `docker ps | grep otel`
4. Generate some traffic to the application: `curl http://localhost:5124`

#### Port conflicts

If ports are already in use, you can modify them in the docker-compose files:

- Application: `docker/local/docker-compose.yml`
- Infrastructure: `docker/local/docker-compose.infra.yml`
- Metrics: `docker/local/docker-compose.metrics.yml`

#### Clean slate restart

```bash
# Stop everything
make docker-down
make docker-infra-down

# Remove volumes (WARNING: deletes data)
docker volume prune -f

# Start fresh
make docker-up
```

## 🤝 Contributing

### Development Workflow

1. Create a feature branch
2. Make your changes
3. Run tests: `make test`
4. Build locally: `make build`
5. Test with Docker: `make docker-up`
6. Submit a pull request

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Write unit tests for new features
- Keep methods focused and concise

### Commit Messages

Use conventional commits format:

```
feat: add new document conversion format
fix: resolve memory leak in converter
docs: update README with new endpoints
test: add integration tests for API
chore: update dependencies
```

## 📚 Additional Documentation

- **Docker Usage**: See `docker/README.md` for detailed Docker information
- **Task Documentation**: See `tasks/prd-infra-observability/` for implementation details
- **Tech Spec**: See `tasks/prd-infra-observability/techspec.md` for technical specifications
- **PRD**: See `tasks/prd-infra-observability/prd.md` for product requirements

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- Observability powered by [OpenTelemetry](https://opentelemetry.io/)
- Metrics by [Prometheus](https://prometheus.io/)
- Logs by [Grafana Loki](https://grafana.com/oss/loki/)
- Visualization by [Grafana](https://grafana.com/)

---

**MathFlow** - Modern mathematical document processing with enterprise-grade observability.
