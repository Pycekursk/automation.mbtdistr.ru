using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class YMSupplyRequestReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_YMSupplyRequests_YMSupplyRequestReference_ParentLinkId",
                table: "YMSupplyRequests",
                column: "ParentLinkId",
                principalTable: "YMSupplyRequestReference",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestReference_ParentLinkId",
                table: "YMSupplyRequests");

            migrationBuilder.DropTable(
                name: "YMSupplyRequestReference");

            migrationBuilder.DropIndex(
                name: "IX_YMSupplyRequests_ParentLinkId",
                table: "YMSupplyRequests");

            migrationBuilder.DropColumn(
                name: "ParentLinkId",
                table: "YMSupplyRequests");
        }
    }
}
