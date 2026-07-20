# Phase 02 — Backend review API

## Objective

Implement verified review creation, public product review retrieval, owner deletion/recreation, order review state, and Admin visibility operations.

## Files to create

- `DTOs/ReviewDto.cs`
- `Interfaces/IReviewService.cs`
- `Services/ReviewService.cs`
- `Controllers/Review/ReviewsController.cs`
- `Controllers/Review/AdminReviewsController.cs`

## Files to modify

- `Extensions/DependencyInjection.cs` — register `IReviewService`.
- `Models/Review.cs` and `Models/ReviewMedia.cs` — keep navigation and validation annotations aligned with DTO limits.
- `DTOs/OrderDto.cs` — add `ReviewId`/review summary state to `OrderItemDto` without exposing hidden review content to the customer order list.
- `Services/OrderService.cs` — load/map review state efficiently; avoid a per-item N+1 query.
- `Interfaces/ICloudinaryService.cs` and `Services/CloudinaryService.cs` — consume deletion support from Phase 01.

## API contract

- `POST /api/reviews` — `[Authorize]`, `multipart/form-data`; fields `OrderId`, `ProductId`, `Rating`, `Comment`, and repeated `Files`.
- `GET /api/reviews/product/{productId}` — anonymous/public; query `page`, `pageSize`, optional `rating`, optional `withMedia`; returns paginated visible reviews plus summary/distribution.
- `DELETE /api/reviews/{reviewId}` — `[Authorize]`; owner-only hard delete followed by media cleanup.
- `GET /api/admin/reviews` — `[Authorize(Roles = "Admin")]`; paginated moderation list with optional visibility/product/rating filters.
- `PATCH /api/admin/reviews/{reviewId}/visibility` — Admin-only; body `{ "isVisible": boolean }`.

## Service rules

1. Resolve user identity only from `ClaimTypes.NameIdentifier`.
2. Load the order by `OrderId` and `UserId`; require `OrderStatus.Completed`.
3. Require an `OrderItem` whose `ProductId` matches the request. Scope duplicate detection to `(UserId, OrderId, ProductId)` and let the unique index protect concurrent requests.
4. Validate rating, comment length, file count, extension/MIME, image count, video count, and file sizes: images ≤10 MB each; videos ≤50 MB each.
5. Set `IsVisible = true` and upload to a fixed review folder only after business validation. If any later upload or database step fails, delete all assets uploaded in that request before returning the error.
6. Return a verified-purchase marker based on the validated order relationship, not on client input.
7. On owner delete, authorize against `Review.UserId`, remove database rows in a transaction, then attempt Cloudinary deletion for each saved `PublicId` using the stored media type; log failures with review/media IDs.
8. Public queries filter `IsVisible = true`, order newest first, calculate summary and distribution from visible rows, and use pagination.

## Verification

- Exercise every business rejection in the spec through API tests/smoke requests.
- Confirm hidden reviews never appear in public results.
- Confirm Admin cannot be reached by Staff/customer tokens.
- Confirm duplicate concurrent inserts result in one persisted row and a controlled conflict for the other request.
