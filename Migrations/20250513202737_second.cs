using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestItem_YMSupplyRequestItemCounters_CountersId",
                table: "YMSupplyRequestItem");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestItem_YMSupplyRequests_YMSupplyRequestId",
                table: "YMSupplyRequestItem");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests");

            migrationBuilder.DropIndex(
                name: "IX_YMSupplyRequestItem_CountersId",
                table: "YMSupplyRequestItem");

            migrationBuilder.DropIndex(
                name: "IX_YMSupplyRequestItem_YMSupplyRequestId",
                table: "YMSupplyRequestItem");

            migrationBuilder.DropColumn(
                name: "CountersId",
                table: "YMSupplyRequestItem");

            migrationBuilder.DropColumn(
                name: "YMSupplyRequestId",
                table: "YMSupplyRequestItem");

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
                name: "Id",
                table: "YMSupplyRequestItemCounters",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<long>(
                name: "SupplyRequestId",
                table: "YMSupplyRequestItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestItem_SupplyRequestId",
                table: "YMSupplyRequestItem",
                column: "SupplyRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestItem_YMSupplyRequests_SupplyRequestId",
                table: "YMSupplyRequestItem",
                column: "SupplyRequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestItemCounters_YMSupplyRequestItem_Id",
                table: "YMSupplyRequestItemCounters",
                column: "Id",
                principalTable: "YMSupplyRequestItem",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestItem_YMSupplyRequests_SupplyRequestId",
                table: "YMSupplyRequestItem");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestItemCounters_YMSupplyRequestItem_Id",
                table: "YMSupplyRequestItemCounters");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests");

            migrationBuilder.DropIndex(
                name: "IX_YMSupplyRequestItem_SupplyRequestId",
                table: "YMSupplyRequestItem");

            migrationBuilder.DropColumn(
                name: "SupplyRequestId",
                table: "YMSupplyRequestItem");

            migrationBuilder.AlterColumn<long>(
                name: "ExternalIdId",
                table: "YMSupplyRequests",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "YMSupplyRequestItemCounters",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "CountersId",
                table: "YMSupplyRequestItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "YMSupplyRequestId",
                table: "YMSupplyRequestItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestItem_CountersId",
                table: "YMSupplyRequestItem",
                column: "CountersId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestItem_YMSupplyRequestId",
                table: "YMSupplyRequestItem",
                column: "YMSupplyRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestItem_YMSupplyRequestItemCounters_CountersId",
                table: "YMSupplyRequestItem",
                column: "CountersId",
                principalTable: "YMSupplyRequestItemCounters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestItem_YMSupplyRequests_YMSupplyRequestId",
                table: "YMSupplyRequestItem",
                column: "YMSupplyRequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests",
                column: "ExternalIdId",
                principalTable: "YMSupplyRequestId",
                principalColumn: "Id");
        }
    }
}
