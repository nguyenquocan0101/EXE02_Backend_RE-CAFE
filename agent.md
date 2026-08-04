# Agent Instructions: RE:CAFE Backend

## Scope

This repository is the RE:CAFE ASP.NET Core API. It owns authentication, authorization, catalog, inventory, cart, checkout, payments, coupons, reviews, loyalty, administration, media integration, database mappings, migrations, and deployment configuration.

The companion frontend is at `W:\DevPool\RECAFE_EXE01\RECAFE_EXE01`. Keep API behavior and response contracts compatible with that project unless a coordinated breaking change is explicitly requested.

## Read First

- `README.md` for architecture, local setup, API areas, and deployment notes.
- `Program.cs` for middleware, authentication, CORS, database, and service registration.
- The relevant `plans/` or `docs/` specification before changing an existing workflow.
- Existing controller, DTO, interface, and service implementations before adding a new pattern.

## Stack and Structure

- .NET 10 / ASP.NET Core Web API with nullable reference types enabled.
- PostgreSQL through Entity Framework Core and Npgsql.
- Controllers expose HTTP boundaries; services own business rules; DTOs define request and response contracts.
- `Data/` contains the EF Core context and persistence configuration.
- `Models/` contains domain entities and enums.
- `Migrations/` contains versioned schema changes.
- `Middlewares/` and `Extensions/` contain shared HTTP and application infrastructure.

## Implementation Rules

- Keep controllers thin. Put validation and business orchestration in services.
- Reuse existing DTO, interface, exception, authorization, and response patterns.
- Preserve existing route names, HTTP status semantics, pagination conventions, and JSON property names unless the contract change is intentional and documented.
- Use async EF Core and service APIs for I/O. Pass cancellation tokens when the surrounding API supports them.
- Do not expose entities directly when an existing DTO boundary is available.
- Treat authorization as part of the feature: verify ownership and role requirements at the service/controller boundary.
- For model changes, create an explicit migration and inspect it for destructive operations.
- Do not commit secrets, development credentials, tokens, payment keys, Cloudinary credentials, or local environment files.

## Commands

```bash
dotnet restore
dotnet build EXE02_Backend_RE-CAFE.csproj --no-restore
dotnet test tests/EXE02_Backend_RE-CAFE.Tests/EXE02_Backend_RE-CAFE.Tests.csproj
dotnet ef migrations add DescribeYourChange
dotnet ef database update
dotnet run
```

Use Docker Compose when PostgreSQL is not already running:

```bash
docker compose up -d --build
docker compose logs -f api
docker compose down
```

Never use `docker compose down -v` unless destroying the local database volume is intentional.

## Verification Checklist

- Build the API after code changes.
- Run the relevant tests; add focused tests for new business rules, authorization paths, and API contracts.
- For database changes, review the generated migration and test against representative existing data.
- Verify `/healthz`, the affected endpoint, error responses, and authenticated/unauthenticated behavior.
- Review `git diff` and `git diff --check` before finishing. Keep unrelated working-tree changes intact.
