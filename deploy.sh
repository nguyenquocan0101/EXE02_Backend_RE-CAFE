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
docker compose --env-file .env up -d --build --force-recreate --remove-orphans

echo "[deploy] Service status"
docker compose --env-file .env ps

echo "[deploy] Last 40 lines of API logs"
docker compose --env-file .env logs --tail=40 api
