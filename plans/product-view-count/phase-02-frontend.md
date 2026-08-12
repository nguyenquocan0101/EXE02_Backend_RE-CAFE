# Phase 02 — Frontend view-count UX integration

**Depends on:** phase-01-backend
**Covers:** spec FR-06 through FR-08; P1 UI consistency and P2 non-blocking failure story

## Tasks

1. Add a typed `src/services/api/products.ts` helper for `POST /api/products/{id}/view`, using the existing API URL and `{ data }` response shape.
2. Extend the listing product mapping with `viewCount` and render an eye icon/label/count row inside the existing card content without changing card navigation or add-to-cart behavior.
3. Extend `DBProduct` with `viewCount` and render the count in the product detail information area using the existing ReCafe palette/spacing.
4. In `ProductDetail.tsx`, invoke the increment after the product load succeeds. Use a ref/key guard so React StrictMode and language changes do not issue duplicate increments for one open.
5. When the increment returns a new count, update only the local `dbProduct.viewCount`; if it fails, log the error and leave the page fully usable.
6. Add Vietnamese and English labels, keeping the count readable on mobile and aligned with existing metadata.

## Tests to Write First

- Product listing mapping preserves an API `viewCount` and renders the label/count.
- Detail page calls the increment helper once for a loaded slug, not again when language changes or StrictMode replays the effect.
- A rejected increment does not render an error boundary/modal and the product detail remains available.

## Exit Criteria

- Frontend production build passes.
- Manual test confirms 125 → 126 (first product) and 178 → 179 (second product) after opening details.
- Listing cards and detail show the same backend count after a fresh fetch.
