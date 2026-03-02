#!/usr/bin/env bash
# =============================================================================
# CITL — Start Script (Bash)
# Starts Grafana LGTM stack (Docker) + WebApi (dotnet run)
# Usage: ./scripts/start.sh
# =============================================================================

set -e
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

echo ""
echo "=== CITL Startup ==="
echo ""

# ---- 1. Start Grafana LGTM stack via Docker Compose ----
echo "[1/2] Starting Grafana LGTM stack..."

docker compose -f "$ROOT/docker-compose.yml" up -d

GRAFANA_STATUS=$(docker ps --filter "name=citl-grafana" --format "{{.Status}}" 2>/dev/null)
if echo "$GRAFANA_STATUS" | grep -q "Up"; then
    echo "  Grafana LGTM is running"
    echo "  Grafana   : http://localhost:3000 (admin / see GF_ADMIN_PASSWORD in .env)"
    echo "  OTLP gRPC : http://localhost:4317"
    echo "  OTLP HTTP : http://localhost:4318"
else
    echo "  WARNING: Grafana container may not be running. Check 'docker ps'"
fi

echo ""

# ---- 2. Start WebApi ----
echo "[2/2] Starting CITL WebApi..."
echo "  Swagger : https://localhost:7001/swagger/index.html"
echo "  Scalar  : https://localhost:7001/scalar"
echo ""

dotnet run --project "$ROOT/src/CITL.WebApi" --launch-profile https
