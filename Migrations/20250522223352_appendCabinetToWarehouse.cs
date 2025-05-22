using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendCabinetToWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CabinetId",
                table: "Warehouses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_CabinetId",
                table: "Warehouses",
                column: "CabinetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Cabinets_CabinetId",
                table: "Warehouses",
                column: "CabinetId",
                principalTable: "Cabinets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Cabinets_CabinetId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_CabinetId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "CabinetId",
                table: "Warehouses");
        }
    }
}
