using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class removeReturnMainInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnMainInfo_Returns_ReturnId",
                table: "ReturnMainInfo");

            migrationBuilder.DropIndex(
                name: "IX_ReturnMainInfo_ReturnId",
                table: "ReturnMainInfo");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "ReturnMainInfo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReturnId",
                table: "ReturnMainInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnMainInfo_ReturnId",
                table: "ReturnMainInfo",
                column: "ReturnId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnMainInfo_Returns_ReturnId",
                table: "ReturnMainInfo",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
