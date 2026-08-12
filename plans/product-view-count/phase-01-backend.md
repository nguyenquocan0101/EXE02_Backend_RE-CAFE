# Phase 01 — Backend view-count domain and API

**Depends on:** none
**Covers:** spec FR-01 through FR-05; P1 view increment and initial backfill stories

## Tasks

1. Add `ViewCount` to `Product` as a non-negative integer defaulting to zero.
2. Add the field to `ProductListDto` and `ProductDetailDto`; map it in every public/admin product projection where the DTO is reused.
3. Add `IncrementProductViewCountAsync(Guid id)` to `IProductService` and implement it in `ProductService`.
4. Ensure the increment is database-side and concurrency-safe. Restrict the update to `IsActive = true`; return null/not-found when no row matches.
5. Add `POST /api/products/{id}/view` to `ProductsController` using `SuccessResponse`/`ErrorResponse`.
6. Add an EF migration that creates the column with a default of zero and backfills stable products `RE-0001` to 125 and `RE-0002` to 178. Inspect generated SQL and ensure rerunning migration is naturally a no-op.
7. Add focused tests for default/model mapping, successful increment, missing/inactive product, list/detail DTO exposure, and concurrent increments where the existing PostgreSQL fixture permits it.

## Tests to Write First

- A product model/context contract sees `ViewCount` with default 0.
- An active product increments from 125 to 126 and returns 126.
- An inactive or unknown product returns the existing not-found contract and does not change a count.
- Parallel requests against one active product produce the exact expected total.
- Public list and slug detail payloads both include `viewCount`.

## Exit Criteria

- Backend builds cleanly.
- Focused product view tests pass.
- Migration contains only the additive column plus deterministic backfill.
