# Phase 03: Backend API and contract tests

## Goal

Expose the public story read contract and Admin management contract using the repository's current controller, authorization, exception, and response-envelope patterns.

## Stories Covered

- All four P1 stories at the backend boundary.

## Backend Files

- Add `Controllers/Product/ProductStoriesController.cs`.
- Add `Controllers/Product/AdminProductStoriesController.cs`.
- Add `Controllers/Product/AdminCoffeeTypesController.cs` or defer its mutations to Phase 06 while retaining the list endpoint.
- Update service interfaces/registration where needed.
- Add API contract and PostgreSQL integration tests.

## Tasks

1. Add anonymous `GET /api/product-stories/{slug}` with the existing `ApiResponse<T>` envelope.
2. Return a consistent 404 for unknown, unpublished, inactive-product, or inactive-coffee-type stories without exposing which condition failed.
3. Add Admin list/detail/create/update/publication endpoints under `/api/admin/product-stories` with `[Authorize(Roles = "Admin")]`.
4. Add an Admin active coffee-type list endpoint for the story dropdown. Keep P2 mutation endpoints separable.
5. Add pagination and filters (`keyword`, `productId`, `coffeeTypeId`, `isPublished`) to the Admin list; bound `pageSize` to 100.
6. Return 201 for create, 200 for reads/updates, and the existing error shape for validation/conflict/not-found cases.
7. Include product name, coffee name, stable slug, canonical URL, bilingual content, status, timestamps, and shared QR count in Admin DTOs.
8. Add OpenAPI-visible annotations/types where the current project pattern supports them.

## Tests

- Anonymous user reads a published story.
- Draft, unknown, inactive-product, and inactive-coffee-type cases all return the same public 404 shape.
- Admin can list, create, edit, publish, and unpublish.
- Customer, Staff, expired token, and anonymous callers cannot mutate stories.
- Duplicate `(ProductId, CoffeeTypeId)` returns conflict/validation error, not raw database details.
- API response JSON names and `ApiResponse<T>` shape remain stable.
- Query count and index usage for slug lookup are inspected against 10,000 stories.

## Verification

```bash
dotnet build EXE02_Backend_RE-CAFE.csproj --no-restore
dotnet test tests/EXE02_Backend_RE-CAFE.Tests/EXE02_Backend_RE-CAFE.Tests.csproj --filter ProductStoryApi
```
