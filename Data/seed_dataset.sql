BEGIN;

INSERT INTO "Users" (
    "Id", "Username", "Email", "PasswordHash", "FullName",
    "Phone", "IsActive", "CreatedAt", "UpdatedAt",
    "Role", "TotalPoints", "Level", "Birthday"
)
VALUES (
    'f89e48b6-28fb-4d92-af57-424ce0b887e2',
    'admin',
    'admin@recafe.local',
    '$2a$11$BE6icE/.kTEwMAAo9OM3wOQMmo700y.h0W5UWlU5eihfXvLsTC9mG',
    'System Admin',
    '0900000000',
    TRUE,
    '2026-05-24T00:00:00Z',
    NULL,
    2,
    0,
    0,
    NULL
)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Categories" ("Id", "Name", "Slug", "Description", "IsActive", "CreatedAt")
VALUES
    ('6b6b9a10-fb69-46b4-abdf-2de06fb909a3', 'Decor', 'decor', 'San pham trang tri tu ba ca phe tai che.', TRUE, '2026-05-24T00:00:00Z'),
    ('e414fd58-c694-4363-b7f0-777a5b34eea6', 'Gifts', 'gifts', 'San pham qua tang thu cong va ben vung.', TRUE, '2026-05-24T00:00:00Z'),
    ('fbba491d-58aa-49fd-bdb5-b0a590fef8a2', 'Limited Edition', 'limited-edition', 'Dong san pham gioi han va ca nhan hoa.', TRUE, '2026-05-24T00:00:00Z')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Products" (
    "Id", "CategoryId", "Name", "Slug", "SKU", "Price", "SalePrice",
    "ShortDescription", "Description", "Material", "Size", "UsageNote",
    "IsPersonalizable", "IsActive", "RewardPoints", "CreatedAt", "UpdatedAt"
)
VALUES
    ('c3534e2e-3661-4a80-b89f-cf8e0bf42a06', '6b6b9a10-fb69-46b4-abdf-2de06fb909a3', 'Bo khay ke Espresso', 'espresso-desk-set', 'RE-0001', 85, NULL, 'Bo khay dung do van phong toi gian lam tu ba ca phe.', 'Bo khay dung do van phong lam bang ba ca phe, thiet ke toi gian. Handcrafted desk organizer set featuring a minimalist style.', 'Ba ca phe tai che', NULL, 'Tag: Decor, Handmade | Collection: New Arrivals', FALSE, TRUE, 85, '2026-05-24T00:00:00Z', NULL),
    ('059325ef-b7ab-4d11-9b42-29d6b1a765c8', '6b6b9a10-fb69-46b4-abdf-2de06fb909a3', 'Dong ho treo tuong Bloom', 'bloom-wall-clock', 'RE-0002', 120, NULL, 'Dong ho treo tuong kim troi tinh am tu chat lieu ben vung.', 'Dong ho treo tuong kim troi tinh am, thiet ke tu chat lieu ba ca phe. A silent statement wall clock designed with eco materials.', 'Ba ca phe tai che', NULL, 'Tag: Decor, Eco-friendly | Collection: Best Sellers', FALSE, TRUE, 120, '2026-05-24T00:00:00Z', NULL),
    ('370dbd62-0f0e-4ff6-b48b-b4b9f65ed40a', 'e414fd58-c694-4363-b7f0-777a5b34eea6', 'Bo lot ly Origin (Bo 4 chiec)', 'origin-coasters-set-of-4', 'RE-0003', 45, NULL, 'De lot ly khac laser sac net tu ba ca phe ep.', 'De lot ly khac laser sac net tu chat lieu ba ca phe ep. Precision laser-etched coasters for gifts and decor.', 'Ba ca phe tai che', NULL, 'Tag: Gifts, Handmade | Collection: Best Sellers | Label: OUT OF STOCK', FALSE, FALSE, 45, '2026-05-24T00:00:00Z', NULL),
    ('d734cd4a-634e-44aa-8ae4-e42b63eb0ccc', '6b6b9a10-fb69-46b4-abdf-2de06fb909a3', 'Chau cay de ban Aroma', 'aroma-table-planter', 'RE-0004', 32, NULL, 'Chau cay thoang khi, mang sac xanh tu nhien cho khong gian.', 'Chau cay ba ca phe thoang khi, mang sac xanh tu nhien cho can phong. Breathable eco-friendly planter for daily spaces.', 'Ba ca phe tai che', NULL, 'Tag: Decor, Eco-friendly | Collection: New Arrivals', FALSE, TRUE, 32, '2026-05-24T00:00:00Z', NULL),
    ('90e4dda3-ea2a-412e-84dc-7f4bce0a6279', 'e414fd58-c694-4363-b7f0-777a5b34eea6', 'Bo nen thom Vessel', 'vessel-candle-set', 'RE-0005', 58, NULL, 'Bo nen sap dau nanh huong Arabica trong coc tai su dung.', 'Bo ba coc nen sap dau nanh cao cap phang phat huong Arabica. A trio of soy-wax candles for gift moments.', 'Ba ca phe tai che', NULL, 'Tag: Gifts, Handmade | Collection: Best Sellers', FALSE, TRUE, 58, '2026-05-24T00:00:00Z', NULL),
    ('d1c31a2a-2ca6-4c96-82ca-49913f0276d8', 'fbba491d-58aa-49fd-bdb5-b0a590fef8a2', 'Thia dong dinh luong Artisan', 'artisan-measuring-scoop', 'RE-0006', 24, NULL, 'Thia dong dinh luong khac can theo yeu cau.', 'Thia dong dinh luong bang go va ba ca phe, khac can theo yeu cau. Precision scoop with a custom engraved handle.', 'Go + ba ca phe tai che', NULL, 'Tag: Handmade, Eco-friendly | Collection: Personalized | Label: PERSONALIZED', TRUE, TRUE, 24, '2026-05-24T00:00:00Z', NULL)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "ProductImages" ("Id", "ProductId", "ImageUrl", "IsThumbnail", "SortOrder")
VALUES
    ('f0ba2d93-cf76-4e9a-987f-d8973dba315b', 'c3534e2e-3661-4a80-b89f-cf8e0bf42a06', '/assets/re_tray.png', TRUE, 1),
    ('3b16b718-00db-4d7e-b883-17ad2a251b68', '059325ef-b7ab-4d11-9b42-29d6b1a765c8', '/assets/bloom_clock.png', TRUE, 1),
    ('6c6b0d2d-4762-4ce4-9ca4-4f952c086a3d', '370dbd62-0f0e-4ff6-b48b-b4b9f65ed40a', '/assets/re_cup.png', TRUE, 1),
    ('e93d66a9-6ecb-4d8e-b7dd-d5251a22c82c', 'd734cd4a-634e-44aa-8ae4-e42b63eb0ccc', '/assets/re_vase.png', TRUE, 1),
    ('2d5d4851-4a26-4cbc-bd32-4a49d852261d', '90e4dda3-ea2a-412e-84dc-7f4bce0a6279', '/assets/re_glow.png', TRUE, 1),
    ('26beed86-11df-4b8f-b5d9-3d401e5d55a0', 'd1c31a2a-2ca6-4c96-82ca-49913f0276d8', '/assets/re_cup.png', TRUE, 1)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
