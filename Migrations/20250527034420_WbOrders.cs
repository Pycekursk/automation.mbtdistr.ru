using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class WbOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMCurrencyValues_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YMCurrencyValues",
                table: "YMCurrencyValues");

            migrationBuilder.RenameTable(
                name: "YMCurrencyValues",
                newName: "YMCurrencyValue");

            migrationBuilder.RenameIndex(
                name: "IX_YMCurrencyValues_YMSupplyRequestItemId",
                table: "YMCurrencyValue",
                newName: "IX_YMCurrencyValue_YMSupplyRequestItemId");

            migrationBuilder.AlterColumn<int>(
                name: "TaxSystem",
                table: "YMOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Substatus",
                table: "YMOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "YMOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentType",
                table: "YMOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "YMOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Currency",
                table: "YMOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Vat",
                table: "YMOrderItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "YMOrderDeliveries",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryPartnerType",
                table: "YMOrderDeliveries",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "YMOrderBuyers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMCurrencyValue",
                table: "YMCurrencyValue",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "WBOrders",
                columns: table => new
                {
                    Srid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastChangeDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    WarehouseName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WarehouseType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OblastOkrugName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SupplierArticle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NmId = table.Column<int>(type: "int", nullable: false),
                    Barcode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Brand = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TechSize = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IncomeID = table.Column<long>(type: "bigint", nullable: false),
                    IsSupply = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRealization = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    Spp = table.Column<int>(type: "int", nullable: false),
                    FinishedPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PriceWithDisc = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    IsCancel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Sticker = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WBOrders", x => x.Srid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_YMCurrencyValue_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValue",
                column: "YMSupplyRequestItemId",
                principalTable: "YMSupplyRequestItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMCurrencyValue_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValue");

            migrationBuilder.DropTable(
                name: "WBOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YMCurrencyValue",
                table: "YMCurrencyValue");

            migrationBuilder.RenameTable(
                name: "YMCurrencyValue",
                newName: "YMCurrencyValues");

            migrationBuilder.RenameIndex(
                name: "IX_YMCurrencyValue_YMSupplyRequestItemId",
                table: "YMCurrencyValues",
                newName: "IX_YMCurrencyValues_YMSupplyRequestItemId");

            migrationBuilder.AlterColumn<string>(
                name: "TaxSystem",
                table: "YMOrders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Substatus",
                table: "YMOrders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "YMOrders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentType",
                table: "YMOrders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "YMOrders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "YMOrders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Vat",
                table: "YMOrderItems",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "YMOrderDeliveries",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryPartnerType",
                table: "YMOrderDeliveries",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "YMOrderBuyers",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMCurrencyValues",
                table: "YMCurrencyValues",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMCurrencyValues_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValues",
                column: "YMSupplyRequestItemId",
                principalTable: "YMSupplyRequestItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
