using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace automation.mbtdistr.ru.Migrations
{
    /// <inheritdoc />
    public partial class appendYMSupplyRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YMSupplyRequestCounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlanCount = table.Column<int>(type: "int", nullable: false),
                    FactCount = table.Column<int>(type: "int", nullable: false),
                    DefectCount = table.Column<int>(type: "int", nullable: false),
                    UndefinedCount = table.Column<int>(type: "int", nullable: false),
                    ActualBoxCount = table.Column<int>(type: "int", nullable: false),
                    ActualPalletsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequestCounters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YMSupplyRequestId",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MarketplaceRequestId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WarehouseRequestId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequestId", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YMSupplyRequestLocationAddress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FullAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gps_Latitude = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Gps_Longitude = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequestLocationAddress", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YMSupplyRequestLocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequestLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequestLocation_YMSupplyRequestLocationAddress_Addre~",
                        column: x => x.AddressId,
                        principalTable: "YMSupplyRequestLocationAddress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YMSupplyRequestReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExternalIdId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    YMSupplyRequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequestReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequestReference_YMSupplyRequestId_ExternalIdId",
                        column: x => x.ExternalIdId,
                        principalTable: "YMSupplyRequestId",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YMSupplyRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CountersId = table.Column<int>(type: "int", nullable: false),
                    ExternalIdId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Subtype = table.Column<int>(type: "int", nullable: false),
                    TargetLocationId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ParentLinkId = table.Column<int>(type: "int", nullable: false),
                    TransitLocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YMSupplyRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequests_YMSupplyRequestCounters_CountersId",
                        column: x => x.CountersId,
                        principalTable: "YMSupplyRequestCounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                        column: x => x.ExternalIdId,
                        principalTable: "YMSupplyRequestId",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequests_YMSupplyRequestLocation_TargetLocationId",
                        column: x => x.TargetLocationId,
                        principalTable: "YMSupplyRequestLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequests_YMSupplyRequestLocation_TransitLocationId",
                        column: x => x.TransitLocationId,
                        principalTable: "YMSupplyRequestLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YMSupplyRequests_YMSupplyRequestReference_ParentLinkId",
                        column: x => x.ParentLinkId,
                        principalTable: "YMSupplyRequestReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestLocation_AddressId",
                table: "YMSupplyRequestLocation",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReference_ExternalIdId",
                table: "YMSupplyRequestReference",
                column: "ExternalIdId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequestReference_YMSupplyRequestId",
                table: "YMSupplyRequestReference",
                column: "YMSupplyRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequests_CountersId",
                table: "YMSupplyRequests",
                column: "CountersId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequests_ExternalIdId",
                table: "YMSupplyRequests",
                column: "ExternalIdId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequests_ParentLinkId",
                table: "YMSupplyRequests",
                column: "ParentLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequests_TargetLocationId",
                table: "YMSupplyRequests",
                column: "TargetLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_YMSupplyRequests_TransitLocationId",
                table: "YMSupplyRequests",
                column: "TransitLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_YMSupplyRequestReference_YMSupplyRequests_YMSupplyRequestId",
                table: "YMSupplyRequestReference",
                column: "YMSupplyRequestId",
                principalTable: "YMSupplyRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestLocation_YMSupplyRequestLocationAddress_Addre~",
                table: "YMSupplyRequestLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestReference_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequestReference");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequests_YMSupplyRequestId_ExternalIdId",
                table: "YMSupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_YMSupplyRequestReference_YMSupplyRequests_YMSupplyRequestId",
                table: "YMSupplyRequestReference");

            migrationBuilder.DropTable(
                name: "YMSupplyRequestLocationAddress");

            migrationBuilder.DropTable(
                name: "YMSupplyRequestId");

            migrationBuilder.DropTable(
                name: "YMSupplyRequests");

            migrationBuilder.DropTable(
                name: "YMSupplyRequestCounters");

            migrationBuilder.DropTable(
                name: "YMSupplyRequestLocation");

            migrationBuilder.DropTable(
                name: "YMSupplyRequestReference");
        }
    }
}
