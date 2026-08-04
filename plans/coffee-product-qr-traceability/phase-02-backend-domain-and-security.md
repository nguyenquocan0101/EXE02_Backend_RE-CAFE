# Phase 02: Backend domain and HTML security

## Goal

Implement the business layer that creates stable coffee-product stories, sanitizes bilingual HTML, and manages their publication state transactionally.

## Stories Covered

- P1: minimal-input Admin creation.
- P1: stable bilingual public content.
- P1: multiple coffee types per product.

## Backend Files

- Add `DTOs/ProductStoryDto.cs` and `DTOs/CoffeeTypeDto.cs`.
- Add `Interfaces/IProductStoryService.cs` and `Interfaces/IStoryHtmlSanitizer.cs`.
- Add `Services/ProductStoryService.cs` and `Services/StoryHtmlSanitizer.cs`.
- Add `Models/TraceabilitySettings.cs` or an equivalent options record.
- Update `Extensions/DependencyInjection.cs`, `Program.cs`, configuration examples, and the backend project package references.
- Add focused service/security tests.

## Tasks

1. Add `HtmlSanitizer` and configure one immutable singleton allowlist for semantic content only.
2. Allow a small set such as headings, paragraphs, strong/emphasis, lists, blockquote, links, line breaks, and horizontal rules. Disallow forms, controls, script/style, iframe/object/embed, event attributes, and arbitrary inline CSS.
3. Permit only `http` and `https` link schemes; normalize external links to safe `rel` behavior.
4. Validate each raw HTML input is non-blank and at most 50 KB, sanitize it, then verify the resulting text/content is still meaningful.
5. Validate `Traceability:PublicBaseUrl` as an absolute HTTPS URL outside Development; trim trailing slashes before URL composition.
6. Create a story in one transaction: resolve active product and coffee type, generate slug, sanitize both HTML fields, create an immediately published story, create shared QR, and save.
7. Set both `QRCode.QRValue` and `LandingPageUrl` to the canonical story URL; set `IsShared = true`, `ScanLimit = null`, and no expiry.
8. Map unique constraint failures to stable business errors for duplicate pair or slug collision.
9. Keep `ProductId`, `CoffeeTypeId`, `Slug`, and QR destination immutable in updates; update only sanitized bilingual content and timestamps.
10. Publish/unpublish through an explicit method. Publication must reject blank/invalid bilingual content.
11. Public lookup returns only active-product, active-coffee-type, published story data with no Admin/internal fields, and sanitizes the persisted HTML again before returning it.
12. Reject generated story slugs over 200 characters or canonical URLs over 250 characters with actionable validation instead of relying on database truncation/errors.

## Tests

- Slug and canonical URL generation, including trailing-slash configuration.
- Duplicate pair and slug collision handling, including concurrent writes where practical.
- Product/coffee type missing or inactive failures.
- Script, event attribute, JavaScript URL, iframe, form, malformed HTML, and oversized input sanitization.
- Sanitizer output is stable across create and update.
- Directly inserted or legacy unsafe HTML is sanitized again by public lookup.
- Oversized generated slug/canonical URL fails before database persistence.
- Updating product/coffee names or slugs does not change an existing story slug or URL.
- Unpublish/republish preserves identity and QR destination.

## Verification

```bash
dotnet test tests/EXE02_Backend_RE-CAFE.Tests/EXE02_Backend_RE-CAFE.Tests.csproj --filter ProductStoryService
```
