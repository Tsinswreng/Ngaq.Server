#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

TAIL_LINES="${1:-200}"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml logs -f --tail=$TAIL_LINES web
"
