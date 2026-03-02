#!/usr/bin/env bash
# =============================================================================
# CITL â€” Start Infrastructure (Redis + Grafana)
# Usage: ./scripts/start-infra.sh
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/../docker-compose.yml"

echo "Starting CITL infrastructure..."

docker compose -f "$COMPOSE_FILE" up -d

echo ""
echo "Services started:"
echo "  Redis             -> localhost:6379"
echo "  Grafana Dashboard -> http://localhost:3000  (admin / see GF_ADMIN_PASSWORD in .env)"
echo "  OTLP gRPC         -> http://localhost:4317"
echo "  OTLP HTTP         -> http://localhost:4318"
echo ""

docker compose -f "$COMPOSE_FILE" ps
