#!/usr/bin/env bash
# set-secrets.sh — Interactively sets all required dotnet user-secrets for CITL.WebApi.
# Usage: ./scripts/set-secrets.sh

set -euo pipefail

PROJECT_PATH="$(cd "$(dirname "$0")/../src/CITL.WebApi" && pwd)"

if [ ! -d "$PROJECT_PATH" ]; then
    echo "Error: Project not found: $PROJECT_PATH" >&2
    exit 1
fi

set_secret() {
    local key="$1"
    local prompt="$2"
    local silent="${3:-false}"

    if [ "$silent" = "true" ]; then
        read -r -s -p "  $prompt: " value
        echo
    else
        read -r -p "  $prompt: " value
    fi

    if [ -n "$value" ]; then
        (cd "$PROJECT_PATH" && dotnet user-secrets set "$key" "$value" > /dev/null)
        echo "  ✔ $key"
    else
        echo "  – $key (skipped)"
    fi
}

echo
echo "╔══════════════════════════════════════════════════════════╗"
echo "║           CITL — User Secrets Setup                     ║"
echo "║  Press Enter to skip (keeps existing value)             ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo

echo "[ SQL Server ]"
echo "  Format: Server=HOST;Database={dbName};User Id=sa;Password=PWD;Encrypt=true;TrustServerCertificate=true;MultipleActiveResultSets=true;Connection Timeout=30;Application Name=CITL"
set_secret "MultiTenancy:ConnectionStringTemplate" "ConnectionStringTemplate"
echo

echo "[ JWT ]"
echo "  Generate: openssl rand -base64 64"
set_secret "Jwt:SecretKey" "SecretKey (Base64)" true
echo

echo "[ Cloudflare R2 Storage ]"
set_secret "FileStorage:R2Endpoint"     "R2Endpoint    (https://ACCOUNT_ID.r2.cloudflarestorage.com)"
set_secret "FileStorage:R2AccessKey"    "R2AccessKey"
set_secret "FileStorage:R2SecretKey"    "R2SecretKey" true
set_secret "FileStorage:R2BucketName"   "R2BucketName"
set_secret "FileStorage:R2PublicDomain" "R2PublicDomain (https://your-domain.com)"

echo
echo "══════════════════════════════════════════════════════════"
echo "Done. Current secrets:"
(cd "$PROJECT_PATH" && dotnet user-secrets list)
