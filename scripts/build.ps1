# =============================================================================
# CITL — Build Script (PowerShell)
# Restores, builds, and runs all tests.
# Usage: .\scripts\build.ps1
# =============================================================================

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent

Write-Host ""
Write-Host "=== CITL Build ===" -ForegroundColor Cyan
Write-Host ""

# ---- 1. Restore ----
Write-Host "[1/3] Restoring packages..." -ForegroundColor Yellow
dotnet restore "$root\CITL.slnx"
Write-Host "  Restore complete" -ForegroundColor Green
Write-Host ""

# ---- 2. Build ----
Write-Host "[2/3] Building solution..." -ForegroundColor Yellow
dotnet build "$root\CITL.slnx" --no-restore -c Debug
Write-Host "  Build complete" -ForegroundColor Green
Write-Host ""

# ---- 3. Test ----
Write-Host "[3/3] Running tests..." -ForegroundColor Yellow
dotnet test "$root\CITL.slnx" --no-build -c Debug
Write-Host "  Tests complete" -ForegroundColor Green
Write-Host ""

Write-Host "=== Build Succeeded ===" -ForegroundColor Cyan
Write-Host ""
