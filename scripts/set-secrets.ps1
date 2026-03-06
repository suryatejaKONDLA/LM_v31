<#
.SYNOPSIS
    Interactively sets all required dotnet user-secrets for CITL.WebApi.

.DESCRIPTION
    Prompts for each secret value and writes them via `dotnet user-secrets set`.
    Press Enter to skip a secret and keep the existing value.

.EXAMPLE
    .\scripts\set-secrets.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot '..\src\CITL.WebApi'

if (-not (Test-Path $projectPath)) {
    Write-Error "Project not found: $projectPath"
    exit 1
}

Push-Location $projectPath

try {
    Write-Host ''
    Write-Host '╔══════════════════════════════════════════════════════════╗' -ForegroundColor Cyan
    Write-Host '║           CITL — User Secrets Setup                     ║' -ForegroundColor Cyan
    Write-Host '║  Press Enter to skip (keeps existing value)             ║' -ForegroundColor Cyan
    Write-Host '╚══════════════════════════════════════════════════════════╝' -ForegroundColor Cyan
    Write-Host ''

    function Set-Secret {
        param(
            [string]$Key,
            [string]$Prompt,
            [bool]$IsPassword = $false
        )

        if ($IsPassword) {
            $secureInput = Read-Host -Prompt $Prompt -AsSecureString
            $value = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
                [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureInput)
            )
        }
        else {
            $value = Read-Host -Prompt $Prompt
        }

        if ($value) {
            dotnet user-secrets set $Key $value | Out-Null
            Write-Host "  ✔ $Key" -ForegroundColor Green
        }
        else {
            Write-Host "  – $Key (skipped)" -ForegroundColor DarkGray
        }
    }

    # ── SQL Server ──────────────────────────────────────────────────────────────
    Write-Host '[ SQL Server ]' -ForegroundColor Yellow
    Write-Host '  Format: Server=HOST;Database={dbName};User Id=sa;Password=PWD;Encrypt=true;TrustServerCertificate=true;MultipleActiveResultSets=true;Connection Timeout=30;Application Name=CITL' -ForegroundColor DarkGray
    Set-Secret `
        -Key 'MultiTenancy:ConnectionStringTemplate' `
        -Prompt 'ConnectionStringTemplate'

    Write-Host ''

    # ── JWT ─────────────────────────────────────────────────────────────────────
    Write-Host '[ JWT ]' -ForegroundColor Yellow
    Write-Host '  Generate: [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))' -ForegroundColor DarkGray
    Set-Secret `
        -Key 'Jwt:SecretKey' `
        -Prompt 'SecretKey (Base64)' `
        -IsPassword $true

    Write-Host ''

    # ── Cloudflare R2 ───────────────────────────────────────────────────────────
    Write-Host '[ Cloudflare R2 Storage ]' -ForegroundColor Yellow
    Set-Secret -Key 'FileStorage:R2Endpoint'     -Prompt 'R2Endpoint    (https://ACCOUNT_ID.r2.cloudflarestorage.com)'
    Set-Secret -Key 'FileStorage:R2AccessKey'    -Prompt 'R2AccessKey'
    Set-Secret -Key 'FileStorage:R2SecretKey'    -Prompt 'R2SecretKey' -IsPassword $true
    Set-Secret -Key 'FileStorage:R2BucketName'   -Prompt 'R2BucketName'
    Set-Secret -Key 'FileStorage:R2PublicDomain' -Prompt 'R2PublicDomain (https://your-domain.com)'

    Write-Host ''
    Write-Host '══════════════════════════════════════════════════════════' -ForegroundColor Cyan
    Write-Host 'Done. Current secrets:' -ForegroundColor Cyan
    dotnet user-secrets list
}
finally {
    Pop-Location
}
