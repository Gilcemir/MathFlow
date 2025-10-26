# Tech Spec: Reestruturação do Projeto e Implementação de Stack de Observabilidade

**Versão:** 1.0  
**Data:** 2025-10-26  
**Autor:** Tech Team  
**Status:** Em Revisão  
**PRD Relacionado:** `tasks/prd-infra-observability/prd.md`

---

## 1. Visão Geral da Arquitetura

### 1.1 Contexto Técnico

O MathFlow é uma aplicação ASP.NET Core 9.0 Razor Pages que utiliza:
- **Jering.Javascript.NodeJS**: Para executar scripts JavaScript (conversão OMML → MathML)
- **DocumentFormat.OpenXml**: Para processar documentos Word
- **Multi-process Node.js**: 4 processos concorrentes para conversão

### 1.2 Decisões Arquiteturais Principais

| Decisão | Justificativa | Alternativas |
|---------|---------------|--------------|
| Estrutura `src/` | Padrão .NET moderno | Manter na raiz |
| Docker Compose separado | Permite rodar infra sem app | Compose único |
| OpenTelemetry SDK | Padrão CNCF, vendor-neutral | Application Insights |
| Loki para logs | Leve, integra com Grafana | ELK Stack |
| PostgreSQL | Requisito futuro de persistência | SQLite |

---

## 2. Estrutura de Pastas Detalhada

```
MathFlow/
├── src/
│   └── MathFlow/
│       ├── Pages/
│       ├── Services/Converters/
│       ├── Infrastructure/
│       │   ├── Converters/Scripts/
│       │   └── Observability/
│       ├── wwwroot/
│       ├── Program.cs
│       └── MathFlow.csproj
├── tests/
│   ├── MathFlow.UnitTests/
│   └── MathFlow.IntegrationTests/
├── docker/
│   ├── app/
│   │   ├── Dockerfile
│   │   └── .dockerignore
│   └── infra/
│       ├── grafana/
│       ├── loki/
│       ├── prometheus/
│       └── otel/
├── docker-compose.yml
├── docker-compose.infra.yml
├── .env.example
└── README.md
```

---

## 3. Componentes Principais

### 3.1 OpenTelemetryConfigurator

**Localização**: `src/MathFlow/Infrastructure/Observability/OpenTelemetryConfigurator.cs`

**Implementação**:
```csharp
namespace MathFlow.Infrastructure.Observability;

public static class OpenTelemetryConfigurator
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];
        
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "MathFlow", serviceVersion: "1.0.0"))
            .WithLogging(logging => logging
                .AddConsoleExporter()
                .AddOtlpExporter(opts =>
                {
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                        opts.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(opts =>
                {
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                        opts.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithMetrics(metrics => metrics
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter(opts =>
                {
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                        opts.Endpoint = new Uri(otlpEndpoint);
                }));
        
        return builder;
    }
}
```

**Integração no Program.cs**:
```csharp
builder.AddOpenTelemetry();
```

---

## 4. Infraestrutura Docker

### 4.1 Dockerfile da Aplicação

**Localização**: `docker/app/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs

COPY ["src/MathFlow/MathFlow.csproj", "MathFlow/"]
RUN dotnet restore "MathFlow/MathFlow.csproj"

COPY src/MathFlow/ MathFlow/

WORKDIR /src/MathFlow/Infrastructure/Converters/Scripts
RUN npm install

WORKDIR /src/MathFlow
RUN dotnet build "MathFlow.csproj" -c Release -o /app/build
RUN dotnet publish "MathFlow.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 5124
ENTRYPOINT ["dotnet", "MathFlow.dll"]
```

### 4.2 OTEL Collector Config

**Localização**: `docker/infra/otel/otel-collector-config.yaml`

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    timeout: 10s
    send_batch_size: 1024

exporters:
  loki:
    endpoint: http://loki:3100/loki/api/v1/push
    labels:
      resource:
        service.name: "service_name"
  
  prometheus:
    endpoint: 0.0.0.0:8889
    namespace: mathflow
  
  logging:
    loglevel: info

service:
  pipelines:
    logs:
      receivers: [otlp]
      processors: [batch]
      exporters: [loki, logging]
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [logging]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [prometheus, logging]
```

### 4.3 Loki Config

**Localização**: `docker/infra/loki/loki-local-config.yml`

```yaml
auth_enabled: false

server:
  http_listen_port: 3100

ingester:
  lifecycler:
    address: 127.0.0.1
    ring:
      kvstore:
        store: inmemory
      replication_factor: 1
  chunk_idle_period: 5m
  chunk_retain_period: 30s

schema_config:
  configs:
    - from: 2020-10-24
      store: boltdb-shipper
      object_store: filesystem
      schema: v11
      index:
        prefix: index_
        period: 24h

storage_config:
  boltdb_shipper:
    active_index_directory: /loki/boltdb-shipper-active
    cache_location: /loki/boltdb-shipper-cache
    cache_ttl: 24h
    shared_store: filesystem
  filesystem:
    directory: /loki/chunks

limits_config:
  enforce_metric_name: false
  reject_old_samples: true
  reject_old_samples_max_age: 168h

chunk_store_config:
  max_look_back_period: 0s

table_manager:
  retention_deletes_enabled: true
  retention_period: 168h
```

### 4.4 Prometheus Config

**Localização**: `docker/infra/prometheus/prometheus.yml`

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'otel-collector'
    scrape_interval: 5s
    static_configs:
      - targets: ['otel-collector:8889']
      - targets: ['otel-collector:8888']
  
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']
```

### 4.5 Grafana Datasources

**Localização**: `docker/infra/grafana/provisioning/datasources/datasources.yml`

```yaml
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    editable: true
  
  - name: Loki
    type: loki
    access: proxy
    url: http://loki:3100
    editable: true
```

---

## 5. Docker Compose

### 5.1 Compose Completo

**Localização**: `docker-compose.yml`

```yaml
version: '3.8'

networks:
  mathflow-network:
    name: mathflow-network

volumes:
  postgres_data:
  loki_data:
  prometheus_data:

services:
  app:
    build:
      context: .
      dockerfile: docker/app/Dockerfile
    image: mathflow:latest
    container_name: mathflow-app
    ports:
      - "5124:5124"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5124
      - Otlp__Endpoint=http://otel-collector:4317
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
    depends_on:
      - otel-collector
      - postgres
    networks:
      - mathflow-network
    restart: unless-stopped

  postgres:
    image: postgres:16-alpine
    container_name: mathflow-postgres
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - mathflow-network
    restart: unless-stopped

  otel-collector:
    image: otel/opentelemetry-collector-contrib:0.89.0
    container_name: mathflow-otel-collector
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./docker/infra/otel/otel-collector-config.yaml:/etc/otel-collector-config.yaml
    ports:
      - "4317:4317"
      - "8889:8889"
    networks:
      - mathflow-network
    depends_on:
      - loki
    restart: unless-stopped

  loki:
    build:
      context: docker/infra/loki
    container_name: mathflow-loki
    ports:
      - "3100:3100"
    volumes:
      - loki_data:/loki
    networks:
      - mathflow-network
    restart: unless-stopped

  prometheus:
    build:
      context: docker/infra/prometheus
    container_name: mathflow-prometheus
    ports:
      - "9090:9090"
    volumes:
      - prometheus_data:/prometheus
    networks:
      - mathflow-network
    depends_on:
      - otel-collector
    restart: unless-stopped

  grafana:
    build:
      context: docker/infra/grafana
    container_name: mathflow-grafana
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_USER=${GRAFANA_USER:-admin}
      - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA_PASSWORD:-admin}
    networks:
      - mathflow-network
    depends_on:
      - prometheus
      - loki
    restart: unless-stopped
```

---

## 6. Configurações

### 6.1 .env.example

```env
POSTGRES_USER=mathflow
POSTGRES_PASSWORD=mathflow_password_change_me
POSTGRES_DB=mathflow_db
GRAFANA_USER=admin
GRAFANA_PASSWORD=admin
ASPNETCORE_ENVIRONMENT=Development
```

### 6.2 appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Otlp": {
    "Endpoint": "http://localhost:4317"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mathflow_db;Username=mathflow;Password=mathflow_password_change_me"
  }
}
```

---

## 7. Pacotes NuGet

Adicionar ao `MathFlow.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="OpenTelemetry" Version="1.7.0" />
  <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.7.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.Process" Version="0.5.0-beta.4" />
  <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
  <PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.7.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
</ItemGroup>
```

---

## 8. Plano de Implementação

### Fase 1: Reestruturação
1. Criar estrutura `src/`, `tests/`, `docker/`
2. Mover arquivos para `src/MathFlow/`
3. Atualizar caminhos no `.csproj`
4. Testar compilação

### Fase 2: Containerização
1. Criar `docker/app/Dockerfile`
2. Criar `.dockerignore`
3. Testar build da imagem
4. Testar execução do container

### Fase 3: Infraestrutura
1. Criar configs OTEL, Loki, Prometheus, Grafana
2. Criar Dockerfiles para cada serviço
3. Criar `docker-compose.infra.yml`
4. Testar stack de infra isoladamente

### Fase 4: Integração
1. Adicionar pacotes OpenTelemetry
2. Criar `OpenTelemetryConfigurator`
3. Integrar no `Program.cs`
4. Criar `docker-compose.yml` completo
5. Testar telemetria end-to-end

### Fase 5: PostgreSQL
1. Adicionar serviço no compose
2. Configurar connection string
3. Testar conectividade

### Fase 6: Documentação e Automação
1. Atualizar README
2. Criar `.env.example`
3. Criar Makefile com comandos úteis
4. Documentar comandos

---

## 9. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Overhead de performance | Baixa | Médio | Usar sampling, exporters assíncronos |
| Complexidade OTEL | Média | Alto | Usar configs testadas do projeto anterior |
| Incompatibilidade versões | Baixa | Médio | Fixar versões de imagens Docker |

---

## 10. Métricas de Sucesso

- Tempo de setup local < 5 minutos
- Overhead de observabilidade < 5%
- Imagem Docker < 500MB
- 100% das requisições HTTP logadas
- Grafana exibe métricas e logs corretamente

---

## 11. Questões em Aberto - RESOLVIDAS

1. **Retenção de logs**: ✅ Configuração local padrão (7 dias). Produção será configurada no Grafana.
2. **Dashboards Grafana**: ✅ Serão criados em tarefas futuras (métricas: fórmulas/arquivo, requisições/usuário).
3. **Alertas**: ✅ Não serão configurados nesta fase.
4. **Tempo (Distributed Tracing)**: ✅ Não será adicionado por simplicidade.
5. **Migrations PostgreSQL**: ✅ Não há domínio persistido ainda. PostgreSQL configurado mas não utilizado inicialmente.

## 12. Makefile - Comandos Úteis

Criar `Makefile` na raiz do projeto:

```makefile
.PHONY: help build run test docker-infra-up docker-infra-down docker-up docker-down docker-logs clean

help:
	@echo "Comandos disponíveis:"
	@echo "  make build          - Compila a aplicação"
	@echo "  make run            - Executa a aplicação localmente"
	@echo "  make test           - Executa testes"
	@echo "  make docker-infra-up    - Sobe apenas infraestrutura (Postgres, OTEL, Grafana, etc)"
	@echo "  make docker-infra-down  - Para infraestrutura"
	@echo "  make docker-up      - Sobe aplicação + infraestrutura"
	@echo "  make docker-down    - Para tudo"
	@echo "  make docker-logs    - Exibe logs dos containers"
	@echo "  make clean          - Limpa arquivos de build"

build:
	cd src/MathFlow && dotnet build

run:
	cd src/MathFlow && dotnet run

test:
	dotnet test

docker-infra-up:
	docker compose -f docker-compose.infra.yml up -d

docker-infra-down:
	docker compose -f docker-compose.infra.yml down

docker-up:
	docker compose up --build -d

docker-down:
	docker compose down

docker-logs:
	docker compose logs -f

clean:
	find . -type d -name "bin" -o -name "obj" | xargs rm -rf
```

---

## Anexo: Comandos Úteis

### Via Makefile (Recomendado)
```bash
make help                  # Lista todos os comandos
make docker-infra-up       # Sobe apenas infra
make run                   # Roda app localmente
make docker-up             # Sobe tudo
make docker-logs           # Ver logs
```

### Comandos Diretos
```bash
# Reestruturar projeto
mkdir -p src tests/MathFlow.UnitTests tests/MathFlow.IntegrationTests docker/app docker/infra/{grafana,loki,prometheus,otel}
mv Pages Services Infrastructure Program.cs *.csproj appsettings.* wwwroot Properties Shared src/MathFlow/

# Build e run local
cd src/MathFlow
dotnet build
dotnet run

# Docker - apenas infra
docker compose -f docker-compose.infra.yml up -d

# Docker - completo
docker compose up --build -d

# Verificar logs
docker compose logs -f app

# Acessar serviços
# Grafana: http://localhost:3000 (admin/admin)
# Prometheus: http://localhost:9090
# Loki: http://localhost:3100/ready
```
