using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class ProductsSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductSku",
                table: "ReturnMainInfo");

            migrationBuilder.AddColumn<string>(
                name: "ProductsSku",
                table: "ReturnMainInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductsSku",
                table: "ReturnMainInfo");

            migrationBuilder.AddColumn<long>(
                name: "ProductSku",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: true);
        }
    }
}
