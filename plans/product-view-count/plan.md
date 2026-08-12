# Plan: Product view count

**Source spec:** `plans/product-view-count/spec.md`
**Mode:** --hard (performed inline; no sub-agent runner is available)
**Risk:** high-risk — additive database schema change, migration/backfill, and coordinated FE/BE API contract
**Status:** Complete

## Scope Challenge

- **Exists?** No persisted product view-count field or public increment endpoint exists. The storefront fetches public products directly and the detail page has no view metric.
- **Minimum?** Add one integer field, one public POST endpoint, additive DTO fields, a frontend API helper, and two UI placements. Seed/backfill the first two stable products to 125/178.
- **Complexity?** Hard: backend model/migration/API, frontend listing/detail, tests, and cross-repository verification.

## Research Summary

- The backend uses `ProductService` behind `IProductService`, thin `ProductsController`, EF Core/PostgreSQL, and success envelopes from `BaseApiController`.
- Public list and detail DTOs are separate (`ProductListDto`, `ProductDetailDto`) and both are mapped in `ProductService`; the new field must be added to both.
- The frontend companion repo is `W:\DevPool\RECAFE_EXE01\RECAFE_EXE01`. `ProductListing.tsx` and `ProductDetail.tsx` currently use direct `fetch` calls and unwrap either a raw array/object or `{ data }`.
- The frontend is rendered under `React.StrictMode`; the detail effect currently depends on `language`, so view counting must be isolated from language changes and guarded against duplicate development effect calls.
- Stable initial products are present in the existing seed data as `RE-0001` and `RE-0002`, with known IDs. The migration should target these stable SKUs/IDs and avoid physical row order.

## Chosen Approach

Use a persisted `Product.ViewCount` integer and `POST /api/products/{id}/view`. The service performs an active-product, database-side atomic increment, then returns the current count in a small response DTO. Existing GET list/detail contracts are extended additively with `viewCount`. The detail page calls the endpoint once per actual route open, updates its local product count from the returned value, and ignores request failure for browsing purposes. Product cards render the count returned by the existing list endpoint.

### Alternative considered: separate `ProductView` event table

This would support analytics and unique-visitor rules later, but it adds a table, indexes, retention/aggregation decisions, and more write volume for a requirement that only needs a total count. Keep it out of this MVP.

## Phase Overview

| Phase | Scope | Depends on |
|---|---|---|
| 01 | Backend model, migration, DTOs, atomic service operation, endpoint, and focused tests | — |
| 02 | Frontend API helper, product listing/card display, detail-page increment and display | 01 |
| 03 | Cross-repo verification, migration review, contract smoke test, and handoff | 01, 02 |

## Files to Change

### Backend: `W:\DevPool\EXE02_Backend_RE-CAFE`

- `Models/Product.cs` — add `ViewCount` with default 0.
- `Data/ApplicationDbContext.cs` — configure non-negative/default behavior if required by the existing model convention.
- `DTOs/ProductDto.cs` — add `ViewCount` to list/detail DTOs and add an increment response DTO if needed.
- `Interfaces/IProductService.cs` — add `IncrementProductViewCountAsync(Guid id)`.
- `Services/ProductService.cs` — map `ViewCount`, implement active-product atomic increment, and return the new count.
- `Controllers/Product/ProductsController.cs` — add `POST {id}/view` with existing response/error envelope conventions.
- `Migrations/<timestamp>_AddProductViewCount.cs` plus designer/snapshot — add the column and deterministic 125/178 backfill.
- `tests/.../ProductViewCount*Tests.cs` — model/API/service/concurrency contract coverage following existing test conventions.

### Frontend: `W:\DevPool\RECAFE_EXE01\RECAFE_EXE01`

- `src/services/api/products.ts` — add typed `incrementProductView(productId)` using `VITE_API_URL` and response unwrapping.
- `src/pages/ProductListing.tsx` — type/map `viewCount` and render a compact eye/count metadata row on each card.
- `src/pages/ProductDetail.tsx` — type `viewCount`, call the helper once after the product is loaded, update local state with returned count, and render the count in the product info area.
- `src/locales/vi-VN.json`, `src/locales/en-US.json` — add the localized view-count label if the current translation structure supports it.

## API Contract

### `POST /api/products/{id}/view`

- Public endpoint; no request body.
- `200 OK` response uses the existing success envelope and returns `{ viewCount: number }` in `data`.
- `404 Not Found` for missing/inactive product using the existing error envelope.
- Each successful request increments exactly once. Concurrent requests must not overwrite each other.
- `viewCount` is included in `GET /api/products` and `GET /api/products/slug/{slug}` as an additive JSON property.

## Red-Team Review

- **Accepted:** Use a database-side increment rather than loading the entity, incrementing in memory, and saving; this prevents lost updates.
- **Accepted:** Target seed products by stable SKU/ID in the migration; do not infer product identity from database row order.
- **Accepted:** Remove `language` from the data-fetch effect or isolate the view effect; otherwise changing language could count an open again.
- **Accepted:** Guard the detail increment against React StrictMode's development double effect invocation.
- **Noted:** A list page already loaded before another tab increments a product will remain stale until re-fetch; real-time cross-tab synchronization is outside this MVP.
- **Rejected:** Authentication or visitor deduplication; the user explicitly requested one increment for every detail-page open.

## Verification Matrix

| Check | Expected result |
|---|---|
| Model/migration contract | `ViewCount` exists, defaults to 0, and RE-0001/RE-0002 backfill to 125/178 |
| Active increment | POST returns 200 and count increases by 1 |
| Missing/inactive increment | POST returns 404 and no product is mutated |
| Concurrent increment | N requests produce exactly N net increase |
| Public DTOs | list and slug detail both expose the same `viewCount` |
| Frontend build | `npm run build` passes |
| UI behavior | one detail open triggers one request; cards/detail render the count; API failure does not block browsing |

## Implementation Handoff

Implementation completed with `$ck-cook --hard --tdd plans/product-view-count/plan.md`. The backend migration was inspected and exercised through the PostgreSQL integration test fixture.

## Session Notes
<!-- Updated by cook automatically — do not edit manually -->

**Last active:** 2026-08-12 18:58
**Phase in progress:** phase-03-verification
**Status:** Backend and frontend implementation completed; full backend/frontend test suites and production builds passed. Final review and scoped commits are next.

### Decisions made this session

- Added `Product.ViewCount` with database default 0.
- Implemented `POST /api/products/{id}/view` with an EF Core database-side increment restricted to active products.
- Backfilled `RE-0001` to 125 and `RE-0002` to 178 in the migration.
- Added a frontend guard keyed by product slug so language changes and React StrictMode effect replay do not create duplicate opens.
- Kept view-count failures non-blocking and logged them without changing the product browsing state.

### Next immediate action

Run the final diff/security review, update feature evidence, and commit only this feature's files in the backend and frontend repositories.
