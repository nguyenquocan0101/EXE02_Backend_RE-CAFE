using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EXE02_Backend_RE_CAFE.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCustomizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCustomizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SourceImagePublicId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PreviewImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResultModelUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResultModelPublicId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsMockResult = table.Column<bool>(type: "boolean", nullable: false),
                    PositionX = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    PositionY = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    PositionZ = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    RotationX = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    RotationY = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    RotationZ = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    Scale = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    EngraveDepth = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCustomizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCustomizations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductCustomizations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustomizations_CreatedAt",
                table: "ProductCustomizations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustomizations_ProductId",
                table: "ProductCustomizations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustomizations_Status",
                table: "ProductCustomizations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustomizations_UserId",
                table: "ProductCustomizations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCustomizations");
        }
    }
}
