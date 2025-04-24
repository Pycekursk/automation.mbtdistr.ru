using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class upgradeModels3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentCabinetId",
                table: "Workers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCabinetId",
                table: "Workers");
        }
    }
}
