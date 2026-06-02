#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' deploy-postgres-1"
