using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EXE02_Backend_RE_CAFE.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialCatalogAndAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var seededAt = new DateTime(2026, 5, 24, 0, 0, 0, DateTimeKind.Utc);
            var adminId = Guid.Parse("F89E48B6-28FB-4D92-AF57-424CE0B887E2");
            var decorCategoryId = Guid.Parse("6B6B9A10-FB69-46B4-ABDF-2DE06FB909A3");
            var giftsCategoryId = Guid.Parse("E414FD58-C694-4363-B7F0-777A5B34EEA6");
            var limitedCategoryId = Guid.Parse("FBBA491D-58AA-49FD-BDB5-B0A590FEF8A2");

            var espressoDeskSetId = Guid.Parse("C3534E2E-3661-4A80-B89F-CF8E0BF42A06");
            var bloomWallClockId = Guid.Parse("059325EF-B7AB-4D11-9B42-29D6B1A765C8");
            var originCoastersId = Guid.Parse("370DBD62-0F0E-4FF6-B48B-B4B9F65ED40A");
            var aromaPlanterId = Guid.Parse("D734CD4A-634E-44AA-8AE4-E42B63EB0CCC");
            var vesselCandleId = Guid.Parse("90E4DDA3-EA2A-412E-84DC-7F4BCE0A6279");
            var artisanScoopId = Guid.Parse("D1C31A2A-2CA6-4C96-82CA-49913F0276D8");

            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[]
                {
                    "Id", "Username", "Email", "PasswordHash", "FullName",
                    "Phone", "IsActive", "CreatedAt", "UpdatedAt",
                    "Role", "TotalPoints", "Level", "Birthday"
                },
                values: new object[]
                {
                    adminId, "admin", "admin@recafe.local", adminPasswordHash, "System Admin",
                    "0900000000", true, seededAt, null,
                    2, 0, 0, null
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Slug", "Description", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { decorCategoryId, "Decor", "decor", "San pham trang tri tu ba ca phe tai che.", true, seededAt },
                    { giftsCategoryId, "Gifts", "gifts", "San pham qua tang thu cong va ben vung.", true, seededAt },
                    { limitedCategoryId, "Limited Edition", "limited-edition", "Dong san pham gioi han va ca nhan hoa.", true, seededAt }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[]
                {
                    "Id", "CategoryId", "Name", "Slug", "SKU", "Price", "SalePrice",
                    "ShortDescription", "Description", "Material", "Size", "UsageNote",
                    "IsPersonalizable", "IsActive", "RewardPoints", "CreatedAt", "UpdatedAt"
                },
                values: new object[,]
                {
                    {
                        espressoDeskSetId, decorCategoryId, "Bo khay ke Espresso", "espresso-desk-set", "RE-0001", 85m, null,
                        "Bo khay dung do van phong toi gian lam tu ba ca phe.",
                        "Bo khay dung do van phong lam bang ba ca phe, thiet ke toi gian. Handcrafted desk organizer set featuring a minimalist style.",
                        "Ba ca phe tai che", null, "Tag: Decor, Handmade | Collection: New Arrivals",
                        false, true, 85, seededAt, null
                    },
                    {
                        bloomWallClockId, decorCategoryId, "Dong ho treo tuong Bloom", "bloom-wall-clock", "RE-0002", 120m, null,
                        "Dong ho treo tuong kim troi tinh am tu chat lieu ben vung.",
                        "Dong ho treo tuong kim troi tinh am, thiet ke tu chat lieu ba ca phe. A silent statement wall clock designed with eco materials.",
                        "Ba ca phe tai che", null, "Tag: Decor, Eco-friendly | Collection: Best Sellers",
                        false, true, 120, seededAt, null
                    },
                    {
                        originCoastersId, giftsCategoryId, "Bo lot ly Origin (Bo 4 chiec)", "origin-coasters-set-of-4", "RE-0003", 45m, null,
                        "De lot ly khac laser sac net tu ba ca phe ep.",
                        "De lot ly khac laser sac net tu chat lieu ba ca phe ep. Precision laser-etched coasters for gifts and decor.",
                        "Ba ca phe tai che", null, "Tag: Gifts, Handmade | Collection: Best Sellers | Label: OUT OF STOCK",
                        false, false, 45, seededAt, null
                    },
                    {
                        aromaPlanterId, decorCategoryId, "Chau cay de ban Aroma", "aroma-table-planter", "RE-0004", 32m, null,
                        "Chau cay thoang khi, mang sac xanh tu nhien cho khong gian.",
                        "Chau cay ba ca phe thoang khi, mang sac xanh tu nhien cho can phong. Breathable eco-friendly planter for daily spaces.",
                        "Ba ca phe tai che", null, "Tag: Decor, Eco-friendly | Collection: New Arrivals",
                        false, true, 32, seededAt, null
                    },
                    {
                        vesselCandleId, giftsCategoryId, "Bo nen thom Vessel", "vessel-candle-set", "RE-0005", 58m, null,
                        "Bo nen sap dau nanh huong Arabica trong coc tai su dung.",
                        "Bo ba coc nen sap dau nanh cao cap phang phat huong Arabica. A trio of soy-wax candles for gift moments.",
                        "Ba ca phe tai che", null, "Tag: Gifts, Handmade | Collection: Best Sellers",
                        false, true, 58, seededAt, null
                    },
                    {
                        artisanScoopId, limitedCategoryId, "Thia dong dinh luong Artisan", "artisan-measuring-scoop", "RE-0006", 24m, null,
                        "Thia dong dinh luong khac can theo yeu cau.",
                        "Thia dong dinh luong bang go va ba ca phe, khac can theo yeu cau. Precision scoop with a custom engraved handle.",
                        "Go + ba ca phe tai che", null, "Tag: Handmade, Eco-friendly | Collection: Personalized | Label: PERSONALIZED",
                        true, true, 24, seededAt, null
                    }
                });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "ProductId", "ImageUrl", "IsThumbnail", "SortOrder" },
                values: new object[,]
                {
                    { Guid.Parse("F0BA2D93-CF76-4E9A-987F-D8973DBA315B"), espressoDeskSetId, "/assets/re_tray.png", true, 1 },
                    { Guid.Parse("3B16B718-00DB-4D7E-B883-17AD2A251B68"), bloomWallClockId, "/assets/bloom_clock.png", true, 1 },
                    { Guid.Parse("6C6B0D2D-4762-4CE4-9CA4-4F952C086A3D"), originCoastersId, "/assets/re_cup.png", true, 1 },
                    { Guid.Parse("E93D66A9-6ECB-4D8E-B7DD-D5251A22C82C"), aromaPlanterId, "/assets/re_vase.png", true, 1 },
                    { Guid.Parse("2D5D4851-4A26-4CBC-BD32-4A49D852261D"), vesselCandleId, "/assets/re_glow.png", true, 1 },
                    { Guid.Parse("26BEED86-11DF-4B8F-B5D9-3D401E5D55A0"), artisanScoopId, "/assets/re_cup.png", true, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    Guid.Parse("F0BA2D93-CF76-4E9A-987F-D8973DBA315B"),
                    Guid.Parse("3B16B718-00DB-4D7E-B883-17AD2A251B68"),
                    Guid.Parse("6C6B0D2D-4762-4CE4-9CA4-4F952C086A3D"),
                    Guid.Parse("E93D66A9-6ECB-4D8E-B7DD-D5251A22C82C"),
                    Guid.Parse("2D5D4851-4A26-4CBC-BD32-4A49D852261D"),
                    Guid.Parse("26BEED86-11DF-4B8F-B5D9-3D401E5D55A0")
                });

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    Guid.Parse("C3534E2E-3661-4A80-B89F-CF8E0BF42A06"),
                    Guid.Parse("059325EF-B7AB-4D11-9B42-29D6B1A765C8"),
                    Guid.Parse("370DBD62-0F0E-4FF6-B48B-B4B9F65ED40A"),
                    Guid.Parse("D734CD4A-634E-44AA-8AE4-E42B63EB0CCC"),
                    Guid.Parse("90E4DDA3-EA2A-412E-84DC-7F4BCE0A6279"),
                    Guid.Parse("D1C31A2A-2CA6-4C96-82CA-49913F0276D8")
                });

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    Guid.Parse("6B6B9A10-FB69-46B4-ABDF-2DE06FB909A3"),
                    Guid.Parse("E414FD58-C694-4363-B7F0-777A5B34EEA6"),
                    Guid.Parse("FBBA491D-58AA-49FD-BDB5-B0A590FEF8A2")
                });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: Guid.Parse("F89E48B6-28FB-4D92-AF57-424CE0B887E2"));
        }
    }
}
