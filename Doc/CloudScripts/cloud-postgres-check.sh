#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "
set -eu
cd /opt/ngaq-server/deploy
echo '=== postgres logs ==='
docker compose -f docker-compose.aot-slim.yml logs --tail=200 postgres
echo
echo '=== recent users ==='
docker exec deploy-postgres-1 psql -U ngaq -d ngaq -c 'select \"Id\", \"UniqName\", \"Email\", \"DbCreatedAt\" from \"User\" order by \"DbCreatedAt\" desc limit 10;' || true
"
