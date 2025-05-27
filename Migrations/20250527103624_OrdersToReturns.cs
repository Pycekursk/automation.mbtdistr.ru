using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class OrdersToReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "Returns",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OrderExternalId",
                table: "Returns",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PostingNumber",
                table: "Returns",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReturnId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Returns_OrderId",
                table: "Returns",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReturnId",
                table: "Orders",
                column: "ReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Returns_ReturnId",
                table: "Orders",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Orders_OrderId",
                table: "Returns",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Returns_ReturnId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Orders_OrderId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_OrderId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ReturnId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderExternalId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "PostingNumber",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "Returns",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
