# Brainstorm: Customer product reviews after purchase

**Date:** 2026-07-20

## Ideas Explored

- Review from the completed order: the customer sees a review action beside each purchased product. This keeps the review tied to a verified purchase.
- Review from the product detail page: convenient for browsing, but it needs an extra lookup to find an eligible completed order and is easier to misuse.
- A standalone review page: simple to discover from the account area, but less contextual than reviewing directly from an order.
- Store media URLs as JSON on `Review`: fastest schema change, but less flexible for mixed media, deletion, moderation, and future metadata.
- Store media in a separate `ReviewMedia` table: clearer ownership and lifecycle for images/videos, and easier to delete the corresponding Cloudinary assets.

## User's Direction

The user chose verified reviews submitted from completed orders:

- A customer can review only products belonging to an order with status `Completed`.
- Each review contains a 1–5 star rating and an optional comment.
- Each review may contain up to 3 media files: 2 images and 1 video.
- Review media is stored in a separate `ReviewMedia` table with `ReviewId`, `Url`, `PublicId`, and `MediaType`.
- A submitted review is visible immediately; Admin can hide it through `IsVisible`.
- A review cannot be edited. To change it, the customer deletes it and creates a new one.

## Repository Findings

- Backend already has the `Review` model, `Reviews` DbSet, navigation properties, and the initial `Reviews` migration table.
- Backend has `OrderStatus.Completed`, `OrderItems`, JWT authorization, and Cloudinary integration.
- Backend does not yet have review DTOs, service, controller, media model, or review endpoints.
- The current Admin media upload endpoint is Admin-only, so customer review uploads need a separately authorized flow.
- Frontend `Profile.tsx` already loads and renders order history, while `ProductDetail.tsx` currently has no review section.

## Open Questions

- Confirm exact per-file size and MIME-format limits during planning; the MVP should use explicit image/video allowlists and upload limits.
- Decide whether a customer can review the same product again when it is purchased in a different completed order. Recommended: yes, one review per `OrderId + ProductId`.

## Risks

- Client-side checks are insufficient; the backend must verify ownership, completed status, purchased product, rating range, media count/type, and duplicate review rules.
- Orphaned Cloudinary files can accumulate if uploads succeed but review creation fails, or if a review is deleted without deleting its media assets.
- A database model without a unique constraint can allow duplicate reviews for the same order item under concurrent requests.
- Immediate visibility needs basic abuse controls and an Admin hide/list flow even if full moderation is out of scope.
