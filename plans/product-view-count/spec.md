# Spec: Product view count

**Date:** 2026-08-12
**Status:** Ready

---

## Problem Statement

The storefront has no persisted product-view metric. Customers and the UI need a single count that increases whenever a product detail page is opened and is visible consistently in the product listing and detail page.

---

## User Stories

- **[P1]** As a shopper, I want each product-detail open to increase the product's view count so that the catalog reflects product interest.
  Accepted when: one successful detail-page open sends one request to the backend and the persisted count increases by exactly 1, including repeated opens by the same shopper.

- **[P1]** As a shopper, I want to see the same view count on product cards and the product detail page so that the UI is consistent.
  Accepted when: listing/card and detail responses render the same `viewCount` value, and the detail page reflects the incremented value after the open request succeeds.

- **[P1]** As an operator, I want the existing first two products initialized to 125 and 178 views so that the feature starts with the requested values.
  Accepted when: the first stable catalog product has `viewCount = 125` and the second stable catalog product has `viewCount = 178` after migration/backfill.

- **[P2]** As a shopper, I want a failed view-count request not to prevent product browsing so that a transient analytics failure does not break the storefront.
  Accepted when: product detail remains usable if the increment request fails, and the failure is logged without an error modal or navigation failure.

- **[P3]** _(out of scope — noted for future)_ Unique visitors, per-session deduplication, view-history tables, time-series analytics, and admin charts.

---

## Functional Requirements

1. **FR-01:** Add a persisted non-negative integer `ViewCount` field to `Product`, defaulting to `0` for newly created products.
2. **FR-02:** Add an explicit EF Core migration that adds the column and backfills the first two stable catalog products to `125` and `178` respectively without changing unrelated product data.
3. **FR-03:** Expose `POST /api/products/{id}/view` for active products. The operation must increment the stored count by exactly one using a concurrency-safe database update and return the new count.
4. **FR-04:** Return a not-found response for a missing or inactive product; the endpoint must not create a view row or mutate any other product.
5. **FR-05:** Include `viewCount` in public product-list and product-detail DTOs, and preserve the existing response envelope/property naming conventions.
6. **FR-06:** Add a frontend product API helper for the view endpoint using the existing `VITE_API_URL` convention.
7. **FR-07:** Call the view endpoint once when the product detail page opens for a product, update local detail state from the returned count, and treat a failed increment as non-blocking.
8. **FR-08:** Render the same `viewCount` on product listing cards and product detail UI with existing localization and visual conventions; do not invent a second client-side count.

---

## Non-Functional Requirements

- **Performance:** The increment endpoint should complete within p95 < 500 ms under normal local/production database conditions and use one database mutation round trip.
- **Concurrency:** 100 concurrent increments for one product must result in a net increase of exactly 100; no read-then-write lost updates are acceptable.
- **Security:** Only active public products are incrementable; no authentication requirement is added because the storefront currently supports public product browsing.
- **Availability:** A failed increment request must not block rendering, navigation, add-to-cart, or checkout actions on the detail page.

---

## Success Criteria

- [ ] API contract: `POST /api/products/{id}/view` returns HTTP 200 and the new `viewCount` for an active product.
- [ ] Persistence: repeated opens increase the stored value by one per successful request, including the initial values 125 and 178.
- [ ] Concurrency: a focused concurrent-increment test shows no lost updates.
- [ ] UI consistency: listing cards and detail page display the backend-provided `viewCount`; frontend production build passes.
- [ ] Regression safety: backend build and focused product API tests pass; inactive/missing product requests do not mutate data.

---

## Out of Scope

- Deduplicating views by user, IP, cookie, browser session, or time window.
- Historical view-event storage, dashboards, ranking, or analytics exports.
- Admin editing/resetting of view counts.

---

## Assumptions

- The first and second products are identified using stable existing catalog identifiers/order, preferably the current seed SKUs, not database physical row order.
- The frontend companion repository is `W:\DevPool\RECAFE_EXE01\RECAFE_EXE01`.
- The existing public product endpoints and response envelope remain unchanged apart from the additive `viewCount` field.

---

## [NEEDS CLARIFICATION]

None.
