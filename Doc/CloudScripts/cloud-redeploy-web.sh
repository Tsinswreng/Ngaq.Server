#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

IMAGE_TAR="${1:-/opt/ngaq-server/upload/ngaq-server-aot-slim.tar.gz}"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "
set -eu
docker load -i '$IMAGE_TAR'
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml up -d web
docker compose -f docker-compose.aot-slim.yml ps
"
