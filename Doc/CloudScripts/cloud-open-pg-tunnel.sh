#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

LOCAL_PORT="${1:-15432}"
PG_IP="${2:?PG container IP is required as the second argument}"

mk_askpass
trap cleanup_askpass EXIT

if ss -ltn | awk '{print $4}' | grep -q ":$LOCAL_PORT\$"; then
  echo "local port $LOCAL_PORT is already in use"
  exit 1
fi

setsid ssh -f -N \
  -o ExitOnForwardFailure=yes \
  -o StrictHostKeyChecking=no \
  -L "$LOCAL_PORT:$PG_IP:5432" \
  "$NGAQ_USER@$NGAQ_HOST"

ss -ltn | grep ":$LOCAL_PORT "
