# Phase 05 — Verification and rollout

## Objective

Verify the cross-repository flow, rehearse the migration, and prepare safe deployment of the review feature.

## Pre-release checks

1. Run the duplicate-review preflight against the target database before migration.
2. Apply the EF migration to a disposable PostgreSQL database and verify `ReviewMedia`, cascade FK, unique review index, and public-read index.
3. Run backend `dotnet build` and frontend `npm run build`.
4. Run API smoke tests for authentication, Completed gate, product-in-order gate, duplicate rule, rating/comment validation, media limits, owner deletion, public visibility, and Admin moderation.
5. Test Cloudinary failure paths: image upload failure, video upload failure, DB save failure after upload, and deletion failure; verify no review is exposed as partially created and failures are logged.
6. Test frontend in Vietnamese and English, authenticated and unauthenticated states, empty/error/loading states, mobile modal layout, and media preview playback.
7. Confirm the reverse-proxy/request limit is above the maximum multipart request size needed for one 50 MB video plus two 10 MB images and multipart overhead.

## Rollout order

- Deploy backend migration/API first.
- Deploy frontend after the API is available.
- Monitor review create/delete errors and Cloudinary cleanup failures.
- If rollback is needed, hide the UI entry points first; preserve the additive ReviewMedia table and migration unless a database rollback is explicitly approved.

## Done when

- All P1 acceptance criteria in `spec.md` have evidence from API/UI verification.
- No source of truth remains in client-provided owner IDs or arbitrary media URLs.
- Migration and builds succeed in a disposable/release-like environment.
