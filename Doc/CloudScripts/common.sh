#!/bin/sh
set -eu

: "${NGAQ_HOST:?NGAQ_HOST is required}"
: "${NGAQ_USER:?NGAQ_USER is required}"
: "${NGAQ_SSH_PASS:?NGAQ_SSH_PASS is required}"

mk_askpass() {
  PASS_SCRIPT="$(mktemp)"
  cat > "$PASS_SCRIPT" <<EOF
#!/bin/sh
printf '%s\n' '$NGAQ_SSH_PASS'
EOF
  chmod 700 "$PASS_SCRIPT"
  export SSH_ASKPASS="$PASS_SCRIPT"
  export SSH_ASKPASS_REQUIRE=force
  export DISPLAY=:0
}

cleanup_askpass() {
  rm -f "${PASS_SCRIPT:-}"
}

run_ssh() {
  ssh -o StrictHostKeyChecking=no "$NGAQ_USER@$NGAQ_HOST" "$@"
}

run_scp() {
  scp -o StrictHostKeyChecking=no "$@"
}
