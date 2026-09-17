using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tailly.BookingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialistName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SpecialistSlug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PriceUnit = table.Column<int>(type: "integer", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanceledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOrderReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Photos = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'{}'"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrderReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceOrderReviews_ServiceOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOrderServiceSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceUnit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrderServiceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceOrderServiceSnapshots_ServiceOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderReviews_OrderId",
                table: "ServiceOrderReviews",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ClientId",
                table: "ServiceOrders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_CreatedAt",
                table: "ServiceOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_Number",
                table: "ServiceOrders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_SpecialistId",
                table: "ServiceOrders",
                column: "SpecialistId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_Status",
                table: "ServiceOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderServiceSnapshots_OrderId",
                table: "ServiceOrderServiceSnapshots",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceOrderReviews");

            migrationBuilder.DropTable(
                name: "ServiceOrderServiceSnapshots");

            migrationBuilder.DropTable(
                name: "ServiceOrders");
        }
    }
}
