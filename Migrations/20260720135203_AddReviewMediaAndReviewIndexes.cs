using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EXE02_Backend_RE_CAFE.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewMediaAndReviewIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.CreateTable(
                name: "ReviewMedias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PublicId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewMedias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewMedias_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId_IsVisible_CreatedAt",
                table: "Reviews",
                columns: new[] { "ProductId", "IsVisible", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_OrderId_ProductId",
                table: "Reviews",
                columns: new[] { "UserId", "OrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewMedias_ReviewId",
                table: "ReviewMedias",
                column: "ReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewMedias");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId_IsVisible_CreatedAt",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_OrderId_ProductId",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");
        }
    }
}
