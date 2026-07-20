<div align="center">

# RE:CAFÉ API

**Circular coffee commerce, built for a cleaner loop.**

ASP.NET Core API for products, carts, checkout, payments, coupons, customer reviews, loyalty, and administration.

<br />

`ASP.NET Core 10` · `PostgreSQL 16` · `Entity Framework Core` · `Docker Compose`

</div>

## What this service does

RE:CAFÉ turns coffee by-products into products people can buy, track, and review. The backend provides the business rules behind the storefront and admin surfaces.

- Authentication with JWT and role-based access for `Admin`, `Staff`, and customers.
- Product catalog, categories, variants, inventory transactions, cart, checkout, and order history.
- Payment flows for card, bank transfer, and cash on delivery, including SePay webhook handling.
- Coupon scopes for an entire order, selected products, or selected categories.
- Customer reviews after completed orders: 1–5 stars, comments, up to 2 images + 1 video, and owner delete/recreate.
- Admin review moderation through `IsVisible`, without hard-deleting customer content.
- Cloudinary-backed image, video, product model, and customization assets.

## Repository map

| Repository | Purpose |
| --- | --- |
| [RECAFE_EXE01_FE](https://github.com/nguyenquocan0101/RECAFE_EXE01_FE) | Customer storefront and Admin UI |
| `EXE02_Backend_RE-CAFE` | This API, database model, migrations, and deployment files |

## Architecture

```text
Controllers      HTTP endpoints and authorization boundaries
DTOs             Request/response contracts
Services         Business rules and orchestration
Data             EF Core DbContext and PostgreSQL mappings
Models           Domain entities and enums
Migrations       Versioned database schema changes
Middlewares      Error handling and shared HTTP concerns
deploy/          VPS deployment notes and Nginx configuration
```

The API follows a service-oriented monolith structure: controllers stay thin, services own business rules, and EF Core owns persistence. PostgreSQL is the runtime database in both local Docker and production.

## Requirements

- .NET SDK 10
- PostgreSQL 16, or Docker Desktop with Docker Compose
- Git
- Cloudinary credentials for media upload features

## Run locally with .NET

1. Create a local PostgreSQL database named `ReCafeDb`.
2. Update the connection string in `appsettings.Development.json` or use environment variables.
3. Apply migrations and start the API:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

The API runs on the URL printed by ASP.NET Core. Keep `ApplyMigrationsOnStartup=false` for local runs when you prefer explicit migration control.

## Run with Docker Compose

```bash
cp docker.env.example .env
# Edit .env and replace every development/placeholder secret.
docker compose up -d --build
docker compose ps
```

The default Compose mapping exposes the API at `http://localhost:5080` and PostgreSQL inside the Compose network. The API waits for the database health check before starting.

Useful commands:

```bash
docker compose logs -f api
docker compose logs -f postgres
docker compose down
```

Do not run `docker compose down -v` unless you intentionally want to destroy the local PostgreSQL volume.

## Configuration

Important environment variables:

| Variable | Purpose |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | Compose database credentials |
| `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE` | Token validation settings |
| `CORS_ALLOWED_ORIGINS` | Allowed storefront origins |
| `CLOUDINARY_CLOUD_NAME`, `CLOUDINARY_API_KEY`, `CLOUDINARY_API_SECRET` | Media storage |
| `SEPAY_BANK_ACCOUNT`, `SEPAY_API_KEY` | Payment webhook configuration |
| `APPLY_MIGRATIONS_ON_STARTUP` | Whether the container applies EF migrations on boot |

Never commit real secrets. Rotate any credential that has been exposed in shell history, logs, chat, or a repository file.

## API areas

The API is grouped by resource:

```text
/api/auth                 Login and registration
/api/products             Public catalog and product details
/api/categories           Public categories
/api/cart                 Customer cart
/api/orders               Checkout and customer order history
/api/coupons              Coupon preview
/api/reviews              Customer creation, public product reads, owner delete
/api/admin/*              Admin management and moderation
/api/sepay-webhook        SePay payment notification
```

The Swagger surface is controlled by `EnableSwagger` and should be enabled only in trusted environments.

## Review rules

- A customer can review only a product contained in their own `Completed` order.
- One review is allowed per `(UserId, OrderId, ProductId)`.
- There is no edit operation. Delete the existing review, then create a replacement.
- A review accepts 1–5 stars, an optional comment, up to 2 images and 1 video.
- Images are limited to 10 MB each; videos to 50 MB each.
- New reviews are visible immediately. Admin can hide them using `IsVisible`.
- `ReviewMedia` stores the URL, Cloudinary public ID, and media type separately from the review.

## Coupons

Coupons support three scopes:

| Scope | Meaning |
| --- | --- |
| `Order` | Applies to the order subtotal |
| `Product` | Applies only to products mapped in `CouponProducts` |
| `Category` | Reserved for category-based business rules |

`POST /api/coupons/preview` validates the current cart and returns the eligible subtotal, discount, and applicable cart item IDs. A `400` response means the request is understood but a business rule rejected it; the response message explains whether the code is invalid, expired, exhausted, or not applicable to the selected products.

## Database workflow

Create a migration after changing the model:

```bash
dotnet ef migrations add DescribeYourChange
dotnet ef database update
```

Before a production migration:

1. Take a PostgreSQL backup.
2. Review the generated migration for destructive operations.
3. Check foreign keys and existing data constraints.
4. Apply during a controlled deployment window.
5. Verify health, logs, and the affected API flow.

## Verification

```bash
dotnet build EXE02_Backend_RE-CAFE.csproj --no-restore
```

Feature smoke scripts for the review work live under `plans/scratch-tests/`. They validate the foundation, review API, customer FE contract, and Admin moderation guard.

## Production deployment

Production deployment uses the VPS at `180.93.114.25` and the Compose stack described in [`deploy/VPS_DEPLOYMENT.md`](deploy/VPS_DEPLOYMENT.md).

The deployment script pulls the selected branch, starts PostgreSQL, waits for its health check, rebuilds the API, and prints service status/logs:

```bash
./deploy.sh main
```

Use key-based SSH authentication where possible. Keep `POSTGRES_PASSWORD`, JWT, Cloudinary, and payment credentials in deployment secrets rather than source files.

## Project docs

- [`deploy/VPS_DEPLOYMENT.md`](deploy/VPS_DEPLOYMENT.md) — VPS and GitHub Actions deployment
- [`docs/FE_VOUCHER_PRODUCT_SCOPE_FLOW.md`](docs/FE_VOUCHER_PRODUCT_SCOPE_FLOW.md) — coupon scope contract
- [`plans/customer-product-review/spec.md`](plans/customer-product-review/spec.md) — customer review requirements
- [`plans/customer-product-review/plan.md`](plans/customer-product-review/plan.md) — implementation phases and verification matrix

<div align="center">

Built for a more circular coffee economy.

</div>
