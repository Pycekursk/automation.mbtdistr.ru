using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class newUpdateObject3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
      migrationBuilder.DropForeignKey(
          name: "FK_Returns_Cabinets_CabinetId",
          table: "Returns");

      migrationBuilder.DropIndex(
          name: "IX_Returns_CabinetId",
          table: "Returns");

      migrationBuilder.AlterColumn<DateTime>(
          name: "OrderedAt",
          table: "Returns",
          type: "datetime(6)",
          nullable: true,
          oldClrType: typeof(DateTime),
          oldType: "datetime(6)");

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

      migrationBuilder.AddColumn<string>(
          name: "OrderId",
          table: "Returns",
          type: "longtext",
          nullable: true)
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.AddColumn<string>(
          name: "OrderNumber",
          table: "Returns",
          type: "longtext",
          nullable: true)
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.AddColumn<string>(
          name: "ReturnId",
          table: "Returns",
          type: "longtext",
          nullable: true)
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.AddColumn<int>(
          name: "ReturnType",
          table: "Returns",
          type: "int",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.AddColumn<int>(
          name: "Scheme",
          table: "Returns",
          type: "int",
          nullable: true);

      migrationBuilder.AddColumn<int>(
          name: "WarehouseId",
          table: "Returns",
          type: "int",
          nullable: true);

      migrationBuilder.CreateTable(
          name: "ReturnProducts",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            Name = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            Sku = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            OfferId = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            ReturnId = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ReturnProducts", x => x.Id);
            table.ForeignKey(
                      name: "FK_ReturnProducts_Returns_ReturnId",
                      column: x => x.ReturnId,
                      principalTable: "Returns",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          })
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.CreateTable(
          name: "Warehouses",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            ExternalId = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            Name = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            Phone = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4")
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Warehouses", x => x.Id);
          })
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.CreateTable(
          name: "ReturnImages",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            ReturnProductId = table.Column<int>(type: "int", nullable: false),
            Url = table.Column<string>(type: "longtext", nullable: false)
                  .Annotation("MySql:CharSet", "utf8mb4")
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ReturnImages", x => x.Id);
            table.ForeignKey(
                      name: "FK_ReturnImages_ReturnProducts_ReturnProductId",
                      column: x => x.ReturnProductId,
                      principalTable: "ReturnProducts",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          })
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.CreateTable(
          name: "Address",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            Country = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            City = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            Street = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            House = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            Office = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            ZipCode = table.Column<string>(type: "longtext", nullable: true)
                  .Annotation("MySql:CharSet", "utf8mb4"),
            Latitude = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
            Longitude = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
            WarehouseId = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Address", x => x.Id);
            table.ForeignKey(
                      name: "FK_Address_Warehouses_WarehouseId",
                      column: x => x.WarehouseId,
                      principalTable: "Warehouses",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          })
          .Annotation("MySql:CharSet", "utf8mb4");

      migrationBuilder.CreateIndex(
          name: "IX_Returns_CabinetId1",
          table: "Returns",
          column: "CabinetId1");

      migrationBuilder.CreateIndex(
          name: "IX_Returns_WarehouseId",
          table: "Returns",
          column: "WarehouseId");

      migrationBuilder.CreateIndex(
          name: "IX_Address_WarehouseId",
          table: "Address",
          column: "WarehouseId",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_ReturnImages_ReturnProductId",
          table: "ReturnImages",
          column: "ReturnProductId");

      migrationBuilder.CreateIndex(
          name: "IX_ReturnProducts_ReturnId",
          table: "ReturnProducts",
          column: "ReturnId");

      migrationBuilder.AddForeignKey(
          name: "FK_Returns_Cabinets_CabinetId1",
          table: "Returns",
          column: "CabinetId1",
          principalTable: "Cabinets",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);

      migrationBuilder.AddForeignKey(
          name: "FK_Returns_Warehouses_WarehouseId",
          table: "Returns",
          column: "WarehouseId",
          principalTable: "Warehouses",
          principalColumn: "Id");
    }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Cabinets_CabinetId1",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Warehouses_WarehouseId",
                table: "Returns");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "ReturnImages");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "ReturnProducts");

            migrationBuilder.DropIndex(
                name: "IX_Returns_CabinetId1",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_WarehouseId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "CabinetId1",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ReturnType",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "Scheme",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Returns");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OrderedAt",
                table: "Returns",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

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
    }
}
