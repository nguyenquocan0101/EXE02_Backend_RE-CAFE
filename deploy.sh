#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

BRANCH="${1:-$(git rev-parse --abbrev-ref HEAD)}"

if [[ ! -f ".env" ]]; then
  cp docker.env.example .env
  echo "[deploy] Created .env from docker.env.example"
  echo "[deploy] Update secrets in .env if needed, then rerun ./deploy.sh"
  exit 0
fi

echo "[deploy] Pulling latest code from branch: $BRANCH"
git fetch --all --prune
git pull --rebase --autostash origin "$BRANCH"

echo "[deploy] Rebuilding and recreating containers"

if docker compose version >/dev/null 2>&1; then
  COMPOSE_CMD="docker compose"
elif command -v docker-compose >/dev/null 2>&1; then
  COMPOSE_CMD="docker-compose"
else
  echo "[deploy] Neither docker compose nor docker-compose is installed."
  exit 1
fi

set -a
. ./.env
set +a

: "${POSTGRES_USER:=postgres}"
: "${POSTGRES_DB:=ReCafeDb}"
: "${POSTGRES_PASSWORD:=postgres}"

echo "[deploy] Starting postgres first"
$COMPOSE_CMD --env-file .env up -d postgres

POSTGRES_CONTAINER_ID="$($COMPOSE_CMD --env-file .env ps -q postgres)"
if [[ -z "$POSTGRES_CONTAINER_ID" ]]; then
  echo "[deploy] Failed to find postgres container id."
  exit 1
fi

echo "[deploy] Waiting for postgres to become healthy"
for _ in {1..60}; do
  STATUS="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "$POSTGRES_CONTAINER_ID" 2>/dev/null || true)"
  if [[ "$STATUS" == "healthy" ]]; then
    break
  fi
  sleep 2
done

if [[ "${STATUS:-}" != "healthy" ]]; then
  echo "[deploy] Postgres is not healthy yet (status=${STATUS:-unknown})."
  $COMPOSE_CMD --env-file .env logs --tail=60 postgres || true
  exit 1
fi

ESCAPED_POSTGRES_PASSWORD="${POSTGRES_PASSWORD//\'/\'\'}"
echo "[deploy] Syncing password for postgres role to avoid 28P01"
$COMPOSE_CMD --env-file .env exec -T postgres \
  psql -v ON_ERROR_STOP=1 \
  -U "$POSTGRES_USER" \
  -d postgres \
  -c "ALTER USER \"$POSTGRES_USER\" WITH PASSWORD '$ESCAPED_POSTGRES_PASSWORD';"

echo "[deploy] Rebuilding and recreating remaining services"
$COMPOSE_CMD --env-file .env up -d --build --force-recreate --remove-orphans

echo "[deploy] Service status"
$COMPOSE_CMD --env-file .env ps

echo "[deploy] Last 40 lines of API logs"
$COMPOSE_CMD --env-file .env logs --tail=40 api
