# Phase 03 — Customer frontend flow

## Objective

Let customers review products from completed orders and let shoppers read verified reviews on product detail, using existing modal, toast, auth, localization, and warm ReCafe styling conventions.

## Files to create

- `src/services/api/reviews.ts`
- `src/components/reviews/ReviewModal.tsx`
- `src/components/reviews/StarRating.tsx`
- `src/components/reviews/ProductReviews.tsx`

## Files to modify

- `src/services/api/orders.ts` — type the review state added to order items and preserve the existing response unwrap behavior.
- `src/pages/Profile.tsx` — add a completed-order review action; open the modal with a distinct product/order pair; show existing-review state and delete/recreate flow.
- `src/pages/ProductDetail.tsx` — fetch reviews by product ID after product load and render `ProductReviews` before related products.
- `src/locales/vi-VN.json` and `src/locales/en-US.json` — add labels, validation errors, upload states, delete confirmation, and empty states.

## UX behavior

1. Keep the current compact order table, but add a review action in the Completed status cell or an order-detail expansion that lists individual products. Do not force the user to find a product manually on ProductDetail.
2. The modal has required star selection, optional comment, file picker, previews, remove-before-submit controls, and a clear `2 ảnh + 1 video` limit.
3. Client validation is immediate, but the server remains authoritative. Use `FormData` and do not set `Content-Type` manually so the browser supplies the multipart boundary.
4. Show an uploading/submitting state, disable duplicate submits, surface API errors through the existing toast, and refresh the order row after success.
5. When an existing review is present, show its content/media with `Xóa đánh giá`; deletion requires confirmation, then the same item returns to the create state.
6. Product detail shows average stars, total count, distribution, paginated visible reviews, verified-purchase badge, image/video media, rating filter, media-only filter, loading, empty, and error states.
7. Use the existing ReCafe palette and `Modal` portal; keep the review section responsive without introducing a new design system.

## Verification

- `npm run build` passes with strict TypeScript checks.
- Manual test on narrow viewport: file previews, modal scrolling, star selection, and submit/delete controls remain usable.
- Manual test as customer: Pending/Shipping orders have no review action; Completed order items do.
- Confirm a review created from Profile appears on ProductDetail after refresh.
