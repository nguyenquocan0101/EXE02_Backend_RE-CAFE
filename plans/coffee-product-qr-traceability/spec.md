# Spec: Coffee-product QR traceability pages

**Date:** 2026-08-04
**Status:** Ready

---

## Problem Statement

RE:CAFE products need scannable public pages that explain which coffee material is associated with each product type. Admins currently have no API or dashboard for creating these coffee-product combinations, publishing bilingual HTML, or generating stable QR links.

---

## User Stories

- **[P1]** As an Admin, I want to select a product and a coffee type from dropdowns and enter Vietnamese and English HTML so that I can publish a traceability page with minimal manual input.
  Accepted when: one save operation creates the coffee-product story, a stable slug, a canonical public URL, and a shared unlimited-use QR code.

- **[P1]** As a customer, I want to scan a QR code and open a public path such as `/arabica-and-lamp` so that I can read the appropriate coffee and product story in Vietnamese or English without signing in.
  Accepted when: the public page resolves by slug, follows the site's current language selection, renders sanitized HTML, and returns a not-found state when unpublished or unknown.

- **[P1]** As an Admin, I want a dashboard that lists every coffee-product page so that I can preview it, download its QR code, edit bilingual content, and publish or unpublish it.
  Accepted when: all four actions are available from the dedicated Admin area and do not require manually editing a URL or QR value.

- **[P1]** As an Admin, I want the same product to support multiple coffee types so that pages such as `/arabica-and-lamp` and `/robusta-and-lamp` can exist simultaneously.
  Accepted when: each `(ProductId, CoffeeTypeId)` pair is unique while one product can own two or more story records.

- **[P2]** As an Admin, I want to see public-page open counts so that I can understand which QR stories receive traffic without performing any extra setup.
  Accepted when: the dashboard displays a total count per story and the label does not imply that every direct page visit was a verified camera scan.

- **[P2]** As an Admin, I want to activate, deactivate, add, or rename coffee types so that the dropdown can evolve without a code deployment.
  Accepted when: coffee-type changes are available in Admin, while seeded types are ready without initial setup and existing published slugs remain unchanged.

- **[P3]** As a customer, I want a unique QR for my physical item so that I can see its source cup, collection date, recycling date, production batch, and resulting product.
  Accepted when: a future per-unit identifier resolves beneath or alongside the stable coffee-product page without breaking existing shared QR codes.

---

## Functional Requirements

1. **FR-01:** Add a coffee-type master catalog with `Name`, unique `Slug`, `IsActive`, and deterministic display order.
2. **FR-02:** Seed these active coffee types: `Arabica`, `Robusta`, `Liberica`, `Excelsa`, `Culi`, `Moka`, `Catimor`, and `Blend`.
3. **FR-03:** Change the traceability relationship so one `Product` can have multiple coffee-product story records, with a unique database constraint on `(ProductId, CoffeeTypeId)`.
4. **FR-04:** A story must store its product, coffee type, stable unique slug, Vietnamese HTML, English HTML, publication state, creation time, and update time.
5. **FR-05:** Both Vietnamese and English HTML are required, non-blank, and limited to 50 KB each before sanitization.
6. **FR-06:** Sanitize HTML on the backend using an explicit allowlist. Remove scripts, inline event handlers, `javascript:` URLs, unsafe embeds, and other executable markup before persistence or public return.
7. **FR-07:** Generate the initial slug as `{coffee-type-slug}-and-{product-slug}`, for example `arabica-and-lamp`; reject unresolved collisions with an actionable validation message.
8. **FR-08:** Persist the generated slug. Renaming a product or coffee type must not change an existing story URL or downloaded QR destination.
9. **FR-09:** Generate one shared unlimited-use QR record per coffee-product story. Its encoded value must be the canonical production URL `https://www.recafe.site/{slug}`.
10. **FR-10:** Expose an anonymous public read endpoint by slug that returns only published story data required by the landing page.
11. **FR-11:** Add a public React route at `/{slug}` after all existing explicit routes. Existing storefront and Admin routes must keep precedence.
12. **FR-12:** The public page must support the site's existing Vietnamese/English language switch and render the matching sanitized HTML.
13. **FR-13:** Add Admin APIs, restricted to the `Admin` role, to list, read, create, update, publish, and unpublish coffee-product stories.
14. **FR-14:** Add a dedicated Admin page with a compact list showing product, coffee type, public URL, publication state, and page-open count when analytics is enabled.
15. **FR-15:** The create form requires only product selection, active coffee-type selection, Vietnamese HTML, and English HTML. Slug, URL, QR value, and initial published state are automatic.
16. **FR-16:** The Admin page must provide edit, publish/unpublish, public preview, copy-link, and QR download actions. QR download must produce a print-usable PNG or SVG.
17. **FR-17:** Do not hard-delete a published story through the normal dashboard. Unpublishing preserves its slug and QR destination for later restoration.
18. **FR-18:** Unknown, inactive, or unpublished slugs must return a consistent not-found response and must not expose draft HTML.
19. **FR-19:** If page-open analytics is included, record at most the minimum data needed for a count; raw IP address and full device details are not required for the MVP.
20. **FR-20:** Adapt the existing `ProductStory`, `QRCode`, and related EF Core mappings rather than introducing a parallel QR subsystem. Existing production-batch fields may become optional until per-unit traceability is implemented.

---

## Non-Functional Requirements

- **Performance:** Public story API p95 response time is under 500 ms for 10,000 story records, excluding network latency and frontend asset loading.
- **Security:** All Admin mutations require the `Admin` role; public HTML contains no executable scripts, inline event handlers, unsafe URL schemes, or unsanitized markup.
- **Availability:** Once published, a story keeps the same slug and QR destination across product-name and coffee-type-name edits; unpublishing is reversible.
- **Accessibility:** The public page and Admin form meet keyboard navigation requirements, preserve visible focus, and expose meaningful labels for language, status, preview, copy, and download controls.
- **Compatibility:** Existing product routes, Admin routes, product API contracts, and current database data continue to work after migration.

---

## Success Criteria

- [ ] Admin creates an `Arabica + Lamp` story with four manual inputs and one save action; the system returns `/arabica-and-lamp` and a downloadable QR.
- [ ] Admin creates `Arabica + Lamp` and `Robusta + Lamp` concurrently, while a duplicate `Arabica + Lamp` request is rejected.
- [ ] Both public URLs load without authentication and switch between the saved Vietnamese and English HTML.
- [ ] HTML containing `<script>`, an `onerror` handler, or a `javascript:` URL is removed or rejected and cannot execute in the public page.
- [ ] Renaming the selected product or coffee type does not alter an existing public slug or QR destination.
- [ ] Unpublishing a story makes the public route unavailable; republishing restores the same URL.
- [ ] Existing frontend routes and backend product flows continue to pass their current build and relevant test suites.
- [ ] Admin users can edit, preview, copy, publish/unpublish, and download QR; non-admin users receive HTTP 401/403 from every mutation endpoint.
- [ ] Public story lookup meets p95 under 500 ms with 10,000 seeded story records.

---

## Out of Scope

- Unique QR codes or serial numbers for each physical product unit.
- Verified source cup, collection date, recycling date, production event timeline, or chain-of-custody evidence.
- Production-batch and coffee-ground-batch management UI.
- Loyalty points awarded from QR scans.
- QR expiration or per-unit scan limits.
- WYSIWYG page builder, media library, templates, or arbitrary JavaScript/CSS in story HTML.
- Proof that a page open originated from a camera scan rather than a copied or directly entered link.

---

## Assumptions

- Each catalog `Product` represents a product type such as Lamp, while a coffee-product story represents one coffee association such as Arabica + Lamp.
- The public URL must remain at the site root in the form `https://www.recafe.site/{coffee-slug}-and-{product-slug}`.
- Admins are trusted content authors, but their HTML is still treated as untrusted input for browser security.
- The initial coffee list is a product taxonomy for the dropdown, not a claim that every entry is a distinct botanical species.
- Vietnamese and English content are both required for publication.
- The existing traceability models contain no production data that prevents changing the current one-to-one story relationship.

---

## [NEEDS CLARIFICATION]

<!-- None. -->
