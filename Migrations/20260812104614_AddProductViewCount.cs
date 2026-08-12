using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EXE02_Backend_RE_CAFE.Migrations
{
    /// <inheritdoc />
    public partial class AddProductViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "Products"
                SET "ViewCount" = CASE "SKU"
                    WHEN 'RE-0001' THEN 125
                    WHEN 'RE-0002' THEN 178
                    ELSE "ViewCount"
                END
                WHERE "SKU" IN ('RE-0001', 'RE-0002');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Products");
        }
    }
}
