#!/usr/bin/env bash
# Headless Linux build of Bandhana. Exits 0 on success, non-zero on failure.
# Unity must NOT be open with this project — it'll fail with a project-lock error.
#
# Usage:
#   ./build-linux.sh             normal build
#   ./build-linux.sh --rebuild   force Unity to rebuild Library/ from scratch
#                                (slow; use after editor crashes or first headless run)
#
# Output: ./Builds/Linux/Bandhana

set -uo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNITY_VERSION="$(awk '/m_EditorVersion:/ {print $2}' "$PROJECT_DIR/ProjectSettings/ProjectVersion.txt")"
UNITY_BIN="$HOME/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity"
LOG_FILE="$PROJECT_DIR/Builds/build-linux.log"

REBUILD_FLAG=""
for arg in "$@"; do
  case "$arg" in
    --rebuild) REBUILD_FLAG="-rebuildLibrary" ;;
    *) echo "Unknown arg: $arg" >&2; exit 2 ;;
  esac
done

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "Unity editor not found at: $UNITY_BIN" >&2
  echo "Check ~/Unity/Hub/Editor/ for the right version." >&2
  exit 2
fi

mkdir -p "$PROJECT_DIR/Builds"

echo "Building with $UNITY_VERSION ${REBUILD_FLAG:+(rebuilding Library/ — this is slow)} ..."
"$UNITY_BIN" \
  -batchmode \
  -nographics \
  -quit \
  $REBUILD_FLAG \
  -projectPath "$PROJECT_DIR" \
  -executeMethod Bandhana.EditorTools.BuildPlayer.BuildLinuxHeadless \
  -logFile "$LOG_FILE"

CODE=$?

# Detect the asset-database corruption case and tell the user the recovery command.
if [[ $CODE -ne 0 ]] && grep -q "Database corruption" "$LOG_FILE" 2>/dev/null; then
  echo "" >&2
  echo "Asset database is out of sync. Re-run with:" >&2
  echo "  ./build-linux.sh --rebuild" >&2
fi
if [[ $CODE -eq 0 ]]; then
  echo "Build OK. Output: $PROJECT_DIR/Builds/Linux/Bandhana"
  echo "Log: $LOG_FILE"
else
  echo "Build FAILED (exit $CODE). See log:" >&2
  echo "  $LOG_FILE" >&2
  tail -40 "$LOG_FILE" >&2 || true
fi
exit $CODE
