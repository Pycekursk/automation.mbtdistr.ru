using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendYMSupplyRequestsFix3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentLinkId",
                table: "YMSupplyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "YMSupplyRequestReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExternalIdId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    YMSupplyRequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequestReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequestReference_YMSupplyRequestId_ExternalIdId",
                        column: x => x.ExternalIdId,
                        principalTable: "YMSupplyRequestId",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequestReference_YMSupplyRequests_YMSupplyRequestId",
                        column: x => x.YMSupplyRequestId,
                        principalTable: "YMSupplyRequests",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequests_ParentLinkId",
                table: "YMSupplyRequests",
                column: "ParentLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReference_ExternalIdId",
                table: "YMSupplyRequestReference",
                column: "ExternalIdId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReference_YMSupplyRequestId",
                table: "YMSupplyRequestReference",
                column: "YMSupplyRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestReference_ParentLinkId",
                table: "YMSupplyRequests",
                column: "ParentLinkId",
                principalTable: "YMSupplyRequestReference",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
