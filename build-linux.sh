#!/usr/bin/env bash
# Headless Linux build of Bandhana. Exits 0 on success, non-zero on failure.
# Unity must NOT be open with this project — it'll fail with a project-lock error.
#
# Usage:
#   ./build-linux.sh
#
# Output: ./Builds/Linux/Bandhana

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNITY_VERSION="$(awk '/m_EditorVersion:/ {print $2}' "$PROJECT_DIR/ProjectSettings/ProjectVersion.txt")"
UNITY_BIN="$HOME/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity"
LOG_FILE="$PROJECT_DIR/Builds/build-linux.log"

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "Unity editor not found at: $UNITY_BIN" >&2
  echo "Check ~/Unity/Hub/Editor/ for the right version." >&2
  exit 2
fi

mkdir -p "$PROJECT_DIR/Builds"

echo "Building with $UNITY_VERSION ..."
"$UNITY_BIN" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$PROJECT_DIR" \
  -executeMethod Bandhana.EditorTools.BuildPlayer.BuildLinuxHeadless \
  -logFile "$LOG_FILE"

CODE=$?
if [[ $CODE -eq 0 ]]; then
  echo "Build OK. Output: $PROJECT_DIR/Builds/Linux/Bandhana"
  echo "Log: $LOG_FILE"
else
  echo "Build FAILED (exit $CODE). See log:" >&2
  echo "  $LOG_FILE" >&2
  tail -40 "$LOG_FILE" >&2 || true
fi
exit $CODE
