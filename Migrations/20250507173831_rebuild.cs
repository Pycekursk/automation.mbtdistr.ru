using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class rebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Workers_UserId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_UserId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "CurrentCabinetId",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "IsSuperEconom",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Returns");

            migrationBuilder.AddColumn<string>(
                name: "ReturnReason",
                table: "Returns",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "Returns");

            migrationBuilder.AddColumn<int>(
                name: "CurrentCabinetId",
                table: "Workers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperEconom",
                table: "Returns",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Returns",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Returns_UserId",
                table: "Returns",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Workers_UserId",
                table: "Returns",
                column: "UserId",
                principalTable: "Workers",
                principalColumn: "Id");
        }
    }
}
