using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lif.Migrations
{
    /// <inheritdoc />
    public partial class AddKlantlogSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3a92224a-c376-4d3d-a387-e8241d455df5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ced5b17c-6633-44f0-9fbd-3079746a9fdb");

            migrationBuilder.CreateTable(
                name: "Klantlogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekNummer = table.Column<int>(type: "int", nullable: false),
                    Jaar = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DefinitiefGemaaktOp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DefinitiefGemaaktDoor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opmerkingen = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Klantlogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Klantlogs_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KlantlogProducten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlantlogId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Aantal = table.Column<int>(type: "int", nullable: false),
                    ToegevoegdOp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KlantlogProducten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KlantlogProducten_Klantlogs_KlantlogId",
                        column: x => x.KlantlogId,
                        principalTable: "Klantlogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KlantlogProducten_Producten_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Producten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KlantlogProducten_KlantlogId",
                table: "KlantlogProducten",
                column: "KlantlogId");

            migrationBuilder.CreateIndex(
                name: "IX_KlantlogProducten_ProductId",
                table: "KlantlogProducten",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Klantlogs_ApplicationUserId_WeekNummer_Jaar",
                table: "Klantlogs",
                columns: new[] { "ApplicationUserId", "WeekNummer", "Jaar" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KlantlogProducten");

            migrationBuilder.DropTable(
                name: "Klantlogs");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3a92224a-c376-4d3d-a387-e8241d455df5", null, "client", "client" },
                    { "ced5b17c-6633-44f0-9fbd-3079746a9fdb", null, "admin", "admin" }
                });
        }
    }
}
