# Deployment (Ubuntu VPS)

## One-time setup

```bash
cd ~/EXE02_Backend_RE-CAFE
cp docker.env.example .env
chmod +x deploy.sh
```

Update values in `.env` (especially `POSTGRES_PASSWORD`, `JWT_KEY`) once.

## Deploy every new push

```bash
cd ~/EXE02_Backend_RE-CAFE
./deploy.sh
```

Or deploy a specific branch:

```bash
./deploy.sh main
```

## URLs

- Swagger: `http://<server-ip>:5080/swagger/index.html`
- Health: `http://<server-ip>:5080/healthz`

## Notes

- Do not edit `docker-compose.yml` on server. Keep server-only values in `.env`.
- `deploy.sh` uses `git pull --rebase --autostash` to reduce pull conflicts.
