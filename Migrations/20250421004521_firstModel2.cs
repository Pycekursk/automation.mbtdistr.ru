using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class firstModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Marketplace",
                table: "Cabinets",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Marketplace",
                table: "Cabinets");
        }
    }
}
