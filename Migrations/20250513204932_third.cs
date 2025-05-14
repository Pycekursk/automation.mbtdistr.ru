using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YMSupplyRequestReference",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    RelatedRequestId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequestReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequestReference_YMSupplyRequests_RelatedRequestId",
                        column: x => x.RelatedRequestId,
                        principalTable: "YMSupplyRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequestReference_YMSupplyRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "YMSupplyRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReference_RelatedRequestId",
                table: "YMSupplyRequestReference",
                column: "RelatedRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReference_RequestId",
                table: "YMSupplyRequestReference",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YMSupplyRequestReference");
        }
    }
}
