# VPS Deployment

## Current production VPS

- Host/IP: `180.93.114.25`
- SSH user: `root`
- SSH port: `22`
- Deploy path: `/opt/EXE02_Backend_RE-CAFE`
- API port before Nginx: `5080`

## GitHub Actions deploy

Workflow `.github/workflows/deploy-vps.yml` deploys automatically on push to `main`.

Required GitHub secrets:

- `POSTGRES_PASSWORD`
- `VPS_SSH_KEY` or `VPS_SSH_PASSWORD`

Optional GitHub variables/secrets:

- `VPS_HOST` defaults to `180.93.114.25`
- `VPS_USERNAME` defaults to `root`
- `VPS_PORT` defaults to `22`
- `DEPLOY_PATH` defaults to `/opt/EXE02_Backend_RE-CAFE`
- `JWT_KEY`
- `CORS_ALLOWED_ORIGINS`
- Cloudinary values if media upload is used

The workflow can bootstrap a fresh Ubuntu VPS by installing missing `git`, Docker,
and the Docker Compose plugin before cloning and running the application.

## First deploy checklist

1. Add `VPS_SSH_PASSWORD` in GitHub repository secrets with the VPS root password,
   or add `VPS_SSH_KEY` for key-based SSH.
2. Add `POSTGRES_PASSWORD` in GitHub repository secrets.
3. Push to `main` or manually run the `Deploy To VPS` workflow.
4. Verify:

```bash
curl http://180.93.114.25:5080/healthz
```

