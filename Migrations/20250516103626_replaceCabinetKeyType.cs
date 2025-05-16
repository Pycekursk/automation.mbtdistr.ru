using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class replaceCabinetKeyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
      //migrationBuilder.DropForeignKey(
      //    name: "FK_Returns_Cabinets_CabinetId1",
      //    table: "Returns");

      migrationBuilder.DropIndex(
                name: "IX_Returns_CabinetId1",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "CabinetId1",
                table: "Returns");

            migrationBuilder.AlterColumn<int>(
                name: "CabinetId",
                table: "Returns",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_CabinetId",
                table: "Returns",
                column: "CabinetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Cabinets_CabinetId",
                table: "Returns",
                column: "CabinetId",
                principalTable: "Cabinets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Cabinets_CabinetId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_CabinetId",
                table: "Returns");

            migrationBuilder.AlterColumn<string>(
                name: "CabinetId",
                table: "Returns",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CabinetId1",
                table: "Returns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Returns_CabinetId1",
                table: "Returns",
                column: "CabinetId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Cabinets_CabinetId1",
                table: "Returns",
                column: "CabinetId1",
                principalTable: "Cabinets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
