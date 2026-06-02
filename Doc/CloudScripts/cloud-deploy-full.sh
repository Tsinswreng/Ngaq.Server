#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "
set -eu
mkdir -p /opt/ngaq-server/deploy /opt/ngaq-server/config
if [ -f /opt/ngaq-server/upload/docker-compose.aot-slim.yml ]; then
  mv /opt/ngaq-server/upload/docker-compose.aot-slim.yml /opt/ngaq-server/deploy/
fi
if [ -f /opt/ngaq-server/upload/Ngaq.Server.cloud.docker.jsonc ]; then
  mv /opt/ngaq-server/upload/Ngaq.Server.cloud.docker.jsonc /opt/ngaq-server/config/
fi
docker load -i /opt/ngaq-server/upload/ngaq-server-aot-slim.tar.gz
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml up -d postgres redis
docker compose -f docker-compose.aot-slim.yml run --rm web migrate Ngaq.Server.cloud.docker.jsonc
docker compose -f docker-compose.aot-slim.yml up -d web
docker compose -f docker-compose.aot-slim.yml ps
"
