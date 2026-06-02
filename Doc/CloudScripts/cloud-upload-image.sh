#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

IMAGE_TAR="${1:-/tmp/ngaq-server-aot-slim.tar.gz}"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "mkdir -p /opt/ngaq-server/upload"
run_scp "$IMAGE_TAR" "$NGAQ_USER@$NGAQ_HOST:/opt/ngaq-server/upload/"
