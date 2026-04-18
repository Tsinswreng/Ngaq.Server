#!/usr/bin/env sh
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CFG_PATH="${1:-$SCRIPT_DIR/Ngaq.Server.test.jsonc}"
DOTNET_CMD="${DOTNET_CMD:-dotnet}"

cd "$SCRIPT_DIR"
"$DOTNET_CMD" run -- "$CFG_PATH"
