#!/bin/bash

# MathFlow Observability Infrastructure Verification Script
# This script verifies that all required configuration files exist

echo "🔍 Verifying MathFlow Observability Infrastructure Setup..."
echo ""

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Counter for errors
errors=0

# Function to check file existence
check_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✓${NC} $1"
    else
        echo -e "${RED}✗${NC} $1 (missing)"
        ((errors++))
    fi
}

# Check OTEL Collector
echo "📦 OTEL Collector:"
check_file "docker/infra/otel/otel-collector-config.yaml"
echo ""

# Check Loki
echo "📦 Loki:"
check_file "docker/infra/loki/loki-local-config.yml"
check_file "docker/infra/loki/Dockerfile"
echo ""

# Check Prometheus
echo "📦 Prometheus:"
check_file "docker/infra/prometheus/prometheus.yml"
check_file "docker/infra/prometheus/Dockerfile"
echo ""

# Check Grafana
echo "📦 Grafana:"
check_file "docker/infra/grafana/provisioning/datasources/datasources.yml"
check_file "docker/infra/grafana/Dockerfile"
echo ""

# Check Docker Compose
echo "📦 Docker Compose:"
check_file "docker-compose.infra.yml"
echo ""

# Check Documentation
echo "📚 Documentation:"
check_file "docker/infra/README.md"
echo ""

# Summary
if [ $errors -eq 0 ]; then
    echo -e "${GREEN}✅ All files present! Setup is complete.${NC}"
    echo ""
    echo "Next steps:"
    echo "1. Start Docker daemon"
    echo "2. Run: docker compose -f docker-compose.infra.yml up -d"
    echo "3. Access Grafana at http://localhost:3000 (admin/admin)"
    exit 0
else
    echo -e "${RED}❌ Setup incomplete. $errors file(s) missing.${NC}"
    exit 1
fi
