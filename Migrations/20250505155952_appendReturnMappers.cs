using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendReturnMappers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ReturnInfoId",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "OrderId",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ReturnMainInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "NmId",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "ReturnMainInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PostingNumber",
                table: "ReturnMainInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ReturnMainInfo",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProductSku",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Schema",
                table: "ReturnMainInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ReturnMainInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "NmId",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "PostingNumber",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "ProductSku",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "Schema",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ReturnMainInfo");

            migrationBuilder.AlterColumn<long>(
                name: "ReturnInfoId",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OrderId",
                table: "ReturnMainInfo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
