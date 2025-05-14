using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendOrderDTO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMCurrencyValue_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValue");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMCurrencyValues",
                table: "YMCurrencyValues",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "YMOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    BuyerItemsTotalBeforeDiscount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Currency = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeliveryTotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Fake = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ItemsTotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Substatus = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxSystem = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMOrders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YMOrderBuyers",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMOrderBuyers", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_YMOrderBuyers_YMOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "YMOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YMOrderDeliveries",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FromTime = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    ToTime = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    RealDeliveryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveryPartnerType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeliveryServiceId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispatchType = table.Column<int>(type: "int", nullable: true),
                    Country = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Street = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    House = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Apartment = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Postcode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LiftPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LiftType = table.Column<int>(type: "int", nullable: true),
                    Estimated = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(65,30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMOrderDeliveries", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_YMOrderDeliveries_YMOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "YMOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YMOrderItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OfferId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OfferName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Vat = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YMOrderItems_YMOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "YMOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_YMOrderItems_OrderId",
                table: "YMOrderItems",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_YMCurrencyValues_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValues",
                column: "YMSupplyRequestItemId",
                principalTable: "YMSupplyRequestItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMCurrencyValues_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValues");

            migrationBuilder.DropTable(
                name: "YMOrderBuyers");

            migrationBuilder.DropTable(
                name: "YMOrderDeliveries");

            migrationBuilder.DropTable(
                name: "YMOrderItems");

            migrationBuilder.DropTable(
                name: "YMOrders");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_YMCurrencyValue",
                table: "YMCurrencyValue",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YMCurrencyValue_YMSupplyRequestItems_YMSupplyRequestItemId",
                table: "YMCurrencyValue",
                column: "YMSupplyRequestItemId",
                principalTable: "YMSupplyRequestItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
