# Phase 04: Public traceability page

## Goal

Add the anonymous root-level landing page that renders the correct sanitized Vietnamese or English story without disturbing existing storefront routes.

## Stories Covered

- P1: customer opens `/arabica-and-lamp` or `/robusta-and-lamp` and reads localized content.

## Frontend Files

- Add `src/services/api/productStories.ts`.
- Add `src/pages/ProductStory.tsx`.
- Add focused styles under `src/styles/` using current design tokens.
- Update `src/App.tsx` and both locale JSON files.
- Add `vitest.config.ts` or the minimal Vite-compatible test configuration, `src/test/setup.ts`, and focused tests.
- Update `package.json`/lockfile with Vitest, React Testing Library, `@testing-library/jest-dom`, and jsdom plus a deterministic `test` script.

## Tasks

1. Define strict TypeScript DTOs and unwrap the backend's `ApiResponse<T>` consistently.
2. Add `/:storySlug` as a public `MainLayout` route; retain all existing explicit routes unchanged.
3. Fetch by slug with cancellation on unmount and explicit loading, not-found, general-error, and success states.
4. Select `contentHtmlVi` or `contentHtmlEn` from `LanguageContext`; switching language updates content without another API request.
5. Render only the backend-sanitized HTML in a constrained content container. Do not merge arbitrary classes/styles from the content into the application shell.
6. Show coffee type and product identity as first-viewport content, with a clear link to the associated product detail when active.
7. Keep headings, lists, links, long words, and bilingual text responsive at 390px and desktop widths.
8. Confirm Vercel deep-link refresh resolves to `index.html` and `/api/*` continues to proxy before the catch-all rewrite.
9. Reserve a P2 effect for the page-open POST; GET itself remains side-effect free.
10. Add route/component tests for localized success, status states, and regression coverage for existing one-segment routes.

## Tests and Browser Checks

- Existing `/products`, `/checkout`, `/profile`, `/environmental-impact`, and `/admin` routes still resolve to their original screens.
- Published story success, unknown/draft 404, API failure, and language switch.
- The page renders only the public DTO's server-sanitized fields; legacy/direct-database malicious fixtures are removed by the public API read-path sanitizer.
- Keyboard navigation, visible focus, 390px layout, desktop layout, and direct refresh at `/arabica-and-lamp`.

## Verification

```bash
npm test -- --run
npm run build
```

Run browser smoke checks against the local FE with a backend fixture or completed Phase 03 API.
