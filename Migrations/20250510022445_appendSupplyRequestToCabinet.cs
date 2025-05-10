using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendSupplyRequestToCabinet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //    name: "CabinetId",
            //    table: "YMSupplyRequests",
            //    type: "int",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_YMSupplyRequests_CabinetId",
            //    table: "YMSupplyRequests",
            //    column: "CabinetId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_YMSupplyRequests_Cabinets_CabinetId",
            //    table: "YMSupplyRequests",
            //    column: "CabinetId",
            //    principalTable: "Cabinets",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_Cabinets_CabinetId",
                table: "YMSupplyRequests");

            migrationBuilder.DropIndex(
                name: "IX_YMSupplyRequests_CabinetId",
                table: "YMSupplyRequests");

            migrationBuilder.DropColumn(
                name: "CabinetId",
                table: "YMSupplyRequests");
        }
    }
}
