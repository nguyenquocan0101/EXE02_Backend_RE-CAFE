# Brainstorm: Product view count

**Date:** 2026-08-12

## Ideas Explored

- Increment on every product-detail open: the frontend calls a dedicated mutation when the detail page mounts, and the backend increments an integer counter atomically.
- Increment only once per session or visitor: would reduce refresh noise, but was explicitly rejected because the requested behavior is every detail-page open.
- Count only frontend-local state: quick to implement, but it would not persist or stay synchronized across cards, detail pages, and users.

## User's Direction

Implement the MVP across the existing ASP.NET API and React storefront. Every opening of a product detail page increments the product's persisted view count by one. The same count is shown in product cards/listing and product detail UI. The first product starts at 125 views and the second starts at 178 views.

## Open Questions

- The implementation plan must choose the stable way to identify the first and second products for the initial backfill; use the existing catalog order/stable SKU data rather than relying on database row order.
- The plan must define the response shape for the increment endpoint and how the frontend handles a failed background increment without blocking product browsing.

## Risks

- A non-atomic read-then-write increment could lose concurrent views; the backend must use a database-side increment or equivalent concurrency-safe update.
- The detail page can be mounted more than once by route transitions or development Strict Mode; the frontend should scope one request to one actual detail-page open and avoid duplicate effect calls.
- Existing list/detail DTOs and frontend types must expose the same field name so the UI does not show stale or divergent counts.
