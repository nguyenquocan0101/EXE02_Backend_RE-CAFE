# Spec: Customer product reviews after purchase

**Date:** 2026-07-20
**Status:** Ready

---

## Problem Statement

Customers need a trustworthy way to share product experience after receiving an order. The system should collect verified 1–5 star reviews with comments and media, then show useful feedback on product pages without allowing reviews from unfulfilled orders.

## User Stories

- **[P1]** As a customer, I want to review each product in a completed order so that my feedback is tied to a verified purchase.
  Accepted when: the review action is available only for an order with `Completed` status and only for products present in that order.

- **[P1]** As a customer, I want to submit a 1–5 star rating and an optional comment so that I can describe my experience.
  Accepted when: ratings outside 1–5 are rejected by the API and comments over 1,000 characters are rejected.

- **[P1]** As a customer, I want to attach up to 2 images and 1 video so that the review shows the product in use.
  Accepted when: the API rejects more than 3 files, more than 2 images, more than 1 video, unsupported media types, or invalid ownership/order data.

- **[P1]** As a customer, I want to delete my review and create a new one instead of editing it so that the review history follows the agreed product rule.
  Accepted when: the owner can delete their review; no update endpoint is exposed; a new review can be created after deletion.

- **[P1]** As a shopper, I want to see verified customer reviews on a product page so that I can make a better purchase decision.
  Accepted when: product detail shows visible reviews, average rating, total count, rating distribution, comment, media, creation date, and a “verified purchase” indicator.

- **[P1]** As an Admin, I want to hide an inappropriate review so that public product feedback remains useful.
  Accepted when: an authorized Admin can list reviews and toggle `IsVisible`; hidden reviews are excluded from public review queries.

- **[P2]** As a customer, I want to filter product reviews by star rating and media presence so that I can find relevant feedback quickly.
  Accepted when: product review queries support a star filter and a media-only filter without changing review ownership rules.

- **[P3]** _(out of scope — future)_ Review replies, likes, reward points, and automated content moderation.

---

## Functional Requirements

1. **FR-01:** Add `ReviewMedia` with `Id`, `ReviewId`, `Url`, `PublicId`, `MediaType`, and `CreatedAt`; configure a required Review relationship with cascade delete.
2. **FR-02:** Add a unique database constraint for `(UserId, OrderId, ProductId)` so a customer has at most one active review for a product in a specific order.
3. **FR-03:** Implement an authenticated create-review operation accepting `ProductId`, `OrderId`, `Rating`, `Comment`, and media metadata.
4. **FR-04:** On create, verify that the order belongs to the current user, has status `Completed`, and contains the requested product in `OrderItems`.
5. **FR-05:** Validate `Rating` from 1 through 5 and `Comment` at a maximum of 1,000 characters.
6. **FR-06:** Allow at most 3 media items per review: no more than 2 images and no more than 1 video. Accept `jpg`, `jpeg`, `png`, and `webp` images up to 10 MB each; accept `mp4` and `webm` videos up to 50 MB each.
7. **FR-07:** Store media in Cloudinary and persist its secure URL, public ID, and media type in `ReviewMedia`. Define cleanup behavior for failed review creation and review deletion.
8. **FR-08:** Implement owner-only delete for reviews. Deleting a review must also remove its `ReviewMedia` rows and attempt to delete corresponding Cloudinary assets.
9. **FR-09:** Implement public product-review retrieval that returns only `IsVisible = true` reviews and includes author display data, verified-purchase state, media, rating summary, and pagination.
10. **FR-10:** Extend product detail UI with rating summary and review list; extend completed order items in `Profile.tsx` with the review form/action.
11. **FR-11:** Implement Admin review listing and visibility toggle using existing Admin authorization. Public review results must reflect `IsVisible` immediately after the toggle.
12. **FR-12:** Return consistent API errors for unauthenticated access, non-completed orders, products not in the order, duplicate reviews, invalid media, and unauthorized deletion.

---

## Non-Functional Requirements

- **Performance:** Public review list endpoint should return the first page within p95 < 500 ms for a product with up to 10,000 reviews, excluding media upload time.
- **Security:** Every mutation must derive `UserId` from JWT claims; never trust a client-supplied owner ID. Validate MIME type, file size, file count, and Cloudinary folder server-side.
- **Consistency:** The database must enforce the duplicate rule with a unique index, not only an application-level pre-check.
- **Availability:** A failed media cleanup must not expose the review as partially created; cleanup failures must be logged for later retry.
- **UX:** The review form must show upload progress/errors and remain usable on mobile widths used by the existing Profile page.

---

## Success Criteria

- [ ] 100% of create-review requests for non-`Completed` orders are rejected by backend authorization/business validation.
- [ ] 100% of create-review requests for products not present in the referenced order are rejected.
- [ ] No user can create more than one review for the same `(UserId, OrderId, ProductId)` under concurrent requests.
- [ ] A valid review with 1–5 stars, comment, 0–2 images, and 0–1 video can be created and displayed on product detail.
- [ ] Deleting a review removes its database media rows and makes it possible to create a replacement review.
- [ ] Hidden reviews are absent from public product review results within the next read after an Admin visibility change.

---

## Out of Scope

- Editing an existing review.
- Review replies, likes, voting, rewards, or loyalty points.
- Full automated moderation or sentiment analysis.
- Reviews from orders that are Pending, Confirmed, Preparing, Shipping, Cancelled, or Returned.
- Anonymous reviews or reviews without a linked order.

---

## Assumptions

- `Completed` is the canonical delivered/fulfilled state in the current backend enum.
- A customer may review the same product again in a different completed order; uniqueness is scoped to `UserId + OrderId + ProductId`.
- `IsVisible = true` is the default for newly created reviews.
- Cloudinary remains the media provider used by the existing backend.
- The API must enforce the agreed file-size and MIME allowlists server-side.
