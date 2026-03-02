# =============================================================================
# CITL — Publish Script (PowerShell / Windows)
# Restores, builds, tests, and publishes for win-x64.
#
# Usage:
#   .\scripts\publish.ps1              → publish to ./publish/
#   .\scripts\publish.ps1 -SkipTests   → skip tests, publish only
# =============================================================================

param(
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent

# ---- Configuration ----
$project      = "$root\src\CITL.WebApi\CITL.WebApi.csproj"
$solution     = "$root\CITL.slnx"
$outputDir    = "$root\publish"
$configuration = "Release"
$runtime      = "win-x64"
$selfContained = $false
$readyToRun   = $true

Write-Host ""
Write-Host "=== CITL Publish (Windows) ===" -ForegroundColor Cyan
Write-Host "  Runtime       : $runtime"
Write-Host "  Configuration : $configuration"
Write-Host "  Output        : $outputDir"
Write-Host "  SelfContained : $selfContained"
Write-Host "  ReadyToRun    : $readyToRun"
Write-Host ""

# ---- 1. Clean previous publish ----
Write-Host "[1/4] Cleaning previous publish..." -ForegroundColor Yellow
if (Test-Path $outputDir)
{
    Remove-Item $outputDir -Recurse -Force
}
Write-Host "  Clean complete" -ForegroundColor Green
Write-Host ""

# ---- 2. Restore ----
Write-Host "[2/4] Restoring packages..." -ForegroundColor Yellow
dotnet restore $solution
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
Write-Host "  Restore complete" -ForegroundColor Green
Write-Host ""

# ---- 3. Test (optional) ----
if (-not $SkipTests)
{
    Write-Host "[3/4] Running tests..." -ForegroundColor Yellow
    dotnet test $solution --no-restore -c $configuration
    if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
    Write-Host "  Tests complete" -ForegroundColor Green
    Write-Host ""
}
else
{
    Write-Host "[3/4] Skipping tests (-SkipTests)" -ForegroundColor DarkGray
    Write-Host ""
}

# ---- 4. Publish ----
Write-Host "[4/4] Publishing..." -ForegroundColor Yellow
dotnet publish $project `
    -c $configuration `
    -r $runtime `
    --self-contained $selfContained `
    -p:PublishReadyToRun=$readyToRun `
    -o $outputDir `
    --no-restore

if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

Write-Host "  Publish complete" -ForegroundColor Green
Write-Host ""

# ---- Summary ----
$fileCount = (Get-ChildItem $outputDir -Recurse -File).Count
$sizeBytes = (Get-ChildItem $outputDir -Recurse -File | Measure-Object -Property Length -Sum).Sum
$sizeMB    = [math]::Round($sizeBytes / 1MB, 2)

Write-Host "=== Publish Succeeded ===" -ForegroundColor Cyan
Write-Host "  Output : $outputDir"
Write-Host "  Files  : $fileCount"
Write-Host "  Size   : $sizeMB MB"
Write-Host ""
