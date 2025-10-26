.PHONY: help build run test docker-infra-up docker-infra-down docker-up docker-down docker-up-all docker-down-all docker-logs docker-restart clean up down logs restart

# Default target
help:
	@echo "╔════════════════════════════════════════════════════════════╗"
	@echo "║              MathFlow - Comandos Disponíveis               ║"
	@echo "╚════════════════════════════════════════════════════════════╝"
	@echo ""
	@echo "📦 Build & Run:"
	@echo "  make build              - Compila a aplicação"
	@echo "  make run                - Executa a aplicação localmente"
	@echo "  make test               - Executa testes"
	@echo "  make clean              - Limpa arquivos de build"
	@echo ""
	@echo "🐳 Docker - Infraestrutura:"
	@echo "  make docker-infra-up    - Sobe apenas infraestrutura (Postgres, OTEL, Grafana, etc)"
	@echo "  make docker-infra-down  - Para infraestrutura"
	@echo ""
	@echo "🐳 Docker - Aplicação Completa:"
	@echo "  make docker-up-all      - Sobe TUDO (infra + metrics + app) 🚀"
	@echo "  make docker-down-all    - Para TUDO (infra + metrics + app)"
	@echo "  make docker-up          - Sobe aplicação + infraestrutura"
	@echo "  make docker-down        - Para tudo"
	@echo "  make docker-restart     - Reinicia aplicação"
	@echo "  make docker-logs        - Exibe logs dos containers"
	@echo ""
	@echo "🔍 Acesso aos Serviços:"
	@echo "  Aplicação:  http://localhost:5124"
	@echo "  Grafana:    http://localhost:3000 (admin/admin)"
	@echo "  Prometheus: http://localhost:9090"
	@echo "  Loki:       http://localhost:3100/ready"
	@echo ""

# ============================================
# Build & Run
# ============================================

build:
	@echo "🔨 Compilando aplicação..."
	cd src/MathFlow && dotnet build

run:
	@echo "🚀 Executando aplicação localmente..."
	@echo "📍 Acesse: http://localhost:5124"
	cd src/MathFlow && dotnet run

test:
	@echo "🧪 Executando testes..."
	dotnet test

clean:
	@echo "🧹 Limpando arquivos de build..."
	find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} +
	@echo "✅ Limpeza concluída"

# ============================================
# Docker - Infraestrutura
# ============================================

docker-infra-up:
	@echo "🐳 Subindo infraestrutura (Postgres, OTEL, Loki, Prometheus, Grafana)..."
	docker compose -f docker/local/docker-compose.infra.yml up -d
	@echo "✅ Infraestrutura iniciada"
	@echo "📍 Grafana: http://localhost:3000 (admin/admin)"
	@echo "📍 Prometheus: http://localhost:9090"

docker-infra-down:
	@echo "🛑 Parando infraestrutura..."
	docker compose -f docker/local/docker-compose.infra.yml down
	@echo "✅ Infraestrutura parada"

# ============================================
# Docker - Stack Completa (Infra + Metrics + App)
# ============================================

docker-up-all:
	@echo "🚀 Subindo TODA a stack (infra + metrics + app)..."
	@echo ""
	@echo "📦 Passo 1/3: Subindo infraestrutura (PostgreSQL)..."
	docker compose -f docker/local/docker-compose.infra.yml up -d
	@echo "✅ Infraestrutura iniciada"
	@echo ""
	@echo "📊 Passo 2/3: Subindo stack de métricas (OTEL, Loki, Prometheus, Grafana)..."
	docker compose -f docker/local/docker-compose.metrics.yml up -d
	@echo "✅ Stack de métricas iniciada"
	@echo ""
	@echo "🎯 Passo 3/3: Subindo aplicação..."
	docker compose -f docker/local/docker-compose.yml up --build -d
	@echo ""
	@echo "╔════════════════════════════════════════════════════════════╗"
	@echo "║              ✅ Stack Completa Iniciada!                   ║"
	@echo "╚════════════════════════════════════════════════════════════╝"
	@echo ""
	@echo "🔍 Serviços disponíveis:"
	@echo "  📱 Aplicação:  http://localhost:5124"
	@echo "  📊 Grafana:    http://localhost:3000 (admin/admin)"
	@echo "  📈 Prometheus: http://localhost:9090"
	@echo "  📋 Loki:       http://localhost:3100/ready"
	@echo ""

docker-down-all:
	@echo "🛑 Parando TODA a stack..."
	docker compose -f docker/local/docker-compose.yml down
	docker compose -f docker/local/docker-compose.metrics.yml down
	docker compose -f docker/local/docker-compose.infra.yml down
	@echo "✅ Stack completa parada"

# ============================================
# Docker - Aplicação Completa
# ============================================

docker-up:
	@echo "🐳 Subindo aplicação + infraestrutura..."
	docker compose -f docker/local/docker-compose.yml up --build -d
	@echo "✅ Stack completa iniciada"
	@echo "📍 Aplicação: http://localhost:5124"
	@echo "📍 Grafana: http://localhost:3000 (admin/admin)"
	@echo "📍 Prometheus: http://localhost:9090"

docker-down:
	@echo "🛑 Parando tudo..."
	docker compose -f docker/local/docker-compose.yml down
	@echo "✅ Stack parada"

docker-restart:
	@echo "🔄 Reiniciando aplicação..."
	docker compose -f docker/local/docker-compose.yml restart app
	@echo "✅ Aplicação reiniciada"

docker-logs:
	@echo "📋 Exibindo logs (Ctrl+C para sair)..."
	docker compose -f docker/local/docker-compose.yml logs -f

# ============================================
# Aliases úteis
# ============================================

up: docker-up
down: docker-down
logs: docker-logs
restart: docker-restart
