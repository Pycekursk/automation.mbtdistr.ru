using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class StoragePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Price_ReturnProducts_ReturnProductId",
                table: "Price");

            migrationBuilder.AddColumn<int>(
                name: "Storage_Days",
                table: "Returns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Storage_Price",
                table: "Returns",
                type: "double",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReturnProductId",
                table: "Price",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Price",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Price_ReturnProducts_ReturnProductId",
                table: "Price",
                column: "ReturnProductId",
                principalTable: "ReturnProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Price_ReturnProducts_ReturnProductId",
                table: "Price");

            migrationBuilder.DropColumn(
                name: "Storage_Days",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "Storage_Price",
                table: "Returns");

            migrationBuilder.AlterColumn<int>(
                name: "ReturnProductId",
                table: "Price",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Price",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Price_ReturnProducts_ReturnProductId",
                table: "Price",
                column: "ReturnProductId",
                principalTable: "ReturnProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
