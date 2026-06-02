#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

IMAGE_TAR="${1:-/tmp/ngaq-server-aot-slim.tar.gz}"
COMPOSE_FILE="${2:-/mnt/e/_code/CsNgaq/Ngaq.Server/Deploy/Cloud/docker-compose.aot-slim.yml}"
CFG_FILE="${3:-/mnt/e/_code/CsNgaq/Ngaq.Server/ExternalRsrc/Ngaq.Server.cloud.docker.jsonc}"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "mkdir -p /opt/ngaq-server/upload"
run_scp \
  "$IMAGE_TAR" \
  "$COMPOSE_FILE" \
  "$CFG_FILE" \
  "$NGAQ_USER@$NGAQ_HOST:/opt/ngaq-server/upload/"
