using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendSecondWarehouseToReturnObjectIdFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Returns",
                newName: "TargetWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Returns_WarehouseId",
                table: "Returns",
                newName: "IX_Returns_TargetWarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetWarehouseId",
                table: "Returns",
                newName: "WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Returns_TargetWarehouseId",
                table: "Returns",
                newName: "IX_Returns_WarehouseId");
        }
    }
}
