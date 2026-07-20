# Plan: Customer product reviews after purchase

**Date:** 2026-07-20
**Mode:** Hard
**Risk:** high-risk — multi-repository schema changes, authenticated ownership checks, Cloudinary uploads/deletes, and public/admin user-facing behavior.
**Source spec:** `plans/customer-product-review/spec.md`
**Status:** Ready for implementation

## Scope Challenge

- **Exists?** Partial foundation only: `Review` model/table exists, but there are no review endpoints, service, media model, or frontend review flow.
- **Minimum?** Verified review creation from a `Completed` order, 1–5 stars, comment, 2 images + 1 video, owner deletion/recreation, public product display, and Admin visibility toggle.
- **Complexity?** Hard.
- **Project roots:** Backend `W:\DevPool\EXE02_Backend_RE-CAFE`; frontend `W:\DevPool\RECAFE_EXE01\RECAFE_EXE01`.

## Research Summary

### Primary approach — server-owned multipart review creation

Use one authenticated `multipart/form-data` create endpoint. The browser submits `OrderId`, `ProductId`, `Rating`, `Comment`, and files; the backend verifies ownership and order status before uploading files to Cloudinary and saving `ReviewMedia`. This avoids trusting client-supplied media URLs and makes validation/cleanup one server-owned flow.

### Alternative considered — separate media upload then JSON review creation

Reuse the existing upload pattern and send media metadata in a second request. This creates orphan-cleanup risk when the user abandons the form or review creation fails, and the existing upload endpoint is Admin-only. Keep this as a future optimization only if large uploads require an asynchronous flow.

### Repository constraints discovered

- `OrderStatus.Completed` is the only current fulfilled state.
- `Review` already has `UserId`, `ProductId`, `OrderId`, `Rating`, `Comment`, `IsVisible`, and `CreatedAt`; the initial migration already creates `Reviews`.
- `ICloudinaryService` currently uploads but has no deletion operation; deletion support must be added before review deletion is implemented.
- `Profile.tsx` renders order history in a compact table; the review action should open an existing-style modal and operate per distinct order product.
- `ProductDetail.tsx` has no review section; add a dedicated component before related products.
- No `.ck.json` or existing `feature_list.json` was found in either project root, so feature-list tracking is not generated.

## Architecture Decisions

1. Store media in a new `ReviewMedia` table with `Id`, `ReviewId`, `Url`, `PublicId`, `MediaType`, and `CreatedAt`. Configure cascade delete from Review to ReviewMedia.
2. Enforce one review per `(UserId, OrderId, ProductId)` with a database unique index. A customer changes feedback by deleting then recreating it.
3. Keep review creation server-owned with `multipart/form-data`; never accept a client-supplied `UserId` or arbitrary Cloudinary folder.
4. Use `recafe/reviews/{productId}` as the Cloudinary folder and persist secure URL plus public ID for cleanup.
5. Add a public paginated product-review query that filters `IsVisible = true` and returns rating summary, distribution, media, display name, date, and verified-purchase state.
6. Add Admin-only list and visibility-toggle endpoints. Public users never receive hidden reviews.
7. Extend order item DTOs with review state so the Profile UI can show `Đánh giá`, `Đã đánh giá`, and `Xóa đánh giá` without relying on a failing create request to discover duplicates.

## Phase Overview

| Phase | Goal | Depends on |
|---|---|---|
| 01 | Schema, media lifecycle, and migration foundation | — |
| 02 | Backend review domain and API | 01 |
| 03 | Customer review journey and product display | 02 |
| 04 | Admin moderation surface | 02 |
| 05 | Verification, migration rehearsal, and release checklist | 01–04 |

## Phase Checklist

- [x] Phase 01: Data and media foundation
- [x] Phase 02: Backend review API
- [x] Phase 03: Customer frontend flow
- [x] Phase 04: Admin moderation
- [ ] Phase 05: Verification and rollout

## Phase Details

- [Phase 01 — Data and media foundation](phase-01-data-media-foundation.md)
- [Phase 02 — Backend review API](phase-02-backend-review-api.md)
- [Phase 03 — Customer frontend flow](phase-03-customer-frontend-flow.md)
- [Phase 04 — Admin moderation](phase-04-admin-moderation.md)
- [Phase 05 — Verification and rollout](phase-05-verification-rollout.md)

## Risks and Mitigations

- **Existing duplicate reviews can block the unique index.** Before applying the migration, run a duplicate query on `(UserId, OrderId, ProductId)`; resolve any rows before adding the constraint.
- **Cloudinary files can become orphaned.** Upload only after business validation, delete already-uploaded assets when DB save fails, and log best-effort deletion failures after review deletion.
- **Large multipart requests can be rejected by ASP.NET before validation.** Set an explicit action/request limit above the 50 MB video limit and validate the individual file limits in the service.
- **MIME values can be spoofed.** Validate both normalized extension and allowed MIME type; do not use the original filename for authorization or path construction.
- **Admin route role mismatch.** Backend review moderation remains Admin-only even though the existing frontend admin shell allows Staff for some order screens.

## Verification Matrix

| Requirement | Verification |
|---|---|
| Completed-order gate | API smoke tests for Pending, Shipping, Completed, Cancelled, and Returned orders |
| Product ownership within order | Create review with an unrelated product ID and expect 400/403 |
| Duplicate rule | Create twice for the same order/product and test concurrent requests against the unique index |
| Media limits | Test 0–2 valid images, 0–1 valid video, 3 valid files, 3 images, 2 videos, invalid MIME, and size overages |
| Delete/recreate | Delete as owner, reject delete as another user, then create replacement review |
| Public visibility | Toggle `IsVisible` as Admin and verify the next public GET excludes/includes the review |
| Frontend behavior | `npm run build`, then manually verify Profile modal, previews, delete/recreate, mobile layout, and ProductDetail review list |
| Backend integrity | `dotnet build`, apply migration on a disposable database, and inspect generated schema/indexes |

## Handoff

Implementation should proceed with `$ck-cook --hard --tdd plans/customer-product-review/plan.md` after any final API naming decision. The exact file names and endpoint shapes are specified in the phase files below.

## Red-Team Review

- **Accepted:** Add a composite public-read index `(ProductId, IsVisible, CreatedAt)` because the existing Review indexes do not cover visibility-filtered pagination.
- **Accepted:** Add migration preflight for duplicate reviews before creating the unique index.
- **Accepted:** Use server-owned multipart creation to prevent arbitrary URL/public-ID injection.
- **Accepted:** Add cleanup logging and best-effort Cloudinary deletion; a full retry queue remains out of scope for this MVP.
- **Noted:** Per-file size limits are fixed at 10 MB for images and 50 MB for videos; the deployment proxy must allow the resulting request size.
- **Rejected:** Adding review replies, rewards, or automated moderation now; all are explicitly P3/out of scope in the spec.

## Session Notes
<!-- Updated by cook automatically — do not edit manually -->

**Last active:** 2026-07-20 21:50
**Phase in progress:** phase-04-admin-moderation
**Status:** Phase 04 implemented; Admin smoke checks green (8/8 + backend role guard), FE production build green, backend build green (0 warnings, 0 errors), and CSS/accessibility static review completed.

### Decisions made this session

- Review creation uses server-owned multipart upload with `OrderId`, `ProductId`, `Rating`, `Comment`, and repeated `Files`.
- Customer order DTOs expose `ReviewId`/`HasReview` so the frontend can show delete/recreate state without probing duplicate errors.
- Public review reads filter `IsVisible`; Admin reads and visibility updates use a separate Admin-only controller.
- Customer FE reuses the shared Modal and Green Deck palette; interactive review controls use visible focus rings and 44px touch targets.
- Added owner-only `GET /api/reviews/{reviewId}` because an order row only exposes `ReviewId`, while delete/recreate UI must display the existing comment and media safely.
- Admin moderation remains Admin-only at both route and controller boundaries; product keyword filtering is server-side through `ProductKeyword`.
- Admin visibility state changes update the table only after a successful PATCH; media preview uses the shared portal Modal and no Admin hard-delete is exposed.

### Next immediate action

Await human approval at the `--hard` phase gate before starting Phase 05 verification and rollout work.
