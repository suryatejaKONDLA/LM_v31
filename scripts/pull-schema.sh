#!/usr/bin/env bash
# pull-schema.sh — Extracts SQL schema from tenant databases using sqlpackage.
# Usage:
#   ./scripts/pull-schema.sh CITLPOS      # single tenant
#   ./scripts/pull-schema.sh              # all tenants

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CONFIG_PATH="$REPO_ROOT/src/CITL.WebApi/appsettings.Development.json"
OUTPUT_ROOT="$REPO_ROOT/src/CITL.Infrastructure/Migrations/Pulled"

if [ ! -f "$CONFIG_PATH" ]; then
    echo "Error: Config file not found: $CONFIG_PATH" >&2
    exit 1
fi

# Verify sqlpackage
if ! command -v sqlpackage &>/dev/null; then
    echo "Installing sqlpackage..."
    dotnet tool install -g microsoft.sqlpackage
fi

# Read config (requires jq)
if ! command -v jq &>/dev/null; then
    echo "Error: jq is required. Install it: apt-get install jq / brew install jq" >&2
    exit 1
fi

# Read config — user secrets overlay on top of appsettings (secrets take precedence)
SECRETS_ID="7edbee65-f437-4353-9b41-f0c6ce374469"
SECRETS_PATH="${HOME}/.microsoft/usersecrets/${SECRETS_ID}/secrets.json"

TEMPLATE=$(jq -r '.MultiTenancy.ConnectionStringTemplate // empty' "$CONFIG_PATH")

if [ -f "$SECRETS_PATH" ]; then
    SECRET_TEMPLATE=$(jq -r '.["MultiTenancy:ConnectionStringTemplate"] // empty' "$SECRETS_PATH")
    [ -n "$SECRET_TEMPLATE" ] && TEMPLATE="$SECRET_TEMPLATE"
fi

if [ -z "$TEMPLATE" ]; then
    echo "Error: MultiTenancy:ConnectionStringTemplate not set. Run:" >&2
    echo "  dotnet user-secrets set \"MultiTenancy:ConnectionStringTemplate\" \"Server=...\"" >&2
    exit 1
fi
TENANT_ID="${1:-}"

extract_tenant() {
    local tid="$1"
    local db_name
    db_name=$(jq -r ".MultiTenancy.TenantMappings.\"$tid\" // empty" "$CONFIG_PATH")

    if [ -z "$db_name" ]; then
        echo "Error: Tenant '$tid' not found in TenantMappings" >&2
        return 1
    fi

    local conn_str="${TEMPLATE//\{dbName\}/$db_name}"
    local out_dir="$OUTPUT_ROOT/$tid"

    [ -d "$out_dir" ] && rm -rf "$out_dir"

    echo ""
    echo "── Extracting: $tid (db: $db_name) ──"

    sqlpackage \
        /Action:Extract \
        "/SourceConnectionString:$conn_str" \
        "/TargetFile:$out_dir" \
        /p:ExtractTarget=SchemaObjectType \
        /p:VerifyExtraction=false \
        /p:ExtractReferencedServerScopedElements=false \
        /p:ExtractAllTableData=false

    local count
    count=$(find "$out_dir" -name '*.sql' | wc -l)
    echo "  Done: $count SQL files → $out_dir"
}

if [ -n "$TENANT_ID" ]; then
    extract_tenant "$TENANT_ID"
else
    echo "Extracting schema for ALL tenants..."
    for tid in $(jq -r '.MultiTenancy.TenantMappings | keys[]' "$CONFIG_PATH"); do
        extract_tenant "$tid" || true
    done
fi

echo ""
echo "Schema extraction complete."
