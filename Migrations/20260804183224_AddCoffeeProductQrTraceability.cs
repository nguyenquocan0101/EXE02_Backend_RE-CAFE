using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EXE02_Backend_RE_CAFE.Migrations
{
    /// <inheritdoc />
    public partial class AddCoffeeProductQrTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "ProductStories") THEN
                        RAISE EXCEPTION 'Coffee product QR traceability migration requires an empty ProductStories table';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_QRCodes_ProductStoryId",
                table: "QRCodes");

            migrationBuilder.DropIndex(
                name: "IX_ProductStories_ProductId",
                table: "ProductStories");

            migrationBuilder.AlterColumn<int>(
                name: "ScanLimit",
                table: "QRCodes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "QRCodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "SustainabilityMessage",
                table: "ProductStories",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "RecyclingProcess",
                table: "ProductStories",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductionBatchId",
                table: "ProductStories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "OriginStory",
                table: "ProductStories",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedWasteReducedGram",
                table: "ProductStories",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "CoffeeTypeId",
                table: "ProductStories",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "ContentHtmlEn",
                table: "ProductStories",
                type: "character varying(50000)",
                maxLength: 50000,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "ContentHtmlVi",
                table: "ProductStories",
                type: "character varying(50000)",
                maxLength: 50000,
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductStories",
                type: "timestamp with time zone",
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "ProductStories",
                type: "boolean",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "ProductStories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProductStories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoffeeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CoffeeTypes",
                columns: new[] { "Id", "CreatedAt", "DisplayOrder", "IsActive", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("3f6b7b8a-4c0b-4d8b-8f6c-000000000001"), new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, "Arabica", "arabica", null },
                    { new Guid("3f6b7b8a-4c0b-4d8b-8f6c-000000000002"), new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "Robusta", "robusta", null },
                    { new Guid("3f6b7b8a-4c0b-4d8b-8f6c-000000000003"), new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "Liberica", "liberica", null },
                    { new Guid("3f6b7b8a-4c0b-4d8b-8f6c-000000000004"), new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Utc), 4, true, "Excelsa", "excelsa", null },
                    { new Guid("3f6b7b8a-4c0b-4d8b-8f6c-000000000005"), new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Utc), 5, true, "Culi", "culi", null },
                    { new Guid("3f6b7b8a-4c0b-4d8b-8f6c-000000000006"), new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Utc), 6, true, "Moka", "moka", null },
                    { new Guid("3f6b7b8a-4c0b-4d8b-8f6c-000000000007"), new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Utc), 7, true, "Catimor", "catimor", null },
                    { new Guid("3f6b7b8a-4c0b-4d8b-8f6c-000000000008"), new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Utc), 8, true, "Blend", "blend", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_ProductStoryId",
                table: "QRCodes",
                column: "ProductStoryId",
                unique: true,
                filter: "\"IsShared\" = TRUE AND \"ProductStoryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStories_CoffeeTypeId",
                table: "ProductStories",
                column: "CoffeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStories_ProductId_CoffeeTypeId",
                table: "ProductStories",
                columns: new[] { "ProductId", "CoffeeTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStories_Slug",
                table: "ProductStories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeTypes_IsActive_DisplayOrder",
                table: "CoffeeTypes",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeTypes_Slug",
                table: "CoffeeTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStories_CoffeeTypes_CoffeeTypeId",
                table: "ProductStories",
                column: "CoffeeTypeId",
                principalTable: "CoffeeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductStories_CoffeeTypes_CoffeeTypeId",
                table: "ProductStories");

            migrationBuilder.DropTable(
                name: "CoffeeTypes");

            migrationBuilder.DropIndex(
                name: "IX_QRCodes_ProductStoryId",
                table: "QRCodes");

            migrationBuilder.DropIndex(
                name: "IX_ProductStories_CoffeeTypeId",
                table: "ProductStories");

            migrationBuilder.DropIndex(
                name: "IX_ProductStories_ProductId_CoffeeTypeId",
                table: "ProductStories");

            migrationBuilder.DropIndex(
                name: "IX_ProductStories_Slug",
                table: "ProductStories");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "QRCodes");

            migrationBuilder.DropColumn(
                name: "CoffeeTypeId",
                table: "ProductStories");

            migrationBuilder.DropColumn(
                name: "ContentHtmlEn",
                table: "ProductStories");

            migrationBuilder.DropColumn(
                name: "ContentHtmlVi",
                table: "ProductStories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProductStories");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "ProductStories");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "ProductStories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProductStories");

            migrationBuilder.AlterColumn<int>(
                name: "ScanLimit",
                table: "QRCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SustainabilityMessage",
                table: "ProductStories",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RecyclingProcess",
                table: "ProductStories",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductionBatchId",
                table: "ProductStories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OriginStory",
                table: "ProductStories",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedWasteReducedGram",
                table: "ProductStories",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_ProductStoryId",
                table: "QRCodes",
                column: "ProductStoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStories_ProductId",
                table: "ProductStories",
                column: "ProductId",
                unique: true);
        }
    }
}
