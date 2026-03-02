# =============================================================================
# CITL — Start Script (PowerShell)
# Starts Grafana LGTM stack (Docker) + WebApi (dotnet run)
# Usage: .\scripts\start.ps1
# =============================================================================

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent

# Ensure Docker is in PATH (Docker Desktop may not propagate to all sessions)
$dockerPath = "C:\Program Files\Docker\Docker\resources\bin"
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    if (Test-Path "$dockerPath\docker.exe") {
        $env:PATH = "$dockerPath;$env:PATH"
    }
    else {
        Write-Host "ERROR: Docker not found. Install Docker Desktop or add it to PATH." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "=== CITL Startup ===" -ForegroundColor Cyan
Write-Host ""

# ---- 1. Start Grafana LGTM stack via Docker Compose ----
Write-Host "[1/2] Starting Grafana LGTM stack..." -ForegroundColor Yellow

$ErrorActionPreference = "Continue"
docker compose -f "$root\docker-compose.yml" up -d 2>&1 | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkGray }
$ErrorActionPreference = "Stop"

$grafanaStatus = docker ps --filter "name=citl-grafana" --format "{{.Status}}" 2>$null
if ($grafanaStatus -match "Up") {
    Write-Host "  Grafana LGTM is running" -ForegroundColor Green
    Write-Host "  Grafana   : http://localhost:3000 (admin / see GF_ADMIN_PASSWORD in .env)" -ForegroundColor Gray
    Write-Host "  OTLP gRPC : http://localhost:4317" -ForegroundColor Gray
    Write-Host "  OTLP HTTP : http://localhost:4318" -ForegroundColor Gray
}
else {
    Write-Host "  WARNING: Grafana container may not be running. Check 'docker ps'" -ForegroundColor Red
}

Write-Host ""

# ---- 2. Start WebApi ----
Write-Host "[2/2] Starting CITL WebApi..." -ForegroundColor Yellow

# Kill any previous instance
Stop-Process -Name "CITL.WebApi" -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

Write-Host "  Swagger : https://localhost:7001/swagger/index.html" -ForegroundColor Gray
Write-Host "  Scalar  : https://localhost:7001/scalar" -ForegroundColor Gray
Write-Host ""

dotnet run --project "$root\src\CITL.WebApi" --launch-profile https
