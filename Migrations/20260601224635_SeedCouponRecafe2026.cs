using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EXE02_Backend_RE_CAFE.Migrations
{
    /// <inheritdoc />
    public partial class SeedCouponRecafe2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var couponId = new Guid("B4A7F5D2-0EAB-4A03-B8DB-9BBAD4A9D211");
            var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2027, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "Coupons",
                columns: new[]
                {
                    "Id",
                    "Code",
                    "Type",
                    "Value",
                    "Scope",
                    "MaxDiscountAmount",
                    "MinimumOrderAmount",
                    "UsageLimit",
                    "UsedCount",
                    "StartDate",
                    "EndDate",
                    "IsActive"
                },
                values: new object[]
                {
                    couponId,
                    "RECAFE2026",
                    0,
                    20m,
                    1,
                    30000m,
                    null,
                    0,
                    0,
                    startDate,
                    endDate,
                    true
                });

            migrationBuilder.Sql($@"
                INSERT INTO ""CouponProducts"" (""CouponId"", ""ProductId"")
                SELECT '{couponId}'::uuid, p.""Id""
                FROM ""Products"" p
                WHERE p.""IsActive"" = TRUE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var couponId = new Guid("B4A7F5D2-0EAB-4A03-B8DB-9BBAD4A9D211");

            migrationBuilder.Sql($@"
                DELETE FROM ""CouponProducts""
                WHERE ""CouponId"" = '{couponId}'::uuid;
            ");

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: couponId);
        }
    }
}
