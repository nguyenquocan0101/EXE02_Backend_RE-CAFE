# Plan: Coffee-product QR traceability pages

**Date:** 2026-08-04
**Validated:** 2026-08-05
**Mode:** Hard
**Risk:** high-risk — changes the PostgreSQL schema, adds public HTML rendering, and spans authenticated Admin APIs plus an anonymous route.
**Status:** P1 and P2 implementation complete; production backup/rollout monitoring remain operational release actions
**Spec:** `plans/coffee-product-qr-traceability/spec.md`
**Repositories:** backend root and `W:\DevPool\RECAFE_EXE01\RECAFE_EXE01`

---

## Scope Challenge

- **Exists?** The database already has `ProductStory`, `QRCode`, `QRScanLog`, coffee-ground batch, and production-batch models. No application service, endpoint, public page, or Admin UI currently uses them.
- **Minimum?** P1 still requires a schema adaptation, public/Admin APIs, one public FE route, and one Admin surface. P2 coffee-type maintenance and page-open analytics can remain isolated in the final phase.
- **Complexity:** Hard. The migration and HTML trust boundary need explicit review and regression coverage.

## Spec Quality Check

- No unresolved `[NEEDS CLARIFICATION]` items.
- P1/P2/P3 priorities are present.
- Acceptance conditions and success criteria are independently testable.
- **Verdict:** PASS.

---

## Selected Architecture

Extend the existing traceability domain instead of introducing a parallel page/QR system:

```text
Product 1 ─── * ProductStory * ─── 1 CoffeeType
                    │
                    └── * QRCode (one shared QR in MVP; per-unit QR later)
```

`ProductStory` becomes the coffee-product landing-page record. Existing batch-oriented fields remain nullable and preserved for future traceability. Each story stores a stable slug and sanitized Vietnamese/English HTML.

The backend stores the canonical URL but not a QR bitmap. The frontend renders that URL as SVG with `qrcode.react`, then serializes the SVG for download. This keeps generated media deterministic and avoids Cloudinary/storage work.

## Key Decisions

1. **Stable identity:** `ProductId` and `CoffeeTypeId` are fixed after creation. Content and publication state can change; changing the pair means creating a new story.
2. **Stable URL:** slug is generated once from `{coffeeType.Slug}-and-{product.Slug}` and never follows later name/slug edits.
3. **Canonical host:** `Traceability:PublicBaseUrl` supplies `https://www.recafe.site` in production and can be overridden locally.
4. **HTML security:** sanitize on write and again on public read with a strict semantic-markup allowlist. Do not allow forms, scripts, iframes, embedded CSS, inline event attributes, or unsafe URI schemes.
5. **Preview safety:** the Admin editor does not render unsaved raw HTML in the main document. Draft preview uses persisted, sanitized Admin DTO content inside a sandboxed preview; published records may also open the public page.
6. **Shared QR:** add `IsShared`; shared records have `ScanLimit = null`, no expiry, and one shared record per story enforced with a PostgreSQL partial unique index.
7. **Analytics semantics:** GET remains side-effect free. A separate best-effort POST increments `ScanCount`; the UI labels it `Lượt mở trang`, not verified scans.
8. **Route:** add `/:storySlug` under `MainLayout`. React Router v6 ranks existing static routes above this dynamic one; Vercel already rewrites non-API paths to `index.html`.
9. **Coffee list:** seed exactly the eight active choices from the spec. Production has no existing `ProductStory` rows, so do not add legacy-only coffee types or backfill behavior.
10. **Length contract:** coffee slugs are limited to 80 characters, generated story slugs to 200 characters, and canonical URLs to the existing 250-character `QRValue` limit. Creation fails before persistence if the generated values exceed those limits.
11. **Initial publication:** a successful create publishes the story immediately. Admin can unpublish it afterward without changing its URL or QR.

## Proposed API Surface

### Public

| Method | Route | Behavior |
| --- | --- | --- |
| `GET` | `/api/product-stories/{slug}` | Return one published story with product, coffee type, bilingual sanitized HTML, canonical URL, and updated time. |
| `POST` | `/api/product-stories/{slug}/open` | P2: atomically increment the shared QR page-open count; return `204`. |

### Admin

| Method | Route | Behavior |
| --- | --- | --- |
| `GET` | `/api/admin/product-stories` | Paginated/filterable list with status, URL, and count. |
| `GET` | `/api/admin/product-stories/{id}` | Full editor DTO. |
| `POST` | `/api/admin/product-stories` | Create from product, coffee type, HTML VI, and HTML EN; generate all derived values. |
| `PUT` | `/api/admin/product-stories/{id}` | Update bilingual HTML only. |
| `PATCH` | `/api/admin/product-stories/{id}/publication` | Publish/unpublish without changing slug. |
| `GET` | `/api/admin/coffee-types` | Return all coffee types for management; active-only query feeds the story form. |
| `POST` | `/api/admin/coffee-types` | P2: create a type. |
| `PUT` | `/api/admin/coffee-types/{id}` | P2: update display fields/order. Existing story slugs stay unchanged. |
| `PATCH` | `/api/admin/coffee-types/{id}/active` | P2: activate/deactivate a type. |

All responses follow the existing `ApiResponse<T>` envelope except the `204` analytics mutation.

## Data Migration Strategy

1. Add a migration precondition that fails clearly if `ProductStories` contains rows. The user confirmed production has none; do not invent a legacy migration path.
2. Create `CoffeeTypes` and seed deterministic IDs for exactly the eight active values.
3. Add required story columns: `CoffeeTypeId`, `Slug`, `ContentHtmlVi`, `ContentHtmlEn`, `IsPublished`, `CreatedAt`, and `UpdatedAt`; make `ProductionBatchId` and legacy narrative fields nullable.
4. Drop the unique product-only story index; add unique indexes for `Slug` and `(ProductId, CoffeeTypeId)`.
5. Add `QRCode.IsShared`, make `ScanLimit` nullable, and add a partial unique index for shared records with non-null `ProductStoryId`. Existing product-level QR rows remain non-shared.
6. Review generated `Up`, `Down`, model snapshot, SQL, and the production preflight result before deployment.
7. Generate the feature migration after the existing `20260727122159_SeedCustomerPaymentDemo` migration without editing or regenerating that user-owned migration.

## Delivery Phases

| Phase | Scope | Stories |
| --- | --- | --- |
| [01](phase-01-database-foundation.md) | Models, mappings, safe migration, seeds | P1 multi-coffee foundation |
| [02](phase-02-backend-domain-and-security.md) | DTOs, sanitizer, slug/URL generation, services | P1 create/edit/public behavior |
| [03](phase-03-backend-api-and-contract-tests.md) | Public/Admin controllers, auth, integration tests | All P1 backend contracts |
| [04](phase-04-public-traceability-page.md) | FE public API client, root route, bilingual page | P1 customer story |
| [05](phase-05-admin-management-and-qr.md) | Admin list/editor/actions/QR SVG | P1 Admin workflows |
| [06](phase-06-analytics-catalog-and-release.md) | Deferred follow-up: P2 coffee catalog, page opens, performance and release checks | P2; not part of the first cook run |

**Implementation run:** execute Phases 01–06, with P2 analytics/catalog delivered after the P1 surface passed its verification gates.

## Phase Checklist

- [x] Phase 01 — database foundation, model contracts, seed catalog, reviewed migration SQL
- [x] Phase 02 — backend domain and security
- [x] Phase 03 — backend API and contract tests
- [x] Phase 04 — public traceability page
- [x] Phase 05 — Admin management and QR
- [x] Phase 06 — analytics, coffee catalog, and release checks

## Session Notes

**Last active:** 2026-08-05 02:00
**Phase:** 05 — Admin Management and QR
**Status:** P1 implementation complete; final cross-repository review and git delivery in progress.

- TDD red run confirmed missing model contracts before implementation.
- Green run: `ProductStoryModelTests`, 5 passed, 0 failed.
- Backend build passed with 0 warnings and 0 errors.
- Generated migration `20260804183224_AddCoffeeProductQrTraceability` after `20260727122159_SeedCustomerPaymentDemo`.
- Migration SQL was reviewed and corrected to fail before schema mutation when `ProductStories` is non-empty, with no placeholder defaults on new required story columns.
- Phase 02: `StoryHtmlSanitizerTests` and `ProductStoryServiceIntegrationTests` pass; HTML is sanitized on write, Admin read, and public read.
- Phase 03: `ProductStoryApiContractTests` pass for public read, Admin lifecycle, role boundaries, duplicate conflict, and page-size clamping.
- Phase 04: public `/:storySlug` route, cancellation, bilingual selection, loading/error/404 states, responsive story styling, and route tests are implemented.
- Phase 05: Admin dashboard, four-field modal editor, persisted draft preview sandbox, copy-link, publication controls, and deterministic SVG QR download are implemented.
- Phase 06: page-open event is atomic/best-effort, catalog CRUD prevents hiding published stories, and the FE has a secondary catalog manager.
- Full backend suite before Phase 06: 38 passed, 0 failed. Focused Phase 06 suite: 3 passed, 0 failed, including 10,000-story p95 slug lookup. Full FE Vitest: 2 passed, 0 failed. FE production build passed.

### Next Immediate Action

Run final diff/security review, commit only feature-owned files in each repository, and push both `main` branches.

## Verification Strategy

- Backend: `dotnet build EXE02_Backend_RE-CAFE.csproj --no-restore` and the full xUnit project.
- Migration: fresh PostgreSQL database plus upgrade from the current schema with an empty `ProductStories` table; a separate negative fixture proves unexpected story data triggers the precondition before schema mutation.
- Security: sanitizer corpus covering scripts, event handlers, unsafe URLs, forms, iframes, malformed markup, and oversized content.
- API: anonymous published read, draft/unknown 404, Admin-only mutations, duplicate pair rejection, stable slug after related renames.
- Frontend: add Vitest, React Testing Library, `@testing-library/jest-dom`, and jsdom; run focused component/route tests plus the strict TypeScript production build and browser smoke checks at 390px and desktop.
- QR: decode a downloaded SVG/PNG in verification and assert the exact canonical URL rather than relying on visual inspection alone.
- Performance: seed 10,000 stories, inspect the slug lookup plan/index, and measure p95 below 500 ms in a repeatable integration/performance test.

## Research Basis

- `HtmlSanitizer` parses HTML with AngleSharp and supports explicit allowlists for tags, attributes, CSS, and URI schemes. Its defaults are broader than this feature needs, so configure a narrower policy: <https://github.com/mganss/HtmlSanitizer>
- NuGet reports the current `HtmlSanitizer` package as compatible with `net10.0`: <https://www.nuget.org/packages/HtmlSanitizer>
- `qrcode.react` supports SVG/Canvas output and a four-module margin; use SVG with `marginSize={4}` for print download: <https://github.com/zpao/qrcode.react>
- React Router v6 uses ranked matching, so current static one-segment routes beat `/:storySlug`: <https://reactrouter.com/6.30.4/start/overview>
- EF Core supports unique composite indexes; PostgreSQL-specific partial-index SQL must be reviewed in the generated migration: <https://learn.microsoft.com/en-us/ef/core/modeling/indexes>

## Risks and Controls

| Risk | Control |
| --- | --- |
| Unexpected production story data invalidates the empty-table assumption | Migration/preflight fails before schema mutation; stop and write a separate data-migration plan rather than guessing a backfill. |
| Stored XSS through Admin HTML | Strict server-side sanitizer, malicious-input tests, no unsanitized live preview. |
| Printed QR breaks after rename | Immutable persisted story slug and canonical URL. |
| Duplicate coffee/product page under concurrent requests | Unique database constraint plus friendly `DbUpdateException` mapping. |
| Root dynamic route masks storefront pages | Route-ranking regression tests for every existing one-segment public route. |
| QR is visually rendered but encodes the wrong host | Canonical URL config validation and decode-based QR verification. |
| Analytics count is inflated by bots/reloads | Label as page opens, keep it non-authoritative, and isolate it to P2. |
| Migration `Down` would lose new content | Document backup requirement; validate rollback only before production data is authored. |
| Dirty worktree contains a newer untracked migration | Treat it as an existing dependency; generate the traceability migration after it and review ordering/snapshot without modifying it. |

## Out of Scope

- Per-unit serial numbers and physical-item QR codes.
- Source cup, collection, recycling, production-batch timeline, or chain-of-custody proof.
- Loyalty awards, QR expiry/limits, WYSIWYG builder, media library, arbitrary CSS, or JavaScript.

## Validated Decisions

1. New stories publish immediately after a successful save.
2. Phases 01–05 ship P1 first; Phase 06 analytics and coffee-type CRUD are deferred to a separate run.
3. Add Vitest and React Testing Library to the frontend and include focused automated tests.
4. Production has no existing `ProductStory` rows. Keep the migration simple and fail safely if the precondition is unexpectedly false.
