# =============================================================================
# CITL — Start Infrastructure (Redis + Grafana)
# Usage: .\scripts\start-infra.ps1
# =============================================================================

$composeFile = Join-Path $PSScriptRoot "..\docker-compose.yml"

Write-Host "Starting CITL infrastructure..." -ForegroundColor Cyan

docker compose -f $composeFile up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "docker compose failed. Is Docker Desktop running?" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Services started:" -ForegroundColor Green
Write-Host "  Redis             -> localhost:6379"
Write-Host "  Grafana Dashboard -> http://localhost:3000  (admin / see GF_ADMIN_PASSWORD in .env)"
Write-Host "  OTLP gRPC         -> http://localhost:4317"
Write-Host "  OTLP HTTP         -> http://localhost:4318"
Write-Host ""

docker compose -f $composeFile ps
