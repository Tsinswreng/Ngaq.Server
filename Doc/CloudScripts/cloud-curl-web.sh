#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "
set -eu
printf '=== curl /Open/Time ===\n'
curl -v --max-time 20 http://127.0.0.1:2341/Open/Time || true
printf '\n=== curl / ===\n'
curl -v --max-time 20 http://127.0.0.1:2341/ || true
"
