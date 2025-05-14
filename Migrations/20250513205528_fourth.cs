using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class fourth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMCurrencyValue_YMSupplyRequestItem_YMSupplyRequestItemId",
                table: "YMCurrencyValue");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestItem_YMSupplyRequests_SupplyRequestId",
                table: "YMSupplyRequestItem");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestItemCounters_YMSupplyRequestItem_Id",
                table: "YMSupplyRequestItemCounters");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestReference_YMSupplyRequests_RelatedRequestId",
                table: "YMSupplyRequestReference");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestReference_YMSupplyRequests_RequestId",
                table: "YMSupplyRequestReference");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YMSupplyRequestReference",
                table: "YMSupplyRequestReference");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YMSupplyRequestItem",
                table: "YMSupplyRequestItem");

            migrationBuilder.RenameTable(
                name: "YMSupplyRequestReference",
                newName: "YMSupplyRequestReferences");

            migrationBuilder.RenameTable(
                name: "YMSupplyRequestItem",
                newName: "YMSupplyRequestItems");

            migrationBuilder.RenameIndex(
                name: "IX_YMSupplyRequestReference_RequestId",
                table: "YMSupplyRequestReferences",
                newName: "IX_YMSupplyRequestReferences_RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_YMSupplyRequestReference_RelatedRequestId",
                table: "YMSupplyRequestReferences",
                newName: "IX_YMSupplyRequestReferences_RelatedRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_YMSupplyRequestItem_SupplyRequestId",
                table: "YMSupplyRequestItems",
                newName: "IX_YMSupplyRequestItems_SupplyRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMSupplyRequestReferences",
                table: "YMSupplyRequestReferences",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMSupplyRequestItems",
                table: "YMSupplyRequestItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMCurrencyValue_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValue",
                column: "YMSupplyRequestItemId",
                principalTable: "YMSupplyRequestItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestItemCounters_YMSupplyRequestItems_Id",
                table: "YMSupplyRequestItemCounters",
                column: "Id",
                principalTable: "YMSupplyRequestItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestItems_YMSupplyRequests_SupplyRequestId",
                table: "YMSupplyRequestItems",
                column: "SupplyRequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestReferences_YMSupplyRequests_RelatedRequestId",
                table: "YMSupplyRequestReferences",
                column: "RelatedRequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestReferences_YMSupplyRequests_RequestId",
                table: "YMSupplyRequestReferences",
                column: "RequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMCurrencyValue_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValue");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestItemCounters_YMSupplyRequestItems_Id",
                table: "YMSupplyRequestItemCounters");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestItems_YMSupplyRequests_SupplyRequestId",
                table: "YMSupplyRequestItems");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestReferences_YMSupplyRequests_RelatedRequestId",
                table: "YMSupplyRequestReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestReferences_YMSupplyRequests_RequestId",
                table: "YMSupplyRequestReferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YMSupplyRequestReferences",
                table: "YMSupplyRequestReferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YMSupplyRequestItems",
                table: "YMSupplyRequestItems");

            migrationBuilder.RenameTable(
                name: "YMSupplyRequestReferences",
                newName: "YMSupplyRequestReference");

            migrationBuilder.RenameTable(
                name: "YMSupplyRequestItems",
                newName: "YMSupplyRequestItem");

            migrationBuilder.RenameIndex(
                name: "IX_YMSupplyRequestReferences_RequestId",
                table: "YMSupplyRequestReference",
                newName: "IX_YMSupplyRequestReference_RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_YMSupplyRequestReferences_RelatedRequestId",
                table: "YMSupplyRequestReference",
                newName: "IX_YMSupplyRequestReference_RelatedRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_YMSupplyRequestItems_SupplyRequestId",
                table: "YMSupplyRequestItem",
                newName: "IX_YMSupplyRequestItem_SupplyRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMSupplyRequestReference",
                table: "YMSupplyRequestReference",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMSupplyRequestItem",
                table: "YMSupplyRequestItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMCurrencyValue_YMSupplyRequestItem_YMSupplyRequestItemId",
                table: "YMCurrencyValue",
                column: "YMSupplyRequestItemId",
                principalTable: "YMSupplyRequestItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_YMSupplyRequestReference_YMSupplyRequests_RelatedRequestId",
                table: "YMSupplyRequestReference",
                column: "RelatedRequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestReference_YMSupplyRequests_RequestId",
                table: "YMSupplyRequestReference",
                column: "RequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
