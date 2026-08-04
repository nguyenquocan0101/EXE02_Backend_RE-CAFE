# Phase 01: Database foundation

## Goal

Create the coffee-type catalog and safely reshape the existing traceability tables for multiple coffee stories per product without losing legacy data.

## Stories Covered

- P1: one product supports multiple coffee types.
- Foundation for every other P1 story.

## Backend Files

- Add `Models/CoffeeType.cs`.
- Update `Models/Product.cs`, `Models/ProductStory.cs`, and `Models/QRCode.cs`.
- Update `Data/ApplicationDbContext.cs`.
- Add one reviewed migration under `Migrations/` and update `ApplicationDbContextModelSnapshot.cs` through EF tooling.
- Add migration tests under `tests/EXE02_Backend_RE-CAFE.Tests/`.

## Tasks

1. Add `CoffeeType` with `Id`, `Name`, `Slug`, `IsActive`, `DisplayOrder`, timestamps, and story navigation.
2. Change `Product.ProductStory` to `Product.ProductStories`.
3. Add `CoffeeTypeId`, stable `Slug`, `ContentHtmlVi`, `ContentHtmlEn`, `IsPublished`, `CreatedAt`, and `UpdatedAt` to `ProductStory`.
4. Make `ProductionBatchId` optional. Retain `OriginStory`, `RecyclingProcess`, `SustainabilityMessage`, and `EstimatedWasteReducedGram` as nullable legacy/future fields.
5. Add `QRCode.IsShared`; change `ScanLimit` to nullable where `null` means unlimited.
6. Configure max lengths, delete behavior, unique coffee slug, unique story slug, unique `(ProductId, CoffeeTypeId)`, and indexes for public/admin query shapes.
7. Add a PostgreSQL partial unique index allowing only one `IsShared = true` QR per non-null story while preserving future per-unit rows.
8. Add a migration precondition that aborts with a clear message if `ProductStories` is not empty. Do not add speculative legacy backfill logic.
9. Seed exactly eight active coffee types using deterministic GUIDs.
10. Verify no generated migration drops the legacy narrative columns or existing product-level QR rows.
11. Generate after `20260727122159_SeedCustomerPaymentDemo`; preserve that existing migration byte-for-byte and verify the resulting migration order/model snapshot.
12. Limit coffee slugs to 80 characters, story slugs to 200, and stored QR/canonical URL values to 250 characters to match the existing `QRValue` column.

## Migration Tests

- Fresh database creates all tables, relationships, seeds, and indexes.
- Upgrade from the prior schema succeeds with zero stories.
- Upgrade with an unexpected legacy story fails before traceability schema mutation and reports the violated precondition.
- The database permits Arabica + Lamp and Robusta + Lamp but rejects duplicate Arabica + Lamp.
- Two shared QR rows for one story are rejected; non-shared legacy/future rows remain possible.
- `Down` is reviewed and tested only against a database without post-migration authored story content.

## Verification

```bash
dotnet ef migrations script <previous-migration> <new-migration>
dotnet test tests/EXE02_Backend_RE-CAFE.Tests/EXE02_Backend_RE-CAFE.Tests.csproj --filter TraceabilityMigration
```

Do not apply this migration to production until the preflight confirms `ProductStories` is empty and a backup exists.
