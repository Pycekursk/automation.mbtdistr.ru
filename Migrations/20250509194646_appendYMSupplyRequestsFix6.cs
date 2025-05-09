using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendYMSupplyRequestsFix6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TargetLocationId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TransitLocationId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestCounters_CountersId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests");

            migrationBuilder.AlterColumn<int>(
                name: "TransitLocationId",
                table: "YMSupplyRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "TargetLocationId",
                table: "YMSupplyRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "ExternalIdId",
                table: "YMSupplyRequests",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "CountersId",
                table: "YMSupplyRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TargetLocationId",
                table: "YMSupplyRequests",
                column: "TargetLocationId",
                principalTable: "YMLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TransitLocationId",
                table: "YMSupplyRequests",
                column: "TransitLocationId",
                principalTable: "YMLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestCounters_CountersId",
                table: "YMSupplyRequests",
                column: "CountersId",
                principalTable: "YMSupplyRequestCounters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests",
                column: "ExternalIdId",
                principalTable: "YMSupplyRequestId",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TargetLocationId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMLocations_TransitLocationId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestCounters_CountersId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests");

            migrationBuilder.AlterColumn<int>(
                name: "TransitLocationId",
                table: "YMSupplyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TargetLocationId",
                table: "YMSupplyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ExternalIdId",
                table: "YMSupplyRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CountersId",
                table: "YMSupplyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestCounters_CountersId",
                table: "YMSupplyRequests",
                column: "CountersId",
                principalTable: "YMSupplyRequestCounters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests",
                column: "ExternalIdId",
                principalTable: "YMSupplyRequestId",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
