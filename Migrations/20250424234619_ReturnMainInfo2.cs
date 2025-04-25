using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class ReturnMainInfo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Marketplace",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ReturnInfoId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ReturnReasonName",
                table: "Returns");

            migrationBuilder.AddColumn<long>(
                name: "OrderId",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ReturnInfoId",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ReturnReasonName",
                table: "ReturnMainInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "ReturnInfoId",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "ReturnReasonName",
                table: "ReturnMainInfo");

            migrationBuilder.AddColumn<string>(
                name: "Marketplace",
                table: "Returns",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "ReturnInfoId",
                table: "Returns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ReturnReasonName",
                table: "Returns",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
