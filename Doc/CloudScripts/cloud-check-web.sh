#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "
set -eu
cd /opt/ngaq-server/deploy
echo '=== web logs ==='
docker compose -f docker-compose.aot-slim.yml logs --tail=120 web
echo
echo '=== curl ==='
curl -i --max-time 15 http://127.0.0.1:2341/Open/Time
"
