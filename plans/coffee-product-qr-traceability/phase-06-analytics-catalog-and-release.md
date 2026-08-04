# Phase 06: P2 analytics, coffee catalog, and release

**Deferred:** Do not implement this phase in the first `$ck-cook` run. Start it only through a separate follow-up request after Phases 01–05 pass verification.

## Goal

Complete optional zero-input analytics and coffee-type maintenance, then run cross-repository regression and deployment checks.

## Stories Covered

- P2: page-open count.
- P2: coffee-type maintenance.
- P3 remains documented and unimplemented.

## Backend Tasks

1. Add anonymous `POST /api/product-stories/{slug}/open`; return the same not-found behavior for unavailable stories and `204` on success.
2. Increment `QRCode.ScanCount` atomically in the database. Do not write raw IP or full device details for MVP.
3. Keep analytics best-effort and separate from public content loading so counting failure never blocks the story page.
4. Add Admin coffee-type create/update/activation endpoints. Names and display order may change; existing story slugs and URLs may not.
5. Prevent deactivation from hiding already-published stories without an explicit policy decision; default plan blocks deactivation while published stories reference the type.
6. Add tests for atomic increments, unavailable story behavior, coffee slug uniqueness, and referenced-type deactivation.

## Frontend Tasks

1. Fire the page-open POST once per mounted story page after successful content load; ignore/report failures without replacing content.
2. Label the metric `Lượt mở trang` / `Page opens` everywhere.
3. Add a compact coffee-type management modal or tab within the traceability Admin page; keep it secondary to story creation.
4. Support create, rename/display-order update, and active toggle with dependency errors shown clearly.

## Release Verification

1. Run full backend build and tests.
2. Run FE production build and focused tests if present.
3. Apply migration to a disposable copy of representative production data.
4. Seed 10,000 stories, verify unique indexes, inspect slug query plan, and measure p95.
5. Browser-test direct links, locale switching, Admin role boundaries, duplicate pairs, unpublish/republish, copy, and QR download.
6. Decode at least Arabica + Lamp and Robusta + Lamp QR files and compare their exact URLs.
7. Confirm production `Traceability__PublicBaseUrl=https://www.recafe.site`, CORS, Vercel rewrites, and API proxy.
8. Back up PostgreSQL before production migration and monitor migration/API logs, public 404 rate, and story lookup latency after release.

## Verification Commands

```bash
dotnet build EXE02_Backend_RE-CAFE.csproj --no-restore
dotnet test tests/EXE02_Backend_RE-CAFE.Tests/EXE02_Backend_RE-CAFE.Tests.csproj
npm run build
```

## Future Handoff

Per-unit QR work must start from a separate spec covering serial generation, label lifecycle, physical inventory linkage, batch/cup evidence, ownership/privacy, duplicate scans, and whether old shared URLs redirect or remain as parent stories.
