#!/usr/bin/env bash
# =============================================================================
# CITL — Publish Script (Bash / Linux)
# Restores, builds, tests, and publishes for linux-x64.
#
# Usage:
#   ./scripts/publish.sh              → publish to ./publish/
#   ./scripts/publish.sh --skip-tests → skip tests, publish only
# =============================================================================

set -e
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

# ---- Configuration ----
PROJECT="$ROOT/src/CITL.WebApi/CITL.WebApi.csproj"
SOLUTION="$ROOT/CITL.slnx"
OUTPUT_DIR="$ROOT/publish"
CONFIGURATION="Release"
RUNTIME="linux-x64"
SELF_CONTAINED="false"
READY_TO_RUN="true"

SKIP_TESTS=false
if [ "$1" = "--skip-tests" ]; then
    SKIP_TESTS=true
fi

echo ""
echo "=== CITL Publish (Linux) ==="
echo "  Runtime       : $RUNTIME"
echo "  Configuration : $CONFIGURATION"
echo "  Output        : $OUTPUT_DIR"
echo "  SelfContained : $SELF_CONTAINED"
echo "  ReadyToRun    : $READY_TO_RUN"
echo ""

# ---- 1. Clean previous publish ----
echo "[1/4] Cleaning previous publish..."
rm -rf "$OUTPUT_DIR"
echo "  Clean complete"
echo ""

# ---- 2. Restore ----
echo "[2/4] Restoring packages..."
dotnet restore "$SOLUTION"
echo "  Restore complete"
echo ""

# ---- 3. Test (optional) ----
if [ "$SKIP_TESTS" = false ]; then
    echo "[3/4] Running tests..."
    dotnet test "$SOLUTION" --no-restore -c "$CONFIGURATION"
    echo "  Tests complete"
    echo ""
else
    echo "[3/4] Skipping tests (--skip-tests)"
    echo ""
fi

# ---- 4. Publish ----
echo "[4/4] Publishing..."
dotnet publish "$PROJECT" \
    -c "$CONFIGURATION" \
    -r "$RUNTIME" \
    --self-contained "$SELF_CONTAINED" \
    -p:PublishReadyToRun="$READY_TO_RUN" \
    -o "$OUTPUT_DIR" \
    --no-restore

echo "  Publish complete"
echo ""

# ---- Summary ----
FILE_COUNT=$(find "$OUTPUT_DIR" -type f | wc -l)
SIZE_MB=$(du -sm "$OUTPUT_DIR" | cut -f1)

echo "=== Publish Succeeded ==="
echo "  Output : $OUTPUT_DIR"
echo "  Files  : $FILE_COUNT"
echo "  Size   : ${SIZE_MB} MB"
echo ""
