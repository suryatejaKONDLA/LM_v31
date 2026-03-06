<#
.SYNOPSIS
    Pulls the full SQL schema from a tenant database into per-object .sql script files.

.DESCRIPTION
    Uses Microsoft's `sqlpackage` CLI to extract every database object
    (tables, views, stored procedures, functions, triggers, indexes, constraints, etc.)
    into a folder structure organized by object type.

    Connection settings are read from appsettings.Development.json.
    Output goes to src/CITL.Infrastructure/Migrations/Pulled/{TenantId}/.

.PARAMETER TenantId
    The tenant key from TenantMappings (e.g., "CITLPOS", "CITLOGO").
    If omitted, extracts schema for ALL registered tenants.

.EXAMPLE
    .\scripts\pull-schema.ps1 -TenantId CITLPOS

.EXAMPLE
    .\scripts\pull-schema.ps1
    # Pulls schema for every tenant in TenantMappings.
#>

param(
    [string]$TenantId
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Resolve paths ──────────────────────────────────────────────────────────────
$repoRoot = Split-Path -Parent $PSScriptRoot
$configPath = Join-Path $repoRoot 'src/CITL.WebApi/appsettings.Development.json'
$outputRoot = Join-Path $repoRoot 'src/CITL.Infrastructure/Migrations/Pulled'

if (-not (Test-Path $configPath)) {
    Write-Error "Config file not found: $configPath"
    exit 1
}

# ── Verify sqlpackage is installed ─────────────────────────────────────────────
if (-not (Get-Command sqlpackage -ErrorAction SilentlyContinue)) {
    Write-Host 'sqlpackage not found. Installing...' -ForegroundColor Yellow
    dotnet tool install -g microsoft.sqlpackage
}

# ── Read configuration (appsettings + user secrets overlay) ────────────────────
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$mt = $config.MultiTenancy
$template = $mt.ConnectionStringTemplate
$mappings = $mt.TenantMappings

# Overlay user secrets (secrets.json takes precedence over appsettings)
$userSecretsId = '7edbee65-f437-4353-9b41-f0c6ce374469'
$userSecretsBase = if ($env:APPDATA) {
    # Windows
    Join-Path $env:APPDATA 'Microsoft\UserSecrets'
}
else {
    # Linux / macOS
    Join-Path $env:HOME '.microsoft/usersecrets'
}
$userSecretsPath = Join-Path $userSecretsBase "$userSecretsId\secrets.json"
if (Test-Path $userSecretsPath) {
    $secrets = Get-Content $userSecretsPath -Raw | ConvertFrom-Json
    if ($secrets.MultiTenancy.ConnectionStringTemplate) {
        $template = $secrets.MultiTenancy.ConnectionStringTemplate
    }
}

if (-not $template) {
    Write-Error "MultiTenancy:ConnectionStringTemplate is not set. Run:`n  dotnet user-secrets set `"MultiTenancy:ConnectionStringTemplate`" `"Server=...`""
    exit 1
}

# ── Determine which tenants to extract ─────────────────────────────────────────
if ($TenantId) {
    $dbName = $mappings.$TenantId
    if (-not $dbName) {
        Write-Error "Tenant '$TenantId' not found in TenantMappings. Available: $($mappings.PSObject.Properties.Name -join ', ')"
        exit 1
    }
    $tenants = @(@{ TenantId = $TenantId; DatabaseName = $dbName })
}
else {
    $tenants = $mappings.PSObject.Properties | ForEach-Object {
        @{ TenantId = $_.Name; DatabaseName = $_.Value }
    }
    Write-Host "Extracting schema for ALL tenants: $($tenants.TenantId -join ', ')" -ForegroundColor Cyan
}

# ── Extract each tenant ───────────────────────────────────────────────────────
foreach ($tenant in $tenants) {
    $tid = $tenant.TenantId
    $dbName = $tenant.DatabaseName
    $connStr = $template.Replace('{dbName}', $dbName)
    $outDir = Join-Path $outputRoot $tid

    # Clean previous extract
    if (Test-Path $outDir) {
        Remove-Item $outDir -Recurse -Force
    }

    Write-Host "`n── Extracting: $tid (db: $dbName) ──" -ForegroundColor Green

    sqlpackage `
        /Action:Extract `
        /SourceConnectionString:"$connStr" `
        /TargetFile:"$outDir" `
        /p:ExtractTarget=SchemaObjectType `
        /p:VerifyExtraction=false `
        /p:ExtractReferencedServerScopedElements=false `
        /p:ExtractAllTableData=false

    if ($LASTEXITCODE -ne 0) {
        Write-Error "sqlpackage failed for tenant '$tid' (exit code: $LASTEXITCODE)"
        continue
    }

    # Count extracted files
    $fileCount = (Get-ChildItem $outDir -Recurse -File -Filter '*.sql' | Measure-Object).Count
    Write-Host "  Done: $fileCount SQL files → $outDir" -ForegroundColor Green
}

Write-Host "`nSchema extraction complete." -ForegroundColor Cyan
