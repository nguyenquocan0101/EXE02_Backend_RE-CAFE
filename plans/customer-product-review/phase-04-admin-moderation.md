# Phase 04 — Admin moderation

## Objective

Provide an Admin-only surface to inspect reviews and toggle `IsVisible`, while keeping public product feedback immediately consistent.

## Files to create

- `src/pages/admin/AdminReviews.tsx`

## Files to modify

- `src/services/api/admin.ts` — add `getAdminReviews`, `getAdminReviewById` only if needed by the chosen table, and `setReviewVisibility`.
- `src/App.tsx` — add `/admin/reviews` under the Admin-only protected route.
- `src/pages/admin/AdminDashboard.tsx` — add a review moderation entry/card.
- `src/locales/vi-VN.json` and `src/locales/en-US.json` — add moderation labels and status copy.
- `Controllers/Review/AdminReviewsController.cs`, `Interfaces/IReviewService.cs`, and `Services/ReviewService.cs` — support filters and visibility updates from Phase 02.

## UI behavior

- Table columns: product, customer, stars, comment/media indicator, created date, visibility.
- Add filters for visible/hidden, product keyword, and rating; use the existing Admin table/modal patterns.
- Toggle visibility optimistically only after the API succeeds; on failure restore the prior state and show a toast.
- Do not expose hard-delete to Admin in this MVP unless a separate retention decision is made; visibility is the moderation action.

## Verification

- Admin can list and toggle visibility.
- Staff/customer tokens receive 403 from backend moderation endpoints.
- The next public product review request excludes a hidden review.
- The new route does not break existing Admin routes or build output.
