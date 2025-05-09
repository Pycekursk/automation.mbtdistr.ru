using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendYMSupplyRequestsFix5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestLocation_YMSupplyRequestLocationAddress_Addre~",
                table: "YMSupplyRequestLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestLocation_YMSupplyRequestLocation_YMSupplyRequ~",
                table: "YMSupplyRequestLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestLocation_TargetLocationId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestLocation_TransitLocationId",
                table: "YMSupplyRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YMSupplyRequestLocation",
                table: "YMSupplyRequestLocation");

            migrationBuilder.RenameTable(
                name: "YMSupplyRequestLocation",
                newName: "YMLocations");

            migrationBuilder.RenameIndex(
                name: "IX_YMSupplyRequestLocation_YMSupplyRequestLocationId",
                table: "YMLocations",
                newName: "IX_YMLocations_YMSupplyRequestLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_YMSupplyRequestLocation_AddressId",
                table: "YMLocations",
                newName: "IX_YMLocations_AddressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMLocations",
                table: "YMLocations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMLocations_YMLocations_YMSupplyRequestLocationId",
                table: "YMLocations",
                column: "YMSupplyRequestLocationId",
                principalTable: "YMLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMLocations_YMSupplyRequestLocationAddress_AddressId",
                table: "YMLocations",
                column: "AddressId",
                principalTable: "YMSupplyRequestLocationAddress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TargetLocationId",
                table: "YMSupplyRequests",
                column: "TargetLocationId",
                principalTable: "YMLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TransitLocationId",
                table: "YMSupplyRequests",
                column: "TransitLocationId",
                principalTable: "YMLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMLocations_YMLocations_YMSupplyRequestLocationId",
                table: "YMLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_YMLocations_YMSupplyRequestLocationAddress_AddressId",
                table: "YMLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TargetLocationId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TransitLocationId",
                table: "YMSupplyRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YMLocations",
                table: "YMLocations");

            migrationBuilder.RenameTable(
                name: "YMLocations",
                newName: "YMSupplyRequestLocation");

            migrationBuilder.RenameIndex(
                name: "IX_YMLocations_YMSupplyRequestLocationId",
                table: "YMSupplyRequestLocation",
                newName: "IX_YMSupplyRequestLocation_YMSupplyRequestLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_YMLocations_AddressId",
                table: "YMSupplyRequestLocation",
                newName: "IX_YMSupplyRequestLocation_AddressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMSupplyRequestLocation",
                table: "YMSupplyRequestLocation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestLocation_YMSupplyRequestLocationAddress_Addre~",
                table: "YMSupplyRequestLocation",
                column: "AddressId",
                principalTable: "YMSupplyRequestLocationAddress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestLocation_YMSupplyRequestLocation_YMSupplyRequ~",
                table: "YMSupplyRequestLocation",
                column: "YMSupplyRequestLocationId",
                principalTable: "YMSupplyRequestLocation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestLocation_TargetLocationId",
                table: "YMSupplyRequests",
                column: "TargetLocationId",
                principalTable: "YMSupplyRequestLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestLocation_TransitLocationId",
                table: "YMSupplyRequests",
                column: "TransitLocationId",
                principalTable: "YMSupplyRequestLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
