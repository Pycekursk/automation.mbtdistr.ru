using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendYMSupplyRequestsFix4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YMSupplyRequestLocationId",
                table: "YMSupplyRequestLocation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestLocation_YMSupplyRequestLocationId",
                table: "YMSupplyRequestLocation",
                column: "YMSupplyRequestLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestLocation_YMSupplyRequestLocation_YMSupplyRequ~",
                table: "YMSupplyRequestLocation",
                column: "YMSupplyRequestLocationId",
                principalTable: "YMSupplyRequestLocation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestLocation_YMSupplyRequestLocation_YMSupplyRequ~",
                table: "YMSupplyRequestLocation");

            migrationBuilder.DropIndex(
                name: "IX_YMSupplyRequestLocation_YMSupplyRequestLocationId",
                table: "YMSupplyRequestLocation");

            migrationBuilder.DropColumn(
                name: "YMSupplyRequestLocationId",
                table: "YMSupplyRequestLocation");
        }
    }
}
