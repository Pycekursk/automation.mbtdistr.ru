using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class fixSuppliesEntity2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestReferences_YMSupplyRequests_RelatedRequestId",
                table: "YMSupplyRequestReferences");

            migrationBuilder.DropIndex(
                name: "IX_YMSupplyRequestReferences_RelatedRequestId",
                table: "YMSupplyRequestReferences");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReferences_RelatedRequestId",
                table: "YMSupplyRequestReferences",
                column: "RelatedRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestReferences_YMSupplyRequests_RelatedRequestId",
                table: "YMSupplyRequestReferences",
                column: "RelatedRequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestReferences_YMSupplyRequests_RelatedRequestId",
                table: "YMSupplyRequestReferences");

            migrationBuilder.DropIndex(
                name: "IX_YMSupplyRequestReferences_RelatedRequestId",
                table: "YMSupplyRequestReferences");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReferences_RelatedRequestId",
                table: "YMSupplyRequestReferences",
                column: "RelatedRequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestReferences_YMSupplyRequests_RelatedRequestId",
                table: "YMSupplyRequestReferences",
                column: "RelatedRequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
