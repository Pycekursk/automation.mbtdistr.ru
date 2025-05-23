using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendSecondWarehouseToReturnObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Returns_Warehouses_WarehouseId",
            //    table: "Returns");

            migrationBuilder.AddColumn<int>(
                name: "CurrentWarehouseId",
                table: "Returns",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Returns_CurrentWarehouseId",
                table: "Returns",
                column: "CurrentWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_CurrentWarehouse",
                table: "Returns",
                column: "CurrentWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_DestinationWarehouse",
                table: "Returns",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Returns_CurrentWarehouse",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_DestinationWarehouse",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_CurrentWarehouseId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "CurrentWarehouseId",
                table: "Returns");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Warehouses_WarehouseId",
                table: "Returns",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }
    }
}
