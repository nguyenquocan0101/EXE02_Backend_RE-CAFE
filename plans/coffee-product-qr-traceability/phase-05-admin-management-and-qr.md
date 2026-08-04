# Phase 05: Admin management and QR download

## Goal

Provide a low-friction Admin workflow: four inputs and one save, followed by direct preview, copy-link, publication, edit, and QR download actions.

## Stories Covered

- P1: minimal-input creation.
- P1: Admin dashboard management.
- P1: multiple coffee pages per product.

## Frontend Files

- Add `src/pages/admin/AdminProductStories.tsx`.
- Add `src/pages/admin/ProductStoryModal.tsx`.
- Add `src/components/traceability/StoryQrCode.tsx` or a similarly scoped QR component.
- Extend `src/services/api/admin.ts` or add `src/services/api/adminProductStories.ts` following the existing API pattern.
- Update `src/App.tsx`, `src/layouts/AdminLayout.tsx`, locale files, `package.json`, and lockfile.

## Tasks

1. Add `qrcode.react` and render `QRCodeSVG` with the canonical URL, four-module margin, accessible title, and print-safe foreground/background colors.
2. Add an Admin-only `/admin/product-stories` route and a concise `QR & truy xuat` sidebar item using the existing protected-route pattern.
3. Build a list with product, coffee type, URL, publication state, updated time, and P2 page-open count.
4. Add filters/search without blocking the core create action.
5. Create modal fields: product dropdown, active coffee-type dropdown, Vietnamese HTML textarea, and English HTML textarea. Do not expose slug, URL, QR value, or publication mechanics as required inputs.
6. On save, show the generated URL and QR immediately from the response.
7. In edit mode, lock product and coffee type; allow bilingual content edits only.
8. Add publish/unpublish, preview, copy-link, and QR download actions with loading, disabled, success, and error states. Draft preview renders only persisted, sanitized Admin DTO content in a sandboxed preview; published preview may open the canonical public URL.
9. Download SVG by serializing the rendered element with an SVG namespace and a deterministic filename such as `arabica-and-lamp-qr.svg`.
10. Do not offer hard delete. Unpublished records remain in the list and can be restored.
11. Keep Admin copy in both locale files even if the current Admin surface contains legacy Vietnamese literals.
12. Add React Testing Library coverage for create/edit validation, duplicate errors, draft/published actions, and deterministic QR URL/download behavior.

## Tests and Browser Checks

- Create requires exactly four content selections/inputs and one save action.
- Product and coffee dropdowns load independently and expose errors/retry states.
- Duplicate pair error is shown without closing or clearing the form.
- Edit cannot mutate the pair or slug.
- Draft preview works without making the story public and does not render unsaved raw HTML.
- Copy link uses the canonical URL, and downloaded QR decodes to the exact same URL.
- Non-admin navigation does not expose the page; backend authorization remains the enforcement boundary.
- Modal focus, textarea labels, status controls, mobile/desktop table behavior, and long bilingual content.

## Verification

```bash
npm test -- --run
npm run build
```

Use a QR decoder in the verification harness to validate generated output rather than relying only on camera/manual scanning.
