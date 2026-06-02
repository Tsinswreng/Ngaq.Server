#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

TAIL_LINES="${1:-300}"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "
set -eu
cd /opt/ngaq-server/deploy
echo '=== web logs ==='
docker compose -f docker-compose.aot-slim.yml logs --tail=$TAIL_LINES web
echo
echo '=== ps ==='
docker compose -f docker-compose.aot-slim.yml ps
"
