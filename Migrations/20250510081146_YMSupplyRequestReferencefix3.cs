using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class YMSupplyRequestReferencefix3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
            //    table: "YMSupplyRequests");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_YMSupplyRequests_YMSupplyRequestReference_ParentLinkId",
            //    table: "YMSupplyRequests");

            //migrationBuilder.DropTable(
            //    name: "YMSupplyRequestReference");

            //migrationBuilder.DropIndex(
            //    name: "IX_YMSupplyRequests_ExternalIdId",
            //    table: "YMSupplyRequests");

            //migrationBuilder.DropIndex(
            //    name: "IX_YMSupplyRequests_ParentLinkId",
            //    table: "YMSupplyRequests");

            //migrationBuilder.DropColumn(
            //    name: "ExternalIdId",
            //    table: "YMSupplyRequests");

            //migrationBuilder.DropColumn(
            //    name: "ParentLinkId",
            //    table: "YMSupplyRequests");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "YMSupplyRequests",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_Id",
                table: "YMSupplyRequests",
                column: "Id",
                principalTable: "YMSupplyRequestId",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_Id",
                table: "YMSupplyRequests");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "YMSupplyRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<long>(
                name: "ExternalIdId",
                table: "YMSupplyRequests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentLinkId",
                table: "YMSupplyRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "YMSupplyRequestReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    YMSupplyRequestReferenceId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    YMSupplyRequestId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequestReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequestReference_YMSupplyRequestId_YMSupplyRequestRe~",
                        column: x => x.YMSupplyRequestReferenceId,
                        principalTable: "YMSupplyRequestId",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YMSupplyRequestReference_YMSupplyRequests_YMSupplyRequestId1",
                        column: x => x.YMSupplyRequestId1,
                        principalTable: "YMSupplyRequests",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequests_ExternalIdId",
                table: "YMSupplyRequests",
                column: "ExternalIdId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequests_ParentLinkId",
                table: "YMSupplyRequests",
                column: "ParentLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReference_YMSupplyRequestId1",
                table: "YMSupplyRequestReference",
                column: "YMSupplyRequestId1");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReference_YMSupplyRequestReferenceId",
                table: "YMSupplyRequestReference",
                column: "YMSupplyRequestReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests",
                column: "ExternalIdId",
                principalTable: "YMSupplyRequestId",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestReference_ParentLinkId",
                table: "YMSupplyRequests",
                column: "ParentLinkId",
                principalTable: "YMSupplyRequestReference",
                principalColumn: "Id");
        }
    }
}
