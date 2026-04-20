#!/usr/bin/env sh
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CFG_PATH="${1:-$SCRIPT_DIR/Ngaq.Server.test.jsonc}"
DOTNET_CMD="${DOTNET_CMD:-dotnet}"
RID="${RID:-win-x64}"

cd "$SCRIPT_DIR"
"$DOTNET_CMD" publish "Ngaq.Server.Test.csproj" -c Release -r "$RID" -p:RunAotTest=true
"./bin/Release/net10.0/$RID/publish/Ngaq.Server.Test.exe" "$CFG_PATH"
