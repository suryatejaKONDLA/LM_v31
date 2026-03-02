#!/usr/bin/env bash
# =============================================================================
# CITL — Build Script (Bash)
# Restores, builds, and runs all tests.
# Usage: ./scripts/build.sh
# =============================================================================

set -e
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

echo ""
echo "=== CITL Build ==="
echo ""

# ---- 1. Restore ----
echo "[1/3] Restoring packages..."
dotnet restore "$ROOT/CITL.slnx"
echo "  Restore complete"
echo ""

# ---- 2. Build ----
echo "[2/3] Building solution..."
dotnet build "$ROOT/CITL.slnx" --no-restore -c Debug
echo "  Build complete"
echo ""

# ---- 3. Test ----
echo "[3/3] Running tests..."
dotnet test "$ROOT/CITL.slnx" --no-build -c Debug
echo "  Tests complete"
echo ""

echo "=== Build Succeeded ==="
echo ""
